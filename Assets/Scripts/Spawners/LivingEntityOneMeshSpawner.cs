using UnityEngine;

namespace GP4
{
    public class LivingEntityOneMeshSpawner : BaseSimulationSpawner
    {
        readonly MeshTool _meshTool = new MeshTool();

        Mesh _mesh;

        Vector3[] _vertices;

        Color[] _colors;

        Vector2[] _uv;

        int[] _triangles;

        Material _material;

        protected override bool UseSort => true;

        protected override void OnInitSimulation()
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
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.colors = _colors;
            _mesh.triangles = _triangles;

            // Material

            _material = new Material(Shader.Find("Unlit/OneMeshSprite"));
            _material.color = Color.white;
            _material.enableInstancing = true;
            _material.mainTexture = sprite.texture; // Should be one texture for all enteties.
        }

        protected override void OnDrawSimulation()
        {
            var i = 0;

            foreach (var entityData in Simulation.Enteties)
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

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

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

            public void PopulateEntity(int index, LivingEntityData data, Vector3[] vertices, Color[] colors)
            {
                // Vertex

                int pointIndex = index * 4;

                var matrix = Matrix4x4.TRS(Vector3.zero, data.Rotation, data.Scale);

                var dataPosition = data.Position;

                Vector3 p1 = dataPosition + (Vector3)(matrix * _spriteVertices[0]);
                Vector3 p2 = dataPosition + (Vector3)(matrix * _spriteVertices[1]);
                Vector3 p3 = dataPosition + (Vector3)(matrix * _spriteVertices[2]);
                Vector3 p4 = dataPosition + (Vector3)(matrix * _spriteVertices[3]);

                vertices[pointIndex + 0] = p1;
                vertices[pointIndex + 1] = p2;
                vertices[pointIndex + 2] = p3;
                vertices[pointIndex + 3] = p4;

                // Color

                var dataColor = data.Color;

                colors[pointIndex + 0] = dataColor;
                colors[pointIndex + 1] = dataColor;
                colors[pointIndex + 2] = dataColor;
                colors[pointIndex + 3] = dataColor;
            }
        }
    }

}
