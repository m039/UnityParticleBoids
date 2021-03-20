using UnityEngine;

using LivingEntetyData = GP4.LivingEntityDrawMeshSpawner.LivingEntityData;
using LivingEntitySimulation = GP4.LivingEntityDrawMeshSpawner.LivingEntitySimulation;

using m039.Common;

namespace GP4
{
    public class LivingEntityOneMeshSpawner : BaseSpawner
    {
        #region Inspector

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        [Range(0, 1f)]
        public float entetiesReferenceAlpha = 1f;

        public bool useGizmos = true;

        #endregion

        LivingEntitySimulation _simulation;

        readonly MeshTool _meshTool = new MeshTool();

        Mesh _mesh;

        Vector3[] _vertices;

        Color[] _colors;

        Vector2[] _uv;

        int[] _triangles;

        Material _material;

        int _previousNumberOfEntities = -1;

        protected override void OnEnable()
        {
            base.OnEnable();

            Init();
        }

        void Init()
        {
            /// Create a sprite data

            var sprite = Context.LivingEntityConfig.GetData().sprite;

            // Mesh

            _vertices = new Vector3[4 * numberOfEntities];
            _colors = new Color[4 * numberOfEntities];
            _uv = new Vector2[4 * numberOfEntities];
            _triangles = new int[6 * numberOfEntities];

            _meshTool.PopulateInit(sprite, _triangles, _uv);

            _mesh = new Mesh();
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;

            // Material

            _material = new Material(Shader.Find("Unlit/OneMeshSprite"));
            _material.color = Color.white;
            _material.enableInstancing = true;
            _material.mainTexture = sprite.texture; // Should be one texture for all enteties.

            /// Create simulation

            _simulation = new LivingEntitySimulation()
            {
                entetiesReferenceScale = () => entetiesReferenceScale,
                entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                entetiesReferenceSpeed = () => entetiesReferenceSpeed
            };
        }

        void LateUpdate()
        {
            UpdateSimulation();
            DrawEnteties();
        }

        void UpdateSimulation()
        {
            // Do physics with enteties.
            _simulation.Update();

            // Reset the simulation when needed.
            if (_previousNumberOfEntities != numberOfEntities)
            {
                Init();
                _previousNumberOfEntities = numberOfEntities;
            }

            // Create all enteties data if needed.
            _simulation.Populate(numberOfEntities, Context.LivingEntityConfig);
        }

        void DrawEnteties()
        {
            var i = 0;

            foreach (var entityData in _simulation.Enteties)
            {
                _meshTool.PopulateEntity(i, entityData, _vertices, _colors);
                i++;
            }

            _mesh.vertices = _vertices;
            _mesh.colors = _colors;

            Graphics.DrawMesh(
                _mesh,
                Matrix4x4.identity,
                _material,
                0,
                Camera.main,
                0,
                null,
                false,
                false,
                false
                );
        }

        void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            if (Application.isPlaying)
                _simulation.DrawGizmos();
        }

        public override void OnSpawnerDeselected()
        {
            
        }

        public override void OnSpawnerSelected()
        {
            _simulation.Reset();
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawStatFrame(4);
            drawer.DrawStat(0, "Entities: " + _simulation.Enteties.Count);
            drawer.DrawStat(1, "Global Scale: " + entetiesReferenceScale);
            drawer.DrawStat(2, "Global Alpha: " + entetiesReferenceAlpha);
            drawer.DrawStat(3, "Global Speed: " + entetiesReferenceSpeed);

            drawer.DrawName("Using one mesh for drawing all enteties");
        }

        public class MeshTool
        {
            Vector2[] _spriteVertices;

            public void PopulateInit(Sprite sprite, int[] triangles, Vector2[] uv)
            {
                var position = 0;
                var tTriangles = sprite.triangles;

                for (int i = 0; i < triangles.Length; i += 6)
                {
                    triangles[i + 0] = position + tTriangles[0];
                    triangles[i + 1] = position + tTriangles[1];
                    triangles[i + 2] = position + tTriangles[2];

                    triangles[i + 3] = position + tTriangles[3];
                    triangles[i + 4] = position + tTriangles[4];
                    triangles[i + 5] = position + tTriangles[5];

                    position += 4;
                }

                var tUv = sprite.uv;

                for (int i = 0; i < uv.Length; i += 4)
                {
                    uv[i + 0] = tUv[0];
                    uv[i + 1] = tUv[1];
                    uv[i + 2] = tUv[2];
                    uv[i + 3] = tUv[3];
                }

                _spriteVertices = sprite.vertices;
            }

            public void PopulateEntity(int index, LivingEntetyData data, Vector3[] vertices, Color[] colors)
            {
                // Vertex

                var rotation = Quaternion.AngleAxis(data.rotation, Vector3.forward);
                var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector2.one * data.scale * data.scaleFactor);

                Vector3 p1 = data.position + (Vector2)(matrix * _spriteVertices[0]);
                Vector3 p2 = data.position + (Vector2)(matrix * _spriteVertices[1]);
                Vector3 p3 = data.position + (Vector2)(matrix * _spriteVertices[2]);
                Vector3 p4 = data.position + (Vector2)(matrix * _spriteVertices[3]);

                vertices[index * 4 + 0] = p1.WithZ(-data.layer);
                vertices[index * 4 + 1] = p2.WithZ(-data.layer);
                vertices[index * 4 + 2] = p3.WithZ(-data.layer);
                vertices[index * 4 + 3] = p4.WithZ(-data.layer);

                // Color

                colors[index * 4 + 0] = data.Color;
                colors[index * 4 + 1] = data.Color;
                colors[index * 4 + 2] = data.Color;
                colors[index * 4 + 3] = data.Color;
            }
        }
    }

}
