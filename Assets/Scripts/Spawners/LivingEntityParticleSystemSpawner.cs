using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using m039.Common;

namespace GP4
{

    public class LivingEntityParticleSystemSpawner : BaseSpawner
    {

        ParticleSystem _particleSystem;

        ParticleSystemRenderer _particleSystemRenderer;

        List<Vector4> _psCustomData = new List<Vector4>();

        ParticleSystem.Particle[] _psParticles;

        void Awake()
        {
            UpdateParameters();
        }

        void Start()
        {
            OnLivingEntityDataChanged();
        }

        void OnValidate()
        {
            UpdateParameters();
        }

        void LateUpdate()
        {
            UpdateParameters();
            KeepParticlesToMax();
            UpdateParticles();
        }

        void UpdateParticles()
        {
            InitIfNeeded();

            var count = _particleSystem.GetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.GetParticles(_psParticles, count);

            for (int i = 0; i < count; i++)
            {
                if (_psCustomData[i].x == 0.0f)
                {
                    InitParticle(i);
                }

                UpdateParticle(i);
            }

            _particleSystem.SetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.SetParticles(_psParticles);
        }

        void InitParticle(int particleIndex)
        {
            Vector3 getPosition(Vector2 normalizedPosition)
            {
                var center = GameScene.Instance.SceneBounds.center;
                var size = GameScene.Instance.SceneBounds.size;
                size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                return center + size;
            }

            var particle = _psParticles[particleIndex];

            var initData = Context.LivingEntityConfig.GetData();

            particle.rotation = initData.rotation;
            particle.startColor = initData.color;
            particle.velocity = Quaternion.AngleAxis(initData.rotation, Vector3.forward) * Vector3.up * initData.speed;
            particle.startLifetime = particle.remainingLifetime = 1000f;
            particle.startSize3D = initData.scale * entetiesReferenceScale;
            particle.position = getPosition(initData.position).WithZ(-initData.layer);

            _psParticles[particleIndex] = particle;

            _psCustomData[particleIndex] = new Vector4(1, initData.radius, 0, 0); // Mark as processed.
        }

        void UpdateParticle(int particleIndex)
        {
            var particle = _psParticles[particleIndex];
            var radius = _psCustomData[particleIndex][1];

            // Update the particle data.

            if (!Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, particle.position + transform.position, radius))
            {
                particle.remainingLifetime = 0f;

                _psParticles[particleIndex] = particle;
            }

            // Update the custom data.
            var custom = _psCustomData[particleIndex];
            var alpha = custom[2];

            if (alpha < 1)
            {
                custom[2] = alpha = Mathf.Clamp(alpha + Time.deltaTime * LivingEntityDrawMeshSpawner.AlphaFadeOutSpeed, 0, 1);
            }

            custom[3] = alpha * entetiesReferenceAlpha;

            _psCustomData[particleIndex] = custom;
        }

        void UpdateParameters()
        {
            InitIfNeeded();

            var main = _particleSystem.main;

            main.maxParticles = numberOfEntities;

            var velocityOverLifetime = _particleSystem.velocityOverLifetime;

            velocityOverLifetime.speedModifierMultiplier = entetiesReferenceSpeed;
        }

        void InitIfNeeded()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            if (_particleSystemRenderer == null)
            {
                _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
            }

            if (_psParticles == null || _psParticles.Length != _particleSystem.main.maxParticles)
            {
                _psParticles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
                _particleSystem.Clear();
            }
        }

        void KeepParticlesToMax()
        {
            if (_particleSystem.particleCount < numberOfEntities)
            {
                var p = new ParticleSystem.EmitParams()
                {
                    startLifetime = 0.01f
                };
                _particleSystem.Emit(p, numberOfEntities - _particleSystem.particleCount);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            InitIfNeeded();

            var count = _particleSystem.GetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.GetParticles(_psParticles, count);

            for (int i = 0; i < count; i++) {
                var particle = _psParticles[i];
                var radius = _psCustomData[i][1];

                var color = Color.blue.WithAlpha(0.5f);

                if (!Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, particle.position + transform.position, radius))
                {
                    color = Color.yellow.WithAlpha(0.5f);
                }

                Gizmos.color = color;
                Gizmos.DrawWireSphere(_psParticles[i].position + transform.position, radius);
            }
        }

        public override void OnSpawnerSelected()
        {
            _particleSystem.Clear();
            _particleSystem.Play();
        }

        public override void OnSpawnerDeselected()
        {
            _particleSystem.Stop();
        }

        protected override void OnLivingEntityDataChanged()
        {
            base.OnLivingEntityDataChanged();

            // Init appearance

            var entityData = Context.LivingEntityConfig.GetData();
            var sprite = entityData.sprite;

            var mesh = new Mesh();
            mesh.vertices = sprite.vertices.Select(v => (Vector3)v).ToArray();
            mesh.triangles = sprite.triangles.Select(t => (int)t).ToArray();
            mesh.uv = sprite.uv;
            mesh.colors = Enumerable.Repeat(Color.white, mesh.vertices.Length).ToArray();

            var material = new Material(Shader.Find("Unlit/SimpleSprite"));
            material.enableInstancing = true;
            material.mainTexture = sprite.texture;
            material.color = Color.white;
            material.EnableKeyword("USE_IN_PARTICLE");

            _particleSystemRenderer.mesh = mesh;
            _particleSystemRenderer.enableGPUInstancing = true;
            _particleSystemRenderer.material = material;
            _particleSystemRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        }

        protected override int EntetiesCount => _particleSystem.particleCount;

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawName("Using Shuriken (the default, CPU-based Unity particle system)");
        }
    }

}
