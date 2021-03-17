using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using m039.Common;

namespace GP4
{

    public class LivingEntityDrawMeshSpawner : BaseSpawner
    {
        static readonly float ReferenceScaleMagnitude = new Vector2(1, 1).magnitude;

        const float AlphaFadeOutSpeed = 0.56f;

        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        [SerializeField]
        Material _LivingEntityMaterial;

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        public float entetiesReferenceAlpha = 1f;

        public bool useGizmos = true;

        #endregion

        readonly LinkedList<LivingEntityData> _enteties = new LinkedList<LivingEntityData>();

        readonly List<LivingEntityData> _entetiesCache = new List<LivingEntityData>(1024);

        Mesh _bigTrianlgeMesh;

        Material _bigTriangleMaterial;

        float _entityRadius;

        MaterialPropertyBlock _propertyBlock;

        static readonly int ColorId = Shader.PropertyToID("_Color");

        void OnEnable()
        {
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
        }

        void InitRenderData()
        {
            var spriteRenderer = _LivingEntityPrefab.BigTriangle.GetComponent<SpriteRenderer>();
            var sprite = spriteRenderer.sprite;

            _bigTrianlgeMesh = new Mesh();
            _bigTrianlgeMesh.vertices = sprite.vertices.Select(v => (Vector3)v).ToArray();
            _bigTrianlgeMesh.triangles = sprite.triangles.Select(t => (int)t).ToArray();
            _bigTrianlgeMesh.uv = sprite.uv;
            _bigTrianlgeMesh.colors = Enumerable.Repeat(Color.white, 4).ToArray();

            _bigTriangleMaterial = new Material(_LivingEntityMaterial);
            _bigTriangleMaterial.enableInstancing = true;
            _bigTriangleMaterial.mainTexture = sprite.texture;
            _bigTriangleMaterial.color = spriteRenderer.color;

            _entityRadius = _LivingEntityPrefab.Radius;

            _propertyBlock = new MaterialPropertyBlock();
        }

        void Update()
        {
            DrawEnteties();
        }

        void FixedUpdate()
        {
            UpdateEnteties();
        }

        void LateUpdate()
        {
            CreateInstances();
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

                var boundRadius = data.scaleFactor * data.scale.magnitude / ReferenceScaleMagnitude * _entityRadius;
                
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
            foreach (var data in _enteties)
            {
                _propertyBlock.SetColor(ColorId, data.Color);
                Graphics.DrawMesh(_bigTrianlgeMesh, data.Matrix, _bigTriangleMaterial, 0, Camera.main, 0, _propertyBlock);
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
                var boundRadius = data.scaleFactor * data.scale.magnitude / ReferenceScaleMagnitude * _entityRadius;

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
            Vector3 getPosition()
            {
                var center = GameScene.Instance.SceneBounds.center;
                var size = GameScene.Instance.SceneBounds.size;
                size = new Vector3(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.5f) * size.y);
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

            entityData.position = getPosition();
            entityData.rotation = Random.Range(0, 360f);
            entityData.scale = Vector2.one;
            entityData.speed = Random.Range(0.1f, 1f);
            entityData.baseColor = _LivingEntityPrefab.BigTriangle.GetComponent<SpriteRenderer>().color;
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
