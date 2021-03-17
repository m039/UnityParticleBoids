using System.Collections.Generic;
using UnityEngine;

using m039.Common;
using static m039.Common.UIUtils;

namespace GP4
{

    public class GameScene : SingletonMonoBehaviour<GameScene>
    {
        enum SpawnerType
        {
            Basic = 0,
            DrawMesh = 1
        }

        #region Inspector

        [SerializeField]
        SpawnerType _SelectedType = SpawnerType.Basic;

        [SerializeField]
        BaseSpawner _BasicSpawner;

        [SerializeField]
        BaseSpawner _DrawMeshSpawner;

        #endregion

        public Bounds SceneBounds {
            get
            {
                if (!_lastBounds.HasValue)
                    UpdateBounds();

                return _lastBounds.Value;
            }
        }

        Bounds? _lastBounds;

        SpawnerType? _type;

        readonly List<BaseSpawner> _spawners = new List<BaseSpawner>();

        ComboBox _comboBox;

        void OnValidate()
        {
            UpdateType();
        }

        void Start()
        {
            UpdateType();
        }

        void LateUpdate()
        {
            UpdateBounds(); // Updates the bounds only when needed.
        }


        bool IsTypeEquals(BaseSpawner spawner, SpawnerType type)
        {
            if (spawner is LivingEntityBasicSpawner && type == SpawnerType.Basic)
            {
                return true;
            }
            else if (spawner is LivingEntityDrawMeshSpawner && type == SpawnerType.DrawMesh)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void UpdateType()
        {
            if (!_type.HasValue)
            {
                _spawners.Add(_BasicSpawner);
                _spawners.Add(_DrawMeshSpawner);

                _spawners.ForEach((s) => s.gameObject.SetActive(false));

                _type = _SelectedType;

                // First init.

                foreach (var spawner in _spawners)
                {
                    if (IsTypeEquals(spawner, _type.Value))
                    {
                        spawner.gameObject.SetActive(true);
                        spawner.SetSelected(true);
                    } else
                    {
                        spawner.gameObject.SetActive(false);
                    }
                }

            } else if (_type.Value != _SelectedType)
            {
                var previousType = _type.Value;
                _type = _SelectedType;

                // Deselect the previous spawner.

                foreach (var spawner in _spawners)
                {
                    if (IsTypeEquals(spawner, previousType))
                    {
                        spawner.SetSelected(false);
                        spawner.gameObject.SetActive(false);
                        break;
                    }
                }

                // Select the new spawner.

                foreach (var spawner in _spawners)
                {
                    if (IsTypeEquals(spawner, _type.Value))
                    {
                        spawner.gameObject.SetActive(true);
                        spawner.SetSelected(true);
                    }
                }
            }

            // Update comboBox

            if (_comboBox != null)
            {
                _comboBox.SelectedItemIndex = (int)_type;
            }
        }

        void UpdateBounds()
        {
            var camera = Camera.main;
            var height = camera.orthographicSize * 2;
            var width = height * camera.aspect;

            _lastBounds = new Bounds(Camera.main.transform.position.WithZ(0), new Vector2(width, height));
        }

        void OnGUI()
        {
            if (_comboBox == null)
            {              
                var width = 400 * UICoeff + 32 * 2 * UICoeff;
                var height = 100 * UICoeff + 32 * 2 * UICoeff;
                var x = Screen.width - width - 100 * UICoeff + 32 * 1 * UICoeff;
                var y = Screen.height - height - 100 * UICoeff + 32 * 1 * UICoeff;

                var rect = new Rect(x, y, width, height);

                var comboBoxList = new GUIContent[2];
                comboBoxList[0] = new GUIContent("Basic");
                comboBoxList[1] = new GUIContent("Draw Mesh");

                var buttonStyle = new GUIStyle("button");
                buttonStyle.fontSize = (int)(60f * UICoeff);

                var boxStyle = new GUIStyle("box");

                var listStyle = new GUIStyle();
                listStyle.fontSize = (int)(60f * UICoeff);
                listStyle.normal.textColor = Color.white;
                listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
                listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

                _comboBox = new ComboBox(
                    rect,
                    comboBoxList,
                    buttonStyle,
                    boxStyle,
                    listStyle
                    );
                _comboBox.OnItemSelected += (i, userHasPressed) => {
                    if (!userHasPressed) return;

                    if (i == 0)
                    {
                        _SelectedType = SpawnerType.Basic;
                    }
                    else if (i == 1)
                    {
                        _SelectedType = SpawnerType.DrawMesh;
                    }

                    UpdateType();
                };
                _comboBox.Direction = ComboBox.PopupDirection.FromBottomToTop;
                _comboBox.SelectedItemIndex = _type.HasValue? (int) _type.Value : 0;

            }

            _comboBox.Show();
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            GizmosUtils.DrawRect(SceneBounds.ToRect());
        }

    }

}
