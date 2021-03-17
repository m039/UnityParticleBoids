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

        public GameObject BigTriangle
        {
            get
            {
                return transform.Find("Head").gameObject;
            }
        }

        public GameObject SmallTriangle
        {
            get
            {
                return transform.Find("Head (1)").gameObject;
            }
        }

        protected CircleCollider2D CircleCollider
        {
            get
            {
                if (_circleCollider == null)
                {
                    _circleCollider = GetComponent<CircleCollider2D>();
                }

                return _circleCollider;
            }
        }

        public float Radius => CircleCollider.radius;

        public System.Action onGoOffScreen;
        
        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.forward);
        }

        void FixedUpdate()
        {
            _rigidbody.position += ((Vector2) _rigidbody.transform.up) * speed * Time.deltaTime;
        }

        void LateUpdate()
        {
            if (!(Physics2DUtils.Within(GameScene.Instance.SceneBounds, CircleCollider.bounds)))
            {
                onGoOffScreen?.Invoke();
            }
        }

    }

}
