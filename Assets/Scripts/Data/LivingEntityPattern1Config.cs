using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP4
{

    [CreateAssetMenu(menuName = Consts.MenuItemRoot + "/LivingEntityData/Pattern1")]
    public class LivingEntityPattern1Config : BaseLivingEntityConfig
    {
        static readonly float ReferenceScaleMagnitude = new Vector2(1, 1).magnitude;

        public Sprite sprite;

        public Color smallPopulationColor;

        public Color mediumPopulationColor;

        public Color largePopulationColor;

        public float radius = 0.5f;

        public override InitData GetData()
        {
            var initData = new InitData();

            const float rotation = 0;
            var rotationQ = Quaternion.AngleAxis(rotation, Vector3.forward);
            var random = Random.Range(0, 1f);

            if (random < 0.01f)
            {
                // Small group
                initData.position = rotationQ * new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                initData.scale = Vector2.one * Random.Range(4, 10f);
                initData.color = smallPopulationColor;
                initData.speed = Random.Range(1f, 5f);
                initData.layer = 0;
            } else if (random < 0.9f)
            {
                // Large group
                initData.position = rotationQ * new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                initData.scale = Vector2.one * Random.Range(0.5f, 2f);
                initData.color = largePopulationColor;
                initData.speed = Random.Range(5f, 30f);
                initData.layer = 2;
            }
            else
            {
                // Medium
                initData.position = rotationQ * new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                initData.scale = Vector2.one * Random.Range(1f, 4f);
                initData.color = mediumPopulationColor;
                initData.speed = Random.Range(2, 15f);
                initData.layer = 1;
            }

            initData.sprite = sprite;
            initData.radius = 0.5f * initData.scale.magnitude / ReferenceScaleMagnitude;
            initData.rotation = rotation;

            return initData;
        }

        public override int NumberOfLayers => 3;
    }

}
