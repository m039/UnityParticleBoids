using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using m039.Common;

namespace GP4
{

    public class LivingEntityBatchSpawner : MonoBehaviour
    {
        static readonly float ReferenceScaleMagnitude = new Vector2(1, 1).magnitude;

        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        [SerializeField]
        Material _LivingEntityMaterial;

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        #endregion

        readonly LinkedList<LivingEntityData> _enteties = new LinkedList<LivingEntityData>();

        readonly List<LivingEntityData> _entetiesCache = new List<LivingEntityData>(1024);

        readonly List<Matrix4x4[]> _bufferedData = new List<Matrix4x4[]>();

        Mesh _bigTrianlgeMesh;

        Material _bigTriangleMaterial;

        float _entityRadius;

        void Awake()
        {
            InitRenderData();
            CreateInstances();
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
        }

        void Update()
        {
            DrawEntetiesBatched();
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

        const int BufferSize = 1023;

        readonly Matrix4x4[] _bufferedDataCache = new Matrix4x4[BufferSize];

        void DrawEntetiesBatched()
        {

            if (_enteties.Count <= 0)
                return;

            _bufferedData.Clear();

            _bufferedDataCache.Fill(default);
            var i = 0;

            foreach (var node in _enteties)
            {
                _bufferedDataCache[i++] = node.Matrix;

                if (i >= BufferSize)
                {
                    Graphics.DrawMeshInstanced(_bigTrianlgeMesh, 0, _bigTriangleMaterial, _bufferedDataCache);

                    _bufferedDataCache.Fill(default);
                    i = 0;
                }
            }

            if (i != 0)
            {
                Graphics.DrawMeshInstanced(_bigTrianlgeMesh, 0, _bigTriangleMaterial, _bufferedDataCache);
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
            foreach (var data in _enteties)
            {
                var boundRadius = data.scaleFactor * data.scale.magnitude / ReferenceScaleMagnitude * _entityRadius;

                if (Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, data.position, boundRadius))
                {
                    Gizmos.color = Color.blue.WithAlpha(0.5f);
                } else
                {
                    Gizmos.color = Color.yellow.WithAlpha(0.5f);
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

            _enteties.AddLast(entityData);
        }

        public class LivingEntityData
        {
            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float scaleFactor = 1f;

            public float speed;

            public Matrix4x4 Matrix {
                get {
                    return Matrix4x4.TRS(position, Quaternion.AngleAxis(rotation, Vector3.forward), scale * scaleFactor);
                }
             }
        }
    }

}
