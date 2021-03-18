using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using m039.Common;

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

        readonly LinkedList<LivingEntityData> _enteties = new LinkedList<LivingEntityData>();

        readonly List<LivingEntityData> _entetiesCache = new List<LivingEntityData>(1024);

        Mesh _bigTrianlgeMesh;

        Material _bigTriangleMaterial;

        MaterialPropertyBlock _propertyBlock;

        static readonly int ColorId = Shader.PropertyToID("_Color");

        protected override void OnEnable()
        {
            base.OnEnable();

            InitRenderData();
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
            UpdateEnteties();
            CreateInstances();
            DrawEnteties();
        }

        void UpdateEnteties()
        {
            bool updateEntity(LivingEntityData data)
            {
                var rotation = Quaternion.AngleAxis(data.rotation, Vector3.forward);
                var deltaPosition = (Vector2)(rotation * Vector3.up * (data.speed * entetiesReferenceSpeed) * Time.deltaTime);

                data.position += deltaPosition;
                data.scaleFactor = entetiesReferenceScale;
                data.alphaFactor = entetiesReferenceAlpha;

                if (data.alpha < 1)
                {
                    data.alpha = Mathf.Clamp(data.alpha + Time.deltaTime * AlphaFadeOutSpeed, 0, 1);
                }

                var boundRadius = data.scaleFactor * data.radius;
                
                return Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, data.position, boundRadius);
            }

            var node = _enteties.First;

            while (node != null)
            {
                var next = node.Next;

                // True to keep the variable.
                if (!updateEntity(node.Value))
                {
                    _enteties.Remove(node);
                    _entetiesCache.Add(node.Value);
                }

                node = next;
            }
        }
      
        void DrawEnteties()
        {
            var camera = Camera.main;

            for (int i = 0; i < Context.LivingEntityData.NumberOfLayers; i++)
            {
                foreach (var data in _enteties)
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

        void CreateInstances()
        {
            while (_enteties.Count < numberOfEntities)
            {
                SpawnLivingEntity();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            foreach (var data in _enteties)
            {
                var boundRadius = data.scaleFactor * data.radius;

                if (Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, data.position, boundRadius))
                {
                    Gizmos.color = Color.blue.WithAlpha(0.5f * data.alpha);
                } else
                {
                    Gizmos.color = Color.yellow.WithAlpha(0.5f * data.alpha);
                }

                Gizmos.DrawWireSphere(data.position, boundRadius);
            }
        }

        void SpawnLivingEntity()
        {
            Vector3 getPosition(Vector2 normalizedPosition)
            {
                var center = GameScene.Instance.SceneBounds.center;
                var size = GameScene.Instance.SceneBounds.size;
                size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                return center + size;
            }

            LivingEntityData entityData;

            // Use cache
            if (_entetiesCache.Count > 0)
            {
                entityData = _entetiesCache[_entetiesCache.Count - 1];
                _entetiesCache.RemoveAt(_entetiesCache.Count - 1);
            } else
            {
                entityData = new LivingEntityData();
            }

            var livingEntityData = Context.LivingEntityData.GetData();

            entityData.position = getPosition(livingEntityData.position);
            entityData.rotation = livingEntityData.rotation;
            entityData.scale = livingEntityData.scale;
            entityData.speed = livingEntityData.speed;
            entityData.baseColor = livingEntityData.color;
            entityData.layer = livingEntityData.layer;
            entityData.radius = livingEntityData.radius;
            entityData.alpha = 0f;

            _enteties.AddLast(entityData);
        }

        public override void OnSpawnerSelected()
        {
            CreateInstances();
        }

        public override void OnSpawnerDeselected()
        {
            _enteties.Clear();
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
            drawer.DrawStat(0, "Entities: " + _enteties.Count);
            drawer.DrawStat(1, "Global Scale: " + entetiesReferenceScale);
            drawer.DrawStat(2, "Global Alpha: " + entetiesReferenceAlpha);
            drawer.DrawStat(3, "Global Speed: " + entetiesReferenceSpeed);

            drawer.DrawName("Graphics.DrawMesh, Transparent [DrawMesh]");

            drawer.DrawGetNumber("Number of Enteties [" + numberOfEntities + "]:", ref numberOfEntities);
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
