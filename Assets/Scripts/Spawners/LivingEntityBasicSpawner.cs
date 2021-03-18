using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class LivingEntityBasicSpawner : BaseSpawner
    {
        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        public int numberOfEntities = 10;

        #endregion

        int _numberOfEntitiesAlive = 0;

        GameObject _parent;

        readonly List<LivingEntity> _entitiesCache = new List<LivingEntity>(4000);

        void CreateLivingEntity()
        {
            while (_numberOfEntitiesAlive < numberOfEntities)
            {
                CreateEntityOrGetFromCache();
                
                _numberOfEntitiesAlive++;
            }
        }

        LivingEntity CreateEntityOrGetFromCache()
        {
            if (_parent == null)
            {
                _parent = new GameObject("LivingEntitySpawner".Decorate());
            }

            Vector3 getPosition()
            {
                var center = GameScene.Instance.SceneBounds.center;
                var size = GameScene.Instance.SceneBounds.size;
                size = new Vector3(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.5f) * size.y);
                return center + size;
            }

            LivingEntity entity;

            if (_entitiesCache.Count > 0)
            {
                // Take from the cache.

                entity = _entitiesCache[_entitiesCache.Count - 1];
                _entitiesCache.RemoveAt(_entitiesCache.Count - 1);

                entity.transform.position = getPosition();
                entity.speed = Random.Range(1, 10);
                entity.gameObject.SetActive(true);
            } else
            {
                // Create a new one.

                entity = LivingEntity.Create(_LivingEntityPrefab, getPosition(), Random.Range(1, 10));
                entity.onGoOffScreen += () => OnGoOffScreen(entity);
                entity.transform.SetParent(_parent.transform, true);
            }

            return entity;
        }

        void OnGoOffScreen(LivingEntity entity)
        {
            _numberOfEntitiesAlive--;
            entity.gameObject.SetActive(false);
            _entitiesCache.Add(entity);
            CreateLivingEntity();
        }

        public override void OnSpawnerSelected()
        {
            Invoke(nameof(CreateLivingEntity), 0.01f);
        }

        public override void OnSpawnerDeselected()
        {
            if (_parent != null)
            {
                Destroy(_parent);
                _parent = null;
                _numberOfEntitiesAlive = 0;
            }
        }

        protected override void PerformOnGUI(IDrawer drawer)
        {
            base.PerformOnGUI(drawer);

            drawer.DrawStatFrame(1);
            drawer.DrawStat(0, "Enteties: " + _numberOfEntitiesAlive);

            drawer.DrawName("GameObject, Transparent [Basic]");

            drawer.DrawGetNumber("Number of Enteties [" + numberOfEntities + "]:", ref numberOfEntities);
        }
    }

}
