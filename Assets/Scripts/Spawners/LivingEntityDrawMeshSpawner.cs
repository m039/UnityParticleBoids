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

        Mesh _bigTrianlgeMesh;

        Material _bigTriangleMaterial;

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
            var entityData = Context.LivingEntityData.GetData();
            var sprite = entityData.sprite;

            _bigTrianlgeMesh = new Mesh();
            _bigTrianlgeMesh.vertices = sprite.vertices.Select(v => (Vector3)v).ToArray();
            _bigTrianlgeMesh.triangles = sprite.triangles.Select(t => (int)t).ToArray();
            _bigTrianlgeMesh.uv = sprite.uv;
            _bigTrianlgeMesh.colors = Enumerable.Repeat(Color.white, _bigTrianlgeMesh.vertices.Length).ToArray();

            _bigTriangleMaterial = new Material(Shader.Find("Unlit/SimpleSprite"));
            _bigTriangleMaterial.enableInstancing = true;
            _bigTriangleMaterial.mainTexture = sprite.texture;
            _bigTriangleMaterial.color = Color.white;

            _propertyBlock = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            _simulation.Update();
            _simulation.Populate(numberOfEntities, Context.LivingEntityData);
            DrawEnteties();
        }

        void DrawEnteties()
        {
            var camera = Camera.main;

            for (int i = 0; i < Context.LivingEntityData.NumberOfLayers; i++)
            {
                foreach (var data in _simulation.Enteties)
                {
                    if (data.layer != i)
                        continue;

                    _propertyBlock.SetColor(ColorId, data.Color);
                    Graphics.DrawMesh(
                        _bigTrianlgeMesh,
                        data.Matrix,
                        _bigTriangleMaterial,
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
            readonly LinkedList<LivingEntityData> _enteties = new LinkedList<LivingEntityData>();

            readonly Queue<LivingEntityData> _entetiesCache = new Queue<LivingEntityData>();

            public GetSettingValue<float> entetiesReferenceScale = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceAlpha = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceSpeed = () => 1.0f;

            public GetSettingValue<Bounds> sceneBounds => () => GameScene.Instance.SceneBounds;

            public LinkedList<LivingEntityData> Enteties => _enteties;

            public void Populate(int numberOfEntities, BaseLivingEntityData entetyData)
            {
                while (_enteties.Count < numberOfEntities)
                {
                    _enteties.AddLast(CreateLivingEntityData(entetyData));
                }
            }

            LivingEntityData CreateLivingEntityData(BaseLivingEntityData entetyData)
            {
                Vector3 getPosition(Vector2 normalizedPosition)
                {
                    var sb = sceneBounds();
                    var center = sb.center;
                    var size = sb.size;
                    size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                    return center + size;
                }

                LivingEntityData entityData;

                // Use cache
                if (_entetiesCache.Count > 0)
                {
                    entityData = _entetiesCache.Dequeue();
                }
                else
                {
                    entityData = new LivingEntityData();
                }

                var initData = entetyData.GetData();

                entityData.position = getPosition(initData.position);
                entityData.rotation = initData.rotation;
                entityData.scale = initData.scale;
                entityData.speed = initData.speed;
                entityData.baseColor = initData.color;
                entityData.layer = initData.layer;
                entityData.radius = initData.radius;
                entityData.alpha = 0f;

                return entityData;
            }

            public void Update()
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

                var node = _enteties.First;

                while (node != null)
                {
                    var next = node.Next;

                    // True to keep the variable.
                    if (!updateEntity(node.Value))
                    {
                        _enteties.Remove(node);
                        _entetiesCache.Enqueue(node.Value);
                    }

                    node = next;
                }
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

                    Gizmos.color = Color.white;

                    Gizmos.DrawLine(data.position, (Vector3) data.position + Quaternion.AngleAxis(data.rotation, Vector3.forward) * Vector3.up * boundRadius);
                }
            }

            public void Reset()
            {
                _enteties.Clear();
                _entetiesCache.Clear();
            }
        }

        public class LivingEntityData
        {
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

            public Color Color
            {
                get
                {
                    return new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha * alphaFactor);
                }
            }

            public Matrix4x4 Matrix {
                get {
                    return Matrix4x4.TRS(position, Quaternion.AngleAxis(rotation, Vector3.forward), scale * scaleFactor);
                }
             }
        }
    }

}
