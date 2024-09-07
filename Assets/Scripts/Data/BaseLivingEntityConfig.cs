using UnityEngine;

namespace GP4
{

    public abstract class BaseLivingEntityConfig : ScriptableObject
    {
        public string Name;

        public struct InitData
        {
            /// <summary>
            /// Normalized position (-0.5 .. 0.5, -0.5 .. 0.5)
            /// </summary>
            public Vector2 position;
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
