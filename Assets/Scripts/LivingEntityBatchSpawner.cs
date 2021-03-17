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

        #endregion

        readonly LinkedList<LivingEntityData> _enteties = new LinkedList<LivingEntityData>();

        readonly List<LivingEntityData> _entetiesCache = new List<LivingEntityData>(1024);

        Mesh _bigTrianlgeMesh;

        Material _bigTriangleMaterial;

        float _entityRadius;

        MaterialPropertyBlock _propertyBlock;

        GUIStyle _labelStyle;

        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake()
        {
            InitRenderData();
            CreateInstances();
        }

        void OnGUI()
        {
            const float referenceHeight = 1920;
            float coeff = Screen.height / referenceHeight;

            var width = Screen.width;
            var height = Screen.height;
            var windowHeight = 200 * coeff;
            var windowWidth = 600 * coeff;
            var margin = 100 * coeff;
            var rect = new Rect(width - windowWidth - margin, margin, windowWidth, windowHeight);
            var offset = 4 * coeff;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = (int) (60 * coeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.normal.textColor = Color.white;
            }

            void drawText(int positionLine, string text)
            {
                var topOffset = _labelStyle.fontSize * positionLine + 50 * coeff * positionLine;

                // Draw shadow

                var tRect = new Rect(rect);
                tRect.center += Vector2.one * offset + Vector2.up * topOffset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, text, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(rect);
                tRect.center += Vector2.up * topOffset;

                GUI.Label(tRect, text, _labelStyle);
            }

            drawText(0, "Entities: " + _enteties.Count);
            drawText(1, "Global Scale: " + entetiesReferenceScale);
            drawText(2, "Global Alpha: " + entetiesReferenceAlpha);
            drawText(3, "Global Speed: " + entetiesReferenceSpeed);
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

        const int BufferSize = 1023;

        readonly Matrix4x4[] _bufferedDataCache = new Matrix4x4[BufferSize];

        const bool UseBatch = false;

        void DrawEntetiesBatched()
        {
            if (!UseBatch)
            {
                foreach (var data in _enteties)
                {
                    _propertyBlock.SetColor(ColorId, data.Color);
                    Graphics.DrawMesh(_bigTrianlgeMesh, data.Matrix, _bigTriangleMaterial, 0, Camera.main, 0, _propertyBlock);
                }
            }
            else
            {
                if (_enteties.Count <= 0)
                    return;

                _propertyBlock.SetColor(ColorId, new Color(1, 1, 1, 1));
                var i = 0;

                foreach (var data in _enteties)
                {
                    _bufferedDataCache[i++] = data.Matrix;

                    if (i >= BufferSize)
                    {
                        Graphics.DrawMeshInstanced(_bigTrianlgeMesh, 0, _bigTriangleMaterial, _bufferedDataCache, BufferSize);

                        i = 0;
                    }
                }

                if (i != 0)
                {
                    Graphics.DrawMeshInstanced(_bigTrianlgeMesh, 0, _bigTriangleMaterial, _bufferedDataCache, i);
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
