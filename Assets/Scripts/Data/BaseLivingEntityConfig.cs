using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP4
{

    public abstract class BaseLivingEntityConfig : ScriptableObject
    {
        public struct InitData
        {
            public Vector2 position; // Normalized positin (-0.5 .. 0.5, -0.5 .. 0.5)
            public float rotation;
            public Sprite sprite;
            public Color color;
            public float radius;
            public Vector2 scale;
            public float speed;
            public int layer;
        }

        public abstract InitData GetData();

        public virtual int NumberOfLayers => 1;
    }

}
