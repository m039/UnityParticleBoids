using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using m039.Common;
using System;

namespace GP4
{

    public class LivingEntityDrawMeshSpawner : BaseSpawner
    {
        static public readonly float ReferenceScaleMagnitude = new Vector2(1, 1).magnitude;

        public const float AlphaFadeOutSpeed = 0.56f;

        #region Inspector

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        [Range(0, 1f)]
        public float entetiesReferenceAlpha = 1f;

        public bool useGizmos = true;

        #endregion

        LivingEntitySimulation _simulation;

        Mesh _mesh;

        Material _material;

        MaterialPropertyBlock _propertyBlock;

        static readonly int ColorId = Shader.PropertyToID("_Color");

        protected override void OnEnable()
        {
            base.OnEnable();

            InitRenderData();

            if (_simulation == null)
            {
                _simulation = new LivingEntitySimulation()
                {
                    entetiesReferenceScale = () => entetiesReferenceScale,
                    entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                    entetiesReferenceSpeed = () => entetiesReferenceSpeed
                };
            }
        }

        void InitRenderData()
        {
            var entityData = Context.LivingEntityConfig.GetData();
            var sprite = entityData.sprite;

            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.vertices = sprite.vertices.Select(v => (Vector3)v).ToArray();
            _mesh.triangles = sprite.triangles.Select(t => (int)t).ToArray();
            _mesh.uv = sprite.uv;
            _mesh.colors = Enumerable.Repeat(Color.white, _mesh.vertices.Length).ToArray();

            _material = new Material(Shader.Find("Unlit/SimpleSprite"));
            _material.enableInstancing = true;
            _material.mainTexture = sprite.texture;
            _material.color = Color.white;

            _propertyBlock = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            _simulation.Update(Context.LivingEntityConfig);
            _simulation.Populate(numberOfEntities, Context.LivingEntityConfig);
            DrawEnteties();
        }

        void DrawEnteties()
        {
            var camera = Camera.main;

            foreach (var data in _simulation.Enteties)
            {
                _propertyBlock.SetColor(ColorId, data.Color);
                Graphics.DrawMesh(
                    _mesh,
                    Matrix4x4.TRS(data.position, Quaternion.AngleAxis(data.rotation, Vector3.forward), data.scale * data.scaleFactor),
                    _material,
                    0,
                    camera,
                    0,
                    _propertyBlock,
                    false,
                    false,
                    false
                    );
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            _simulation.DrawGizmos();
        }

        public override void OnSpawnerSelected()
        {
        }

        public override void OnSpawnerDeselected()
        {
            _simulation.Reset();
        }

        protected override void OnLivingEntityDataChanged()
        {
            base.OnLivingEntityDataChanged();

            InitRenderData();
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawStatFrame(4);
            drawer.DrawStat(0, "Entities: " + _simulation.Enteties.Count);
            drawer.DrawStat(1, "Global Scale: " + entetiesReferenceScale);
            drawer.DrawStat(2, "Global Alpha: " + entetiesReferenceAlpha);
            drawer.DrawStat(3, "Global Speed: " + entetiesReferenceSpeed);

            drawer.DrawName("Draw each particle with Graphics.DrawMesh");

            drawer.DrawGetNumber("Number of Enteties [" + numberOfEntities + "]:", ref numberOfEntities);
        }

        public class LivingEntitySimulation
        {
            class LivingEntityDataComparer : IComparer<LivingEntityData>
            {
                public int Compare(LivingEntityData x, LivingEntityData y)
                {
                    if (x == null && y != null)
                        return -1;

                    if (x != null && y == null)
                        return 1;

                    if (x == null && y == null)
                        return 0;

                    var compare = x.layer.CompareTo(y.layer) ;
                    if (compare == 0)
                    {
                        return x.__id.CompareTo(y.__id);
                    } else
                    {
                        return compare;
                    }
                }
            }

            readonly static LivingEntityDataComparer _sEntetiesComparer = new LivingEntityDataComparer();

            readonly List<LivingEntityData> _enteties = new List<LivingEntityData>();

            public GetSettingValue<float> entetiesReferenceScale = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceAlpha = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceSpeed = () => 1.0f;

            public GetSettingValue<Bounds> sceneBounds => () => GameScene.Instance.SceneBounds;

            public List<LivingEntityData> Enteties => _enteties;

            public void Populate(int numberOfEntities, BaseLivingEntityConfig entetyConfig)
            {

                while (_enteties.Count < numberOfEntities)
                {
                    LivingEntityData entityData = null;
                    CreateOrReinitLivingEntityData(entetyConfig, ref entityData);
                    _enteties.Add(entityData);
                }
            }

            void CreateOrReinitLivingEntityData(BaseLivingEntityConfig entetyConfig, ref LivingEntityData entityData)
            {
                Vector3 getPosition(Vector2 normalizedPosition)
                {
                    var sb = sceneBounds();
                    var center = sb.center;
                    var size = sb.size;
                    size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                    return center + size;
                }

                if (entityData == null)
                {
                    entityData = new LivingEntityData();
                }               

                var initData = entetyConfig.GetData();

                entityData.position = getPosition(initData.position);
                entityData.rotation = initData.rotation;
                entityData.scale = initData.scale;
                entityData.speed = initData.speed;
                entityData.baseColor = initData.color;
                entityData.layer = initData.layer;
                entityData.radius = initData.radius;
                entityData.alpha = 0f;
            }

            public void Update(BaseLivingEntityConfig entityConfig)
            {
                var tSceneBounds = sceneBounds();
                var deltaTime = Time.deltaTime;
                var refenceScale = entetiesReferenceScale();
                var referenceAlpha = entetiesReferenceAlpha();
                var referenceSpeed = entetiesReferenceSpeed();

                bool updateEntity(LivingEntityData data)
                {
                    var rotation = Quaternion.AngleAxis(data.rotation, Vector3.forward);
                    var deltaPosition = (Vector2)(rotation * Vector3.up * (data.speed * referenceSpeed) * deltaTime);

                    data.position += deltaPosition;
                    data.scaleFactor = refenceScale;
                    data.alphaFactor = referenceAlpha;

                    if (data.alpha < 1)
                    {
                        data.alpha = Mathf.Clamp(data.alpha + deltaTime * AlphaFadeOutSpeed, 0, 1);
                    }

                    var boundRadius = data.scaleFactor * data.radius;

                    return Physics2DUtils.CircleWithin(tSceneBounds, data.position, boundRadius);
                }

                for (int i = 0; i < _enteties.Count; i++)
                {
                    var entityData = _enteties[i];

                    if (!updateEntity(entityData)) {
                        CreateOrReinitLivingEntityData(entityConfig, ref entityData);
                    }
                }

                _enteties.Sort(_sEntetiesComparer);
            }

            public void DrawGizmos()
            {
                var tSceneBounds = sceneBounds();

                // Draw direction

                foreach (var data in Enteties)
                {
                    var boundRadius = data.scaleFactor * data.radius;

                    // Draw a circle around the entity.

                    if (Physics2DUtils.CircleWithin(tSceneBounds, data.position, boundRadius))
                    {
                        Gizmos.color = Color.blue.WithAlpha(0.5f * data.alpha);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow.WithAlpha(0.5f * data.alpha);
                    }

                    Gizmos.DrawWireSphere(data.position, boundRadius);

                    // Draw the direction.

                    Gizmos.color = Color.green;

                    Gizmos.DrawLine(data.position, (Vector3) data.position + Quaternion.AngleAxis(data.rotation, Vector3.forward) * Vector3.up * boundRadius);
                }
            }

            public void Reset()
            {
                _enteties.Clear();
            }
        }

        public class LivingEntityData
        {
            static Int64 _sId = 0;

            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float scaleFactor = 1f;

            public float speed;

            public Color baseColor;

            public float alpha;

            public float alphaFactor = 1f;

            public int layer;

            public float radius;

            internal Int64 __id;

            public LivingEntityData()
            {
                __id = _sId++;
            }

            public Color Color
            {
                get
                {
                    return new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha * alphaFactor);
                }
            }
        }
    }

}
