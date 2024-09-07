using m039.Common;
using System.Linq;
using UnityEngine;

namespace GP4
{
    public class LivingEntityGPUSpawner : BaseSpawner
    {
        GraphicsBuffer _meshTriangles;

        GraphicsBuffer _meshPositions;

        GraphicsBuffer _meshUV;

        Material _material;

        MaterialPropertyBlock _materialPropertyBlock;

        ComputeBuffer _buffer;

        int _previousNumberOfEntities = -1;

        GetSettingValue<Bounds> SceneBounds => () => GameScene.Instance.SceneBounds;

        protected override void OnEnable()
        {
            base.OnEnable();

            OnInit();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            OnDeinit();
        }

        void OnDeinit()
        {
            _meshTriangles?.Dispose();
            _meshTriangles = null;

            _meshPositions?.Dispose();
            _meshPositions = null;

            _meshUV?.Dispose();
            _meshUV = null;

            if (_material != null)
            {
                Destroy(_material);
            }
            _material = null;

            _buffer?.Dispose();
            _buffer = null;
        }

        protected override void OnLivingEntityDataChanged()
        {
            base.OnLivingEntityDataChanged();

            OnInit();
        }

        void OnInit()
        {
            OnDeinit();

            if (numberOfEntities == 0)
                return;

            var entityData = Context.LivingEntityConfig.GetData();
            var sprite = entityData.sprite;

            _material = new Material(Shader.Find("Game/Particle"));
            _material.enableInstancing = true;
            _material.mainTexture = sprite.texture;
            _material.color = Color.white;

            _materialPropertyBlock = new MaterialPropertyBlock();

            _meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, sprite.triangles.Length, sizeof(uint));
            _meshTriangles.SetData(sprite.triangles.Select(t => (uint)t).ToArray());
            _meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, sprite.vertices.Length, 2 * sizeof(float));
            _meshPositions.SetData(sprite.vertices);
            _meshUV = new GraphicsBuffer(GraphicsBuffer.Target.Structured, sprite.uv.Length, 2 * sizeof(float));
            _meshUV.SetData(sprite.uv);

            // Init buffer.

            Vector3 getPosition(Vector2 normalizedPosition)
            {
                var sb = SceneBounds();
                var center = sb.center;
                var size = sb.size / 2f;
                size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                return center + size;
            }

            var particles = new Particle[numberOfEntities];

            for (int i = 0; i < numberOfEntities; i++)
            {
                var initData = Context.LivingEntityConfig.GetData();

                particles[i] = new Particle
                {
                    position = getPosition(initData.position),
                    rotation = initData.rotation,
                    scale = initData.scale,
                    speed = initData.speed,
                    baseColor = initData.color,
                    radius = initData.radius,
                    layer = initData.layer,
                    alpha = 0f,
                };
            }

            _buffer = new ComputeBuffer(
                numberOfEntities,
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(Particle)),
                ComputeBufferType.Default
            );
            _buffer.SetData(particles);

            _material.SetBuffer("_Particles", _buffer);
        }

        void Update()
        {
            OnUpdate();
            OnDraw();
        }

        void OnUpdate()
        {
            if (_previousNumberOfEntities != numberOfEntities)
            {
                OnInit();
                _previousNumberOfEntities = numberOfEntities;
            }
        }

        struct Particle
        {
            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float speed;

            public Color baseColor;

            public float alpha;

            public float radius;

            public int layer;

            public int outOfBounds;
        }

        void OnDraw()
        {
            if (numberOfEntities == 0)
                return;

            RenderParams rp = new(_material);
            rp.worldBounds = SceneBounds();
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer("_Triangles", _meshTriangles);
            rp.matProps.SetBuffer("_Positions", _meshPositions);
            rp.matProps.SetBuffer("_UV", _meshUV);
            rp.matProps.SetFloat("_Scale", entetiesReferenceScale);
            rp.matProps.SetFloat("_Alpha", entetiesReferenceAlpha);
            Graphics.RenderPrimitives(rp, MeshTopology.Triangles, 6, numberOfEntities);
        }

        protected override void PerformOnGUI(Drawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Using compute shader to move particles and GPU to draw them.");
        }
    }
}
