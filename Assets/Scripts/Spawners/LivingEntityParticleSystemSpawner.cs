using UnityEngine;
using System.Linq;
using m039.Common;

namespace GP4
{

    public class LivingEntityParticleSystemSpawner : BaseSimulationSpawner
    {
        ParticleSystem _particleSystem;

        ParticleSystemRenderer _particleSystemRenderer;

        ParticleSystem.Particle[] _particles;

        protected override void OnInitSimulation()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();

            var main = _particleSystem.main;

            main.maxParticles = numberOfEntities;

            if (_particles == null || _particles.Length != _particleSystem.main.maxParticles)
            {
                _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
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

        protected override void OnDrawSimulation()
        {
            int i = 0;

            foreach (var entityData in Simulation.Enteties)
            {
                var particle = _particles[i];

                particle.rotation = entityData.rotation;
                particle.axisOfRotation = Vector3.forward;
                particle.startColor = entityData.Color;
                particle.velocity = Vector3.zero;
                particle.startLifetime = particle.remainingLifetime = 1000f;
                particle.startSize3D = entityData.Scale;
                particle.position = entityData.Position;

                _particles[i] = particle;

                i++;
            }

            _particleSystem.SetParticles(_particles);
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
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Using Shuriken (the default, CPU-based Unity particle system)");
        }
    }

}
