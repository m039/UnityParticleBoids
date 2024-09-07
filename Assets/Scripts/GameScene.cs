using System.Collections.Generic;
using UnityEngine;

using m039.Common;
using static m039.Common.UIUtils;
using UnityEngine.InputSystem;
using m039.Common.DependencyInjection;
using m039.UIToolbox;
using Game;
using System;
using System.Linq;

namespace GP4
{

    public class GameScene : SingletonMonoBehaviour<GameScene>, ISpawnerContext
    {
        enum SpawnerType
        {
            GameObject = 0,
            DrawMesh = 1,
            OneMesh = 2,
            ParticleSystem = 3,
            GPU = 4
        }

        #region Inspector

        [SerializeField]
        SpawnerType _SelectedType = SpawnerType.GameObject;

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
                var camera = Camera.main;
                var height = camera.orthographicSize * 2;
                var width = height * camera.aspect;

                return new Bounds(Camera.main.transform.position.WithZ(0), new Vector2(width, height));
            }
        }

        public BaseLivingEntityConfig LivingEntityConfig => _LivingEntityData;

        public bool GUIVisibility => _GUIVisibility;

        public event System.Action OnLivingEntityDataChanged;

        SpawnerType? _type;

        readonly List<BaseSpawner> _spawners = new List<BaseSpawner>();

        ComboBox _spawnerComboBox;

        ComboBox _configComboBox;

        BaseLivingEntityConfig _lastLivingEntityData;

        [Inject]
        public NotificationMessage notificationMessage { get; private set; }

        [Inject]
        ModularPanel _modularPanel;

        [NonSerialized]
        public int numberOfEntities = 2000;

        [NonSerialized]
        public float entetiesReferenceSpeed = 0.11f;

        [NonSerialized]
        public float entetiesReferenceScale = 0.2f;

        [NonSerialized]
        public float entetiesReferenceAlpha = 0.7f;

        void Awake()
        {
            _lastLivingEntityData = _LivingEntityData;
            CreatePanel();
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

        void CreatePanel()
        {
            if (_modularPanel == null)
                return;

            var builder = _modularPanel.CreateBuilder();

            var numberOfEntitiesItem = new ModularPanel.SliderItem(numberOfEntities, 0, 100000)
            {
                label = "Entities",
                valueFormat = "0"
            };
            numberOfEntitiesItem.onValueChanged += v => numberOfEntities = (int)v;
            builder.AddItem(numberOfEntitiesItem);

            var entetiesReferenceSpeedItem = new ModularPanel.SliderItem(entetiesReferenceSpeed, 0f, 10f)
            {
                label = "Speed"
            };
            entetiesReferenceSpeedItem.onValueChanged += v => entetiesReferenceSpeed = v;
            builder.AddItem(entetiesReferenceSpeedItem);

            var entetiesReferenceScaleItem = new ModularPanel.SliderItem(entetiesReferenceScale, 0.01f, 1f)
            {
                label = "Scale"
            };
            entetiesReferenceScaleItem.onValueChanged += v => entetiesReferenceScale = v;
            builder.AddItem(entetiesReferenceScaleItem);

            var entetiesReferenceAlphaItem = new ModularPanel.SliderItem(entetiesReferenceAlpha, 0f, 1f)
            {
                label = "Alpha"
            };
            entetiesReferenceAlphaItem.onValueChanged += v => entetiesReferenceAlpha = v;
            builder.AddItem(entetiesReferenceAlphaItem);

            var spawnerTypeItem = new ModularPanel.DropdownEnumItem(typeof(SpawnerType), "Mode");
            spawnerTypeItem.value = (int)_SelectedType;
            spawnerTypeItem.onValueChanged += v =>
            {
                _SelectedType = (SpawnerType)v;
                UpdateType();
            };
            builder.AddItem(spawnerTypeItem);

            var patterns = new ModularPanel.DropdownItem(Array.IndexOf(_LivingEntityDatas, _LivingEntityData), "Patterns");
            patterns.onValueChanged += v =>
            {
                _LivingEntityData = _LivingEntityDatas[v];
                UpdateLivingEntityData();
            };
            patterns.options = _LivingEntityDatas.Select(d => d.Name).ToList();
            builder.AddItem(patterns);

            builder.Build();
        }

        void LateUpdate()
        {
            UpdateUI();
        }

        bool IsTypeEquals(BaseSpawner spawner, SpawnerType type)
        {
            if (spawner is LivingEntityBasicSpawner && type == SpawnerType.GameObject ||
                spawner is LivingEntityDrawMeshSpawner && type == SpawnerType.DrawMesh ||
                spawner is LivingEntityOneMeshSpawner && type == SpawnerType.OneMesh ||
                spawner is LivingEntityParticleSystemSpawner && type == SpawnerType.ParticleSystem ||
                spawner is LivingEntityGPUSpawner && type == SpawnerType.GPU)
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

        void UpdateUI()
        {
            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                _GUIVisibility = !_GUIVisibility;
            }
        }

        void UpdateLivingEntityData()
        {
            if (_lastLivingEntityData != _LivingEntityData)
            {
                OnLivingEntityDataChanged?.Invoke();
                _lastLivingEntityData = _LivingEntityData;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            GizmosUtils.DrawRect(SceneBounds.ToRect());
        }

    }

}
