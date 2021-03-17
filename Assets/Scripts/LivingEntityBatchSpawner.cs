using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using m039.Common;

namespace GP4
{

    public class LivingEntityBatchSpawner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        [SerializeField]
        Material _LivingEntityMaterial;

        public int numberOfEntities = 10;

        public float entetiesSpeed = 5f;

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
            CreateInstances();
        }

        void UpdateEnteties()
        {
            bool updateEntity(LivingEntityData data)
            {
                var rotation = Quaternion.AngleAxis(data.rotation, Vector3.forward);
                var deltaPosition = (Vector2)(rotation * Vector3.up * (data.speed * entetiesSpeed) * Time.deltaTime);

                data.position += deltaPosition;
                
                return Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, data.position, _entityRadius);
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

        readonly List<Matrix4x4[]> _bufferedDataCache = new List<Matrix4x4[]>();

        void DrawEntetiesBatched()
        {
            Matrix4x4[] getBufferedDataCache()
            {
                if (_bufferedDataCache.Count <= 0)
                {
                    return new Matrix4x4[BufferSize];
                } else
                {
                    var cache = _bufferedDataCache[_bufferedDataCache.Count - 1];
                    _bufferedDataCache.RemoveAt(_bufferedDataCache.Count - 1);
                    cache.Fill(default);
                    return cache;
                }
            }

            void saveBufferedDataToCache()
            {
                _bufferedDataCache.AddRange(_bufferedData);
            }

            if (_enteties.Count <= 0)
                return;

            _bufferedData.Clear();

            var tBuffer = getBufferedDataCache();
            var i = 0;

            foreach (var node in _enteties)
            {
                tBuffer[i++] = node.Matrix;

                if (i >= BufferSize)
                {
                    _bufferedData.Add(tBuffer);
                    tBuffer = getBufferedDataCache();
                    i = 0;
                }
            }

            if (i != 0)
            {
                _bufferedData.Add(tBuffer);
            }

            foreach (var batch in _bufferedData)
            {
                Graphics.DrawMeshInstanced(_bigTrianlgeMesh, 0, _bigTriangleMaterial, batch);
            }

            saveBufferedDataToCache();
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
            foreach (var e in _enteties)
            {
                if (Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, e.position, _entityRadius))
                {
                    Gizmos.color = Color.blue.WithAlpha(0.5f);
                } else
                {
                    Gizmos.color = Color.yellow.WithAlpha(0.5f);
                }

                Gizmos.DrawWireSphere(e.position, _entityRadius);
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
            entityData.scale = Vector2.one * 0.5f;
            entityData.speed = Random.Range(0.1f, 1f);

            _enteties.AddLast(entityData);
        }

        public class LivingEntityData
        {
            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float speed;

            public Matrix4x4 Matrix {
                get {
                    return Matrix4x4.TRS(position, Quaternion.AngleAxis(rotation, Vector3.forward), scale);
                }
             }
        }
    }

}
