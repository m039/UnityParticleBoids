using System.Collections.Generic;
using UnityEngine;

using m039.Common;
using static m039.Common.UIUtils;
using UnityEngine.InputSystem;

namespace GP4
{

    public class GameScene : SingletonMonoBehaviour<GameScene>, ISpawnerContext
    {
        enum SpawnerType
        {
            Basic = 0,
            DrawMesh = 1,
            OneMesh = 2,
            ParticleSystem = 3
        }

        #region Inspector

        [SerializeField]
        SpawnerType _SelectedType = SpawnerType.Basic;

        [SerializeField]
        BaseLivingEntityConfig _LivingEntityData;

        [SerializeField]
        BaseLivingEntityConfig[] _LivingEntityDatas;

        [SerializeField]
        bool _GUIVisibility = true;

        #endregion

        public Bounds SceneBounds {
            get
            {
                if (!_lastBounds.HasValue)
                    UpdateBounds();

                return _lastBounds.Value;
            }
        }

        public BaseLivingEntityConfig LivingEntityConfig => _LivingEntityData;

        public bool GUIVisibility => _GUIVisibility;

        public event System.Action OnLivingEntityDataChanged;

        Bounds? _lastBounds;

        SpawnerType? _type;

        readonly List<BaseSpawner> _spawners = new List<BaseSpawner>();

        ComboBox _spawnerComboBox;

        ComboBox _configComboBox;

        BaseLivingEntityConfig _lastLivingEntityData;

        FPSDisplay _fpsDisplay;

        private void Awake()
        {
            _lastLivingEntityData = _LivingEntityData;
            _fpsDisplay = FindObjectOfType<FPSDisplay>();
        }

        void OnValidate()
        {
            UpdateType();
            UpdateLivingEntityData();
        }

        void Start()
        {
            UpdateType();
        }

        void LateUpdate()
        {
            UpdateBounds(); // Updates the bounds only when needed.
            UpdateUI();
        }

        bool IsTypeEquals(BaseSpawner spawner, SpawnerType type)
        {
            if (spawner is LivingEntityBasicSpawner && type == SpawnerType.Basic ||
                spawner is LivingEntityDrawMeshSpawner && type == SpawnerType.DrawMesh ||
                spawner is LivingEntityOneMeshSpawner && type == SpawnerType.OneMesh ||
                spawner is LivingEntityParticleSystemSpawner && type == SpawnerType.ParticleSystem)
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
                _spawners.AddRange(GetComponentsInChildren<BaseSpawner>(true));
                _spawners.ForEach((s) =>
                {
                    s.gameObject.SetActive(false);
                });

                _type = _SelectedType;

                // First init.

                foreach (var spawner in _spawners)
                {
                    if (IsTypeEquals(spawner, _type.Value))
                    {
                        spawner.gameObject.SetActive(true);
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
                    }
                }
            }

            // Update comboBox

            if (_spawnerComboBox != null)
            {
                _spawnerComboBox.SelectedItemIndex = (int)_type;
            }
        }

        void UpdateBounds()
        {
            var camera = Camera.main;
            var height = camera.orthographicSize * 2;
            var width = height * camera.aspect;

            _lastBounds = new Bounds(Camera.main.transform.position.WithZ(0), new Vector2(width, height));
        }

        void UpdateUI()
        {
            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                _GUIVisibility = !_GUIVisibility;
            }

            _fpsDisplay.Visibility = _GUIVisibility;
        }

        void UpdateLivingEntityData()
        {
            if (_lastLivingEntityData != _LivingEntityData)
            {
                OnLivingEntityDataChanged?.Invoke();
                _lastLivingEntityData = _LivingEntityData;
            }
        }

        void OnGUI()
        {
            const float spawnerBoxWidth = 440f;

            if (_spawnerComboBox == null)
            {              
                var width = spawnerBoxWidth * UICoeff;
                var height = UIMediumMargin + UISmallMargin * 2;
                var x = Screen.width - width - UIMediumMargin;
                var y = Screen.height - height - UIMediumMargin;

                var rect = new Rect(x, y, width, height);

                var comboBoxList = new GUIContent[]
                {
                    new GUIContent("GameObject"), // Basic
                    new GUIContent("Draw Mesh"),
                    new GUIContent("One Mesh"),
                    new GUIContent("Particle System")
                };

                var buttonStyle = new GUIStyle("button");
                buttonStyle.fontSize = (int)(60f * UICoeff);

                var boxStyle = new GUIStyle("box");

                var listStyle = new GUIStyle();
                listStyle.fontSize = (int)(60f * UICoeff);
                listStyle.normal.textColor = Color.white;
                listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
                listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

                _spawnerComboBox = new ComboBox(
                    rect,
                    comboBoxList,
                    buttonStyle,
                    boxStyle,
                    listStyle
                    );
                _spawnerComboBox.OnItemSelected += (i, userHasPressed) => {
                    if (!userHasPressed) return;

                    _SelectedType = (SpawnerType)i;

                    UpdateType();
                };
                _spawnerComboBox.Direction = ComboBox.PopupDirection.FromBottomToTop;
                _spawnerComboBox.SelectedItemIndex = _type.HasValue? (int) _type.Value : 0;
            }

            if (_configComboBox == null)
            {
                var width = 550 * UICoeff;
                var height = UIMediumMargin + UISmallMargin * 2;
                var x = Screen.width - UIMediumMargin - spawnerBoxWidth * UICoeff - UISmallMargin - width;
                var y = Screen.height - height - UIMediumMargin;

                var rect = new Rect(x, y, width, height);

                var comboBoxList = new GUIContent[_LivingEntityDatas.Length];

                for (int i = 0; i < comboBoxList.Length; i++)
                {
                    comboBoxList[i] = new GUIContent(_LivingEntityDatas[i].Name);
                }

                var buttonStyle = new GUIStyle("button");
                buttonStyle.fontSize = (int)(60f * UICoeff);

                var boxStyle = new GUIStyle("box");

                var listStyle = new GUIStyle();

                listStyle.fontSize = (int)(60f * UICoeff);
                listStyle.normal.textColor = Color.white;
                listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
                listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

                _configComboBox = new ComboBox(
                    rect,
                    comboBoxList,
                    buttonStyle,
                    boxStyle,
                    listStyle
                    );
                _configComboBox.OnItemSelected += (i, userHasPressed) => {
                    if (!userHasPressed) return;

                    _LivingEntityData = _LivingEntityDatas[i];

                    UpdateLivingEntityData();
                };

                _configComboBox.Direction = ComboBox.PopupDirection.FromBottomToTop;
                _configComboBox.SelectedItemIndex = System.Array.FindIndex(_LivingEntityDatas, (d) => d.Equals(_LivingEntityData));
            }

            if (GUIVisibility)
            {
                _spawnerComboBox.Show();
                _configComboBox.Show();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            GizmosUtils.DrawRect(SceneBounds.ToRect());
        }

    }

}
