using m039.Common;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GP4
{
    public class LivingEntityGPUSpawner : BaseSpawner
    {
        #region Inspector

        [SerializeField]
        ComputeShader _ParticleShader;

        #endregion

        GraphicsBuffer _meshTriangles;

        GraphicsBuffer _meshPositions;

        GraphicsBuffer _meshUV;

        Material _material;

        MaterialPropertyBlock _materialPropertyBlock;

        ComputeBuffer _buffer;

        int _previousNumberOfEntities = -1;

        int _kernelId;

        int _threadGroups;

        AsyncGPUReadbackRequest? _request;

        [NonSerialized]
        public bool HandleOutOfBoundsOnGPU = true;

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

            var particles = new Particle[numberOfEntities];

            for (int i = 0; i < numberOfEntities; i++)
            {
                particles[i] = CreateParticle();
            }

            _buffer = new ComputeBuffer(
                numberOfEntities,
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(Particle)),
                ComputeBufferType.Default
            );
            _buffer.SetData(particles);

            _material.SetBuffer("_Particles", _buffer);

            _kernelId = _ParticleShader.FindKernel("Process");
            _ParticleShader.SetBuffer(_kernelId, "_Particles", _buffer);
            _ParticleShader.SetInt("_NumberOfEnteties", numberOfEntities);
            _ParticleShader.SetFloat("_AlphaFadeOutSpeed", BaseSimulationSpawner.AlphaFadeOutSpeed);

            _ParticleShader.GetKernelThreadGroupSizes(_kernelId, out uint threadGroupSizeX, out _, out _);
            _threadGroups = Mathf.CeilToInt((float)numberOfEntities / threadGroupSizeX);
        }

        Particle CreateParticle()
        {
            Vector3 getPosition(Vector2 normalizedPosition)
            {
                var sb = SceneBounds();
                var center = sb.center;
                var size = sb.size / 2f;
                size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                return center + size;
            }

            var initData = Context.LivingEntityConfig.GetData();

            return new Particle
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

        void Update()
        {
            if (numberOfEntities == 0)
                return;

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

            var bounds = SceneBounds();

            _ParticleShader.SetFloat("_Speed", entetiesReferenceSpeed);
            _ParticleShader.SetFloat("_DeltaTime", Time.deltaTime);
            _ParticleShader.SetFloats("_BoundSize", bounds.size.x, bounds.size.y);
            _ParticleShader.SetFloats("_BoundCenter", bounds.center.x, bounds.center.y);
            _ParticleShader.SetInt("_HandleOutOfBounds", HandleOutOfBoundsOnGPU ? 1 : 0);

            _ParticleShader.Dispatch(_kernelId, _threadGroups, 1, 1);

            if (!HandleOutOfBoundsOnGPU && _request == null)
            {
                _request = AsyncGPUReadback.Request(_buffer, (callback) =>
                {
                    _request = null;

                    if (!enabled)
                        return;

                    var particles = callback.GetData<Particle>();

                    for (int i = 0; i < particles.Length; i++)
                    {
                        if (particles[i].outOfBounds == 1)
                        {
                            particles[i] = CreateParticle();
                        }
                    }

                    _buffer.SetData(particles);
                });
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
            RenderParams rp = new(_material);
            rp.worldBounds = SceneBounds();
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer("_Triangles", _meshTriangles);
            rp.matProps.SetBuffer("_Positions", _meshPositions);
            rp.matProps.SetBuffer("_UV", _meshUV);
            rp.matProps.SetFloat("_Scale", entetiesReferenceScale);
            rp.matProps.SetFloat("_Alpha", entetiesReferenceAlpha);

            for (int i = 0; i < Context.LivingEntityConfig.NumberOfLayers; i++)
            {
                rp.matProps.SetInt("_Layer", i);
                Graphics.RenderPrimitives(rp, MeshTopology.Triangles, 6, numberOfEntities);
            }
        }

        protected override void PerformOnGUI(Drawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Using compute shader to move particles and GPU to draw them.");
        }
    }
}
