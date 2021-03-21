using UnityEngine;
using m039.Common;

using System.Collections.Generic;

namespace GP4
{

    public class LivingEntityBasicSpawner : BaseSimulationSpawner
    {
        #region Inspector

        [Header("Basic Spawner Settings")]
        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        #endregion

        LivingEntity[] _livingEntities;

        GameObject _parent;

        protected override void OnInitSimulation()
        {
            // Create gameObjects

            var cache = new List<Transform>(); // To reuse gameObjects from the parent.

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

        protected override void OnDrawSimulation()
        {
            int i = 0;

            foreach (var entityData in Simulation.Enteties)
            {
                var livingEntity = _livingEntities[i++];

                livingEntity.Color = entityData.Color;
                livingEntity.transform.position = entityData.Position;
                livingEntity.transform.localScale = entityData.Scale;
                livingEntity.transform.rotation = entityData.Rotation;
            }
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawInfo("Each entety is a living GameObject");
        }
    }

}
