using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class LivingEntity : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _renderer;

        #endregion

        public Color Color
        {
            get
            {
                return _renderer.color;
            }

            set
            {
                _renderer.color = value;
            }
        }

        public Sprite Sprite
        {
            get
            {
                return _renderer.sprite;
            }

            set
            {
                _renderer.sprite = value;
            }
        }

        public static LivingEntity Create(LivingEntity prefab)
        {
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            return obj;
        }

    }

}
