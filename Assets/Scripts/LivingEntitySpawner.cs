using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP4
{

    public class LivingEntitySpawner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        LivingEntity _LivingEntityPrefab;

        public int numberOfEntities = 10;

        #endregion

        int _numberOfEntitiesAlive = 0;

        void Start()
        {
            CreateLivingEntity();
        }

        void CreateLivingEntity()
        {
            Vector3 getPosition()
            {
                var center = GameScene.Instance.SceneBounds.center;
                var size = GameScene.Instance.SceneBounds.size;
                size = new Vector3(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.5f) * size.y);
                return center + size;
            }

            while (_numberOfEntitiesAlive < numberOfEntities)
            {
                LivingEntity.Create(_LivingEntityPrefab, getPosition(), Random.Range(1, 10)).onGoOffScreen += () => { _numberOfEntitiesAlive--; CreateLivingEntity(); };
                _numberOfEntitiesAlive++;
            }
        }
    }

}
