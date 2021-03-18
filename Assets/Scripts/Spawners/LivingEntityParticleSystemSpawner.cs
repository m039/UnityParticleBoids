using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using m039.Common;

namespace GP4
{

    public class LivingEntityParticleSystemSpawner : BaseSpawner
    {

        #region Inspector

        public bool useBurstCompiler = false;

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        public bool useGizmos = true;

        #endregion

        ParticleSystem _particleSystem;

        ParticleSystemRenderer _particleSystemRenderer;

        List<Vector4> _psCustomData = new List<Vector4>();

        ParticleSystem.Particle[] _psParticles;

        float _entityRadius;

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
            UpdateNewParticles();
        }

        void UpdateNewParticles()
        {
            InitIfNeeded();

            var count = _particleSystem.GetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.GetParticles(_psParticles, count);

            for (int i = 0; i < count; i++)
            {
                if (_psCustomData[i].x == 0.0f)
                {
                    InitParticle(i);
                } else
                {
                    UpdateParticle(i);
                }
            }

            _particleSystem.SetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.SetParticles(_psParticles);
        }

        void InitParticle(int particleIndex)
        {
            var particle = _psParticles[particleIndex];

            var initData = Context.LivingEntityData.GetData();

            particle.rotation = initData.rotation;
            particle.startColor = initData.color;
            particle.velocity = Quaternion.AngleAxis(initData.rotation, Vector3.forward) * Vector3.up * initData.speed;
            particle.startLifetime = particle.remainingLifetime = 1000000f;
            particle.startSize3D = initData.scale * entetiesReferenceScale;
            particle.position = particle.position.WithZ(-initData.layer);

            _psParticles[particleIndex] = particle;

            _psCustomData[particleIndex] = new Vector4(1, initData.radius, 0, 0); // Mark as processed.
        }

        void UpdateParticle(int particleIndex)
        {
            var particle = _psParticles[particleIndex];
            var radius = _psCustomData[particleIndex][1];

            var boundRadius = particle.startSize / LivingEntityDrawMeshSpawner.ReferenceScaleMagnitude * radius;

            if (!Physics2DUtils.CircleWithin(GameScene.Instance.SceneBounds, particle.position + transform.position, boundRadius))
            {
                particle.remainingLifetime = 0f;

                _psParticles[particleIndex] = particle;
            }
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

            if (_psParticles == null || _psParticles.Length < _particleSystem.main.maxParticles)
            {
                _psParticles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            var count = _particleSystem.GetCustomParticleData(_psCustomData, ParticleSystemCustomData.Custom1);
            _particleSystem.GetParticles(_psParticles, count);

            for (int i = 0; i < count; i++) {
                var particle = _psParticles[i];
                var radius = _psCustomData[i][1];

                Gizmos.DrawWireSphere(_psParticles[i].position + transform.position, particle.startSize / LivingEntityDrawMeshSpawner.ReferenceScaleMagnitude * radius);
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

            var entityData = Context.LivingEntityData.GetData();
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

            _entityRadius = entityData.radius;
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawStatFrame(3);
            drawer.DrawStat(0, "Entities: " + _particleSystem.particleCount);
            drawer.DrawStat(1, "Global Scale: " + entetiesReferenceScale);
            drawer.DrawStat(2, "Global Speed: " + entetiesReferenceSpeed);

            drawer.DrawName("ParticleSystem, " + (useBurstCompiler? "With" : "Without") + " Burst [ParticleSystem]");
        }
    }

}
