using UnityEngine;
using UnityEngine.InputSystem;
using m039.Common;

namespace GP4
{

    public class InputController : SingletonMonoBehaviourFromResources<InputController>
    {
        public class KeyControlAdapter {

            UnityEngine.InputSystem.Controls.KeyControl _keyControl;

            public KeyControlAdapter(UnityEngine.InputSystem.Controls.KeyControl keyControl)
            {
                _keyControl = keyControl;
            }

            public bool IsPressed => _keyControl.isPressed;
        }

        #region Inspector

        [SerializeField]
        bool _Test = false;

        #endregion

        KeyControlAdapter _rotateLeft;

        KeyControlAdapter _rotateRight;

        private void Awake()
        {
            _rotateLeft = new KeyControlAdapter(Keyboard.current.qKey);
            _rotateRight = new KeyControlAdapter(Keyboard.current.eKey);
        }

        public KeyControlAdapter RotateLeft => _rotateLeft;

        public KeyControlAdapter RotateRight => _rotateRight;

        protected override bool UseResourceFolder => true;

        protected override string PathToResource => "InputController";

        protected override bool ShouldDestroyOnLoad => false;

    }

}
