using m039.Common;
using System.Collections.Generic;
using UnityEngine;

namespace GP4
{

    public abstract class BaseSimulationSpawner : BaseSpawner
    {
        public const float AlphaFadeOutSpeed = 0.56f;

        LivingEntitySimulation _simulation;

        protected LivingEntitySimulation Simulation => _simulation;

        int _previousNumberOfEntities = -1;

        protected override void OnEnable()
        {
            base.OnEnable();

            InitSimulation();
        }

        void InitSimulation()
        {
            /// Create simulation

            _simulation = new LivingEntitySimulation()
            {
                entetiesReferenceScale = () => entetiesReferenceScale,
                entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                entetiesReferenceSpeed = () => entetiesReferenceSpeed
            };

            OnInitSimulation();
        }

        protected abstract void OnInitSimulation();

        protected abstract void OnDrawSimulation();

        protected virtual void LateUpdate()
        {
            UpdateSimulation();
            OnDrawSimulation();
        }

        void UpdateSimulation()
        {
            // Do physics with enteties.
            _simulation.Update(Context.LivingEntityConfig);

            // Reset the simulation when needed.
            if (_previousNumberOfEntities != numberOfEntities)
            {
                InitSimulation();
                _previousNumberOfEntities = numberOfEntities;
            }

            // Create all enteties data if needed.
            _simulation.Populate(numberOfEntities, Context.LivingEntityConfig);
        }

        public override void OnSpawnerSelected()
        {
        }

        public override void OnSpawnerDeselected()
        {
            _simulation.Reset();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            if (Application.isPlaying)
                _simulation.DrawGizmos();
        }

        protected override int EntetiesCount => _simulation.Enteties.Count;

        public class LivingEntitySimulation
        {
            readonly static LivingEntityDataComparer _sEntetiesComparer = new LivingEntityDataComparer();

            readonly List<LivingEntityData> _enteties = new List<LivingEntityData>();

            public GetSettingValue<float> entetiesReferenceScale = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceAlpha = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceSpeed = () => 1.0f;

            public GetSettingValue<Bounds> sceneBounds => () => GameScene.Instance.SceneBounds;

            public List<LivingEntityData> Enteties => _enteties;

            public void Populate(int numberOfEntities, BaseLivingEntityConfig entetyConfig)
            {
                while (_enteties.Count < numberOfEntities)
                {
                    LivingEntityData entityData = null;
                    CreateOrReinitLivingEntityData(entetyConfig, ref entityData);
                    _enteties.Add(entityData);
                }
            }

            void CreateOrReinitLivingEntityData(BaseLivingEntityConfig entetyConfig, ref LivingEntityData entityData)
            {
                Vector3 getPosition(Vector2 normalizedPosition)
                {
                    var sb = sceneBounds();
                    var center = sb.center;
                    var size = sb.size;
                    size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                    return center + size;
                }

                if (entityData == null)
                {
                    entityData = new LivingEntityData();
                }

                var initData = entetyConfig.GetData();

                entityData.position = getPosition(initData.position);
                entityData.rotation = initData.rotation;
                entityData.scale = initData.scale;
                entityData.speed = initData.speed;
                entityData.baseColor = initData.color;
                entityData.layer = initData.layer;
                entityData.radius = initData.radius;
                entityData.alpha = 0f;
            }

            public void Update(BaseLivingEntityConfig entityConfig)
            {
                var tSceneBounds = sceneBounds();
                var deltaTime = Time.deltaTime;
                var refenceScale = entetiesReferenceScale();
                var referenceAlpha = entetiesReferenceAlpha();
                var referenceSpeed = entetiesReferenceSpeed();

                bool updateEntity(LivingEntityData data)
                {
                    var rotation = Quaternion.AngleAxis(data.rotation, Vector3.forward);
                    var deltaPosition = (Vector2)(rotation * Vector3.up * (data.speed * referenceSpeed) * deltaTime);

                    data.position += deltaPosition;
                    data.scaleFactor = refenceScale;
                    data.alphaFactor = referenceAlpha;

                    if (data.alpha < 1)
                    {
                        data.alpha = Mathf.Clamp(data.alpha + deltaTime * AlphaFadeOutSpeed, 0, 1);
                    }

                    var boundRadius = data.scaleFactor * data.radius;

                    return Physics2DUtils.CircleWithin(tSceneBounds, data.position, boundRadius);
                }

                for (int i = 0; i < _enteties.Count; i++)
                {
                    var entityData = _enteties[i];

                    if (!updateEntity(entityData))
                    {
                        CreateOrReinitLivingEntityData(entityConfig, ref entityData);
                    }
                }

                _enteties.Sort(_sEntetiesComparer);
            }

            public void DrawGizmos()
            {
                var tSceneBounds = sceneBounds();

                // Draw direction

                foreach (var data in Enteties)
                {
                    var boundRadius = data.scaleFactor * data.radius;

                    // Draw a circle around the entity.

                    if (Physics2DUtils.CircleWithin(tSceneBounds, data.position, boundRadius))
                    {
                        Gizmos.color = Color.blue.WithAlpha(0.5f * data.alpha);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow.WithAlpha(0.5f * data.alpha);
                    }

                    Gizmos.DrawWireSphere(data.position, boundRadius);

                    // Draw the direction.

                    Gizmos.color = Color.green;

                    Gizmos.DrawLine(data.position, (Vector3)data.position + Quaternion.AngleAxis(data.rotation, Vector3.forward) * Vector3.up * boundRadius);
                }
            }

            public void Reset()
            {
                _enteties.Clear();
            }

            class LivingEntityDataComparer : IComparer<LivingEntityData>
            {
                public int Compare(LivingEntityData x, LivingEntityData y)
                {
                    if (x == null && y != null)
                        return -1;

                    if (x != null && y == null)
                        return 1;

                    if (x == null && y == null)
                        return 0;

                    var compare = x.layer.CompareTo(y.layer);
                    if (compare == 0)
                    {
                        return x.__id.CompareTo(y.__id);
                    }
                    else
                    {
                        return compare;
                    }
                }
            }
        }

        public class LivingEntityData
        {
            static long _sId = 0;

            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float scaleFactor = 1f;

            public float speed;

            public Color baseColor;

            public float alpha;

            public float alphaFactor = 1f;

            public int layer;

            public float radius;

            internal long __id;

            public LivingEntityData()
            {
                __id = _sId++;
            }

            public Vector2 Scale
            {
                get
                {
                    return scale * scaleFactor;
                }
            }

            public Color Color
            {
                get
                {
                    return new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha * alphaFactor);
                }
            }
        }
    }

}
