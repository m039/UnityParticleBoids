using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m039.Common;

namespace GP4
{

    public class GameScene : SingletonMonoBehaviour<GameScene>
    {
        enum SpawnerType
        {
            Basic,
            DrawMesh
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
