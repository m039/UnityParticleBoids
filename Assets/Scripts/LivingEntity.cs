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

        Rigidbody2D _rigidbody;

        public System.Action onGoOffScreen;
        
        void Awake()
        {
            _circleCollider = GetComponent<CircleCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.forward);
        }

        void FixedUpdate()
        {
            _rigidbody.position += ((Vector2) _rigidbody.transform.up) * speed * Time.deltaTime;
        }

        void LateUpdate()
        {
            if (!(Physics2DUtils.Within(GameScene.Instance.SceneBounds, _circleCollider.bounds)))
            {
                onGoOffScreen?.Invoke();
            }
        }

    }

}
