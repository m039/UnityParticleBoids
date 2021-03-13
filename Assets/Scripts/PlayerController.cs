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

        // Update is called once per frame
        void Update()
        {
            if (InputController.Instance.RotateLeft.IsPressed)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            } else if (InputController.Instance.RotateRight.IsPressed)
            {
                transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
            }
        }
    }

}
