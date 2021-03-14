using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class LivingEntity : MonoBehaviour
    {
        public static LivingEntity Create(LivingEntity prefab, Vector3 position, float speed)
        {
            var obj = Instantiate(prefab, position, Quaternion.identity);
            obj.speed = speed;
            return obj;
        }

        #region Inspector

        public float speed = 10f;

        #endregion

        CircleCollider2D _circleCollider;

        public System.Action onGoOffScreen;
        
        void Awake()
        {
            _circleCollider = GetComponent<CircleCollider2D>();
            transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
        }

        void FixedUpdate()
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }

        void LateUpdate()
        {
            if (!(Physics2DUtils.Within(GameScene.Instance.SceneBounds, _circleCollider.bounds)))
            {
                onGoOffScreen?.Invoke();
                onGoOffScreen = null;
                Destroy(gameObject);
            }
        }

    }

}
