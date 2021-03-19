using UnityEngine;

using LivingEntityData = GP4.LivingEntityDrawMeshSpawner.LivingEntityData;
using LivingEntitySimulation = GP4.LivingEntityDrawMeshSpawner.LivingEntitySimulation;

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

        Mesh _mesh;

        Vector3[] _vertices;

        Vector2[] _uv;

        int[] _triangles;

        int _previousNumberOfEntities = -1;

        protected override void OnEnable()
        {
            base.OnEnable();

            Init();
        }

        void Init()
        {
            // Create mesh

            _vertices = new Vector3[4 * numberOfEntities];
            _uv = new Vector2[4 * numberOfEntities];
            _triangles = new int[6 * numberOfEntities];

            _mesh = new Mesh();
            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            // Create simulation

            _simulation = new LivingEntitySimulation()
            {
                entetiesReferenceScale = () => entetiesReferenceScale,
                entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                entetiesReferenceSpeed = () => entetiesReferenceSpeed
            };
        }

        void LateUpdate()
        {

            DrawEnteties();
        }

        void UpdateSimulation()
        {

        }

        void DrawEnteties()
        {
            // Do physics with enteties.
            _simulation.Update();

            // Resete the simulation when needed.
            if (_previousNumberOfEntities != numberOfEntities)
            {
                _simulation.Reset();
                _previousNumberOfEntities = numberOfEntities;
            }

            // Create all enteties data if needed.
            _simulation.Populate(numberOfEntities, Context.LivingEntityData);
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
    }

}
