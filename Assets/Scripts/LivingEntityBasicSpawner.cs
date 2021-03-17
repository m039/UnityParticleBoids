using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class LivingEntityBasicSpawner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        public int numberOfEntities = 10;

        #endregion

        [HideInInspector]
        [SerializeField]
        int _numberOfEntitiesAlive = 0;

        GameObject _parent;

        readonly List<LivingEntity> _entitiesCache = new List<LivingEntity>(4000);

        private void Start()
        {
            CreateLivingEntity();
        }

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
                entity = _entitiesCache[_entitiesCache.Count - 1];
                _entitiesCache.RemoveAt(_entitiesCache.Count - 1);

                entity.transform.position = getPosition();
                entity.speed = Random.Range(1, 10);
                entity.gameObject.SetActive(true);
            } else
            {
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
    }

}
