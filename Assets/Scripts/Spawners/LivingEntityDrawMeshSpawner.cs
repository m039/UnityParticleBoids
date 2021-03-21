using UnityEngine;
using System.Linq;
using m039.Common;

namespace GP4
{

    public class LivingEntityDrawMeshSpawner : BaseSimulationSpawner
    {
        Mesh _mesh;

        Material _material;

        MaterialPropertyBlock _propertyBlock;

        static readonly int ColorId = Shader.PropertyToID("_Color");

        protected override void OnInitSimulation()
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

        protected override void OnDrawSimulation()
        {
            var camera = Camera.main;

            foreach (var data in Simulation.Enteties)
            {
                _propertyBlock.SetColor(ColorId, data.Color);
                Graphics.DrawMesh(
                    _mesh,
                    Matrix4x4.TRS(data.Position, Quaternion.AngleAxis(data.rotation, Vector3.forward), data.Scale),
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

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Draw each entity with Graphics.DrawMesh");
        }
    }

}
