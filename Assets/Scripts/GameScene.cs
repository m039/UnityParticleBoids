using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class GameScene : SingletonMonoBehaviour<GameScene>
    {

        public Bounds SceneBounds {
            get
            {
                var sceneBounds = _sceneCollider.bounds;
                sceneBounds.center = sceneBounds.center.WithZ(0);
                sceneBounds.size = sceneBounds.size.WithZ(0);
                return sceneBounds;
            }
        }

        BoxCollider2D _sceneCollider;

        void Awake()
        {
            var camera = Camera.main;
            var height = camera.orthographicSize * 2;
            var width = height * camera.aspect;

            _sceneCollider = camera.gameObject.AddComponent<BoxCollider2D>();
            _sceneCollider.size = new Vector2(width, height);
        }

    }

}
