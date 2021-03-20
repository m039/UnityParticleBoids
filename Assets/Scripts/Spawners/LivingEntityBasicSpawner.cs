using UnityEngine;
using m039.Common;

using LivingEntetyData = GP4.LivingEntityDrawMeshSpawner.LivingEntityData;
using LivingEntitySimulation = GP4.LivingEntityDrawMeshSpawner.LivingEntitySimulation;
using System.Collections.Generic;

namespace GP4
{

    public class LivingEntityBasicSpawner : BaseSpawner
    {
        #region Inspector

        [Header("Basic Spawner Settings")]
        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        #endregion

        LivingEntitySimulation _simulation;

        LivingEntity[] _livingEntities;

        int _previousNumberOfEntities = -1;

        GameObject _parent;

        protected override void OnEnable()
        {
            base.OnEnable();

            Init();
        }

        void LateUpdate()
        {
            UpdateSimulation();
            PrePareForDrawing();
        }

        void Init()
        {
            /// Create simulation

            _simulation = new LivingEntitySimulation()
            {
                entetiesReferenceScale = () => entetiesReferenceScale,
                entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                entetiesReferenceSpeed = () => entetiesReferenceSpeed
            };

            // Create gameObjects

            var cache = new List<Transform>(); // Reuse gameobjects from the parent.

            if (_parent != null)
            {
                for (int i = 0; i < _parent.transform.childCount; i++)
                {
                    cache.Add(_parent.transform.GetChild(i));
                }
                _parent.transform.DetachChildren();
            } else {
                _parent = new GameObject("LivingEntitySpawner".Decorate());
                _parent.transform.SetParent(transform, true);
            }

            if (_livingEntities == null || _livingEntities.Length != numberOfEntities)
            {
                _livingEntities = new LivingEntity[numberOfEntities];
            }

            var sprite = Context.LivingEntityConfig.GetData().sprite;

            for (int i = 0; i < numberOfEntities; i++)
            {
                LivingEntity entity;

                if (cache.Count > 0)
                {
                    entity = cache[cache.Count - 1].GetComponent<LivingEntity>();
                    cache.RemoveAt(cache.Count - 1);
                } else
                {
                    entity = LivingEntity.Create(_LivingEntityPrefab);
                }

                entity.transform.SetParent(_parent.transform, true);
                entity.Sprite = sprite;
                _livingEntities[i] = entity;
            }

            foreach (var c in cache)
            {
                Destroy(c.gameObject);
            }
        }

        void UpdateSimulation()
        {
            // Do physics with enteties.
            _simulation.Update(Context.LivingEntityConfig);

            // Reset the simulation when needed.
            if (_previousNumberOfEntities != numberOfEntities)
            {
                Init();
                _previousNumberOfEntities = numberOfEntities;
            }

            // Create all enteties data if needed.
            _simulation.Populate(numberOfEntities, Context.LivingEntityConfig);
        }

        void PrePareForDrawing()
        {
            int i = 0;

            foreach (var entityData in _simulation.Enteties)
            {
                var livingEntity = _livingEntities[i++];

                livingEntity.Color = entityData.Color;
                livingEntity.transform.position = ((Vector3)entityData.position).WithZ(-entityData.layer);
                livingEntity.transform.localScale = entityData.scale;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            if (Application.isPlaying)
                _simulation.DrawGizmos();
        }

        public override void OnSpawnerSelected()
        {
        }

        public override void OnSpawnerDeselected()
        {
            _livingEntities = null;
        }

        protected override int EntetiesCount => _parent.transform.childCount;

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawName("Each entety is a living GameObject");
        }
    }

}
