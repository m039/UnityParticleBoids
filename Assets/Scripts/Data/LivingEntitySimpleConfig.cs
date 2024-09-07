using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP4
{

    [CreateAssetMenu(menuName = Consts.MenuItemRoot + "/LivingEntityData/Simple")]
    public class LivingEntitySimpleConfig : BaseLivingEntityConfig
    {
        public Sprite sprite;

        public Color color;

        public float radius = 0.5f;

        public override InitData GetData()
        {
            return new InitData
            {
                color = color,
                radius = radius,
                sprite = sprite,
                scale = Vector2.one,
                speed = Random.Range(0.1f, 1f),
                position = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
                rotation = Random.Range(0, 360f)
            };
        }
    }

}
