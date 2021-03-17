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
                if (!_lastBounds.HasValue)
                    UpdateBounds();

                return _lastBounds.Value;
            }
        }

        Bounds? _lastBounds;

        void LateUpdate()
        {
            UpdateBounds(); // Updates the bounds only when needed.
        }

        void UpdateBounds()
        {
            var camera = Camera.main;
            var height = camera.orthographicSize * 2;
            var width = height * camera.aspect;

            _lastBounds = new Bounds(Camera.main.transform.position.WithZ(0), new Vector2(width, height));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            GizmosUtils.DrawRect(SceneBounds.ToRect());
        }

    }

}
