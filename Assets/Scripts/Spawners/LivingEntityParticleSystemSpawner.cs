using UnityEngine;
using System.Linq;
using m039.Common;
using Unity.Collections;
using Unity.Jobs;

namespace GP4
{

    public class LivingEntityParticleSystemSpawner : BaseSimulationSpawner
    {
        ParticleSystem _particleSystem;

        ParticleSystemRenderer _particleSystemRenderer;

        NativeArray<ParticleSystem.Particle>? _particles;

        protected override void OnInitSimulation()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();

            var main = _particleSystem.main;

            main.maxParticles = numberOfEntities;

            if (!_particles.HasValue || _particles.Value.Length != _particleSystem.main.maxParticles)
            {
                if (_particles.HasValue)
                    _particles.Value.Dispose();

                _particles = new NativeArray<ParticleSystem.Particle>(_particleSystem.main.maxParticles, Allocator.Persistent);
            }

            _particleSystem.Clear();

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

        struct CopyMemoryJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<LivingEntityData> enteties;

            public NativeArray<ParticleSystem.Particle> particles;

            public void Execute(int index)
            {
                var particle = particles[index];
                var entityData = enteties[index];

                particle.rotation = entityData.rotation;
                particle.axisOfRotation = Vector3.forward;
                particle.startColor = entityData.Color;
                particle.velocity = Vector3.zero;
                particle.startLifetime = particle.remainingLifetime = 1000f;
                particle.startSize3D = entityData.Scale;
                particle.position = entityData.Position;

                particles[index] = particle;
            }
        }

        protected override void OnDrawSimulation()
        {
            new CopyMemoryJob
            {
                enteties = Simulation.Enteties,
                particles = _particles.Value
            }.Schedule(_particles.Value.Length, 1024).Complete();

            _particleSystem.SetParticles(_particles.Value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _particleSystem.Clear();
            _particleSystem.Play();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _particleSystem.Stop();

            _particles.Value.Dispose();
            _particles = null;
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Using Shuriken (the default, CPU-based Unity particle system)");
        }
    }

}
