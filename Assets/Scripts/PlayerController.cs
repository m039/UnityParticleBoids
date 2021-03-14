using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GP4
{

    public class PlayerController : MonoBehaviour
    {
        #region Inspector

        public float rotationSpeed = 5f;

        public float gravity = 10f;

        #endregion

        bool _isPlayerReady = false;

        GUIStyle _readyButtonStyle;

        private void OnGUI()
        {
            if (!_isPlayerReady)
            {
                var width = 200;
                var height = 100;

                if (_readyButtonStyle == null)
                {
                    _readyButtonStyle = new GUIStyle(GUI.skin.box);
                    _readyButtonStyle.alignment = TextAnchor.MiddleCenter;
                }

                GUI.Label(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height), "Press Any Key To Start", _readyButtonStyle);
            }
        }

        void Update()
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
                _isPlayerReady = true;
        }

        void FixedUpdate()
        {
            if (!_isPlayerReady)
                return;

            transform.Translate(Vector3.down * gravity * Time.deltaTime);
        }
    }

}
