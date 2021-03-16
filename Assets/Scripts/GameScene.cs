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
                var camera = Camera.main;
                var height = camera.orthographicSize * 2;
                var width = height * camera.aspect;

                return new Bounds(Camera.main.transform.position.WithZ(0), new Vector2(width, height));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            GizmosUtils.DrawRect(SceneBounds.ToRect());
        }

    }

}
