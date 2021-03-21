using m039.Common;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GP4
{

    public abstract class BaseSimulationSpawner : BaseSpawner
    {
        public const float AlphaFadeOutSpeed = 0.56f;

        LivingEntitySimulation _simulation;

        protected LivingEntitySimulation Simulation => _simulation;

        int _previousNumberOfEntities = -1;

        protected virtual bool UseSort => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            InitSimulation();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_simulation != null)
            {
                _simulation.Reset();
            }
        }

        void InitSimulation()
        {
            /// Create simulation

            _simulation = new LivingEntitySimulation()
            {
                entetiesReferenceScale = () => entetiesReferenceScale,
                entetiesReferenceAlpha = () => entetiesReferenceAlpha,
                entetiesReferenceSpeed = () => entetiesReferenceSpeed,
                useSort = () => UseSort
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

        protected override void OnLivingEntityDataChanged()
        {
            base.OnLivingEntityDataChanged();

            InitSimulation();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!useGizmos)
                return;

            if (Application.isPlaying)
                _simulation.DrawGizmos();
        }

        public class LivingEntitySimulation
        {
            readonly static LivingEntityDataComparer _sEntetiesComparer = new LivingEntityDataComparer();

            NativeArray<LivingEntityData> _enteties;

            public GetSettingValue<float> entetiesReferenceScale = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceAlpha = () => 1.0f;

            public GetSettingValue<float> entetiesReferenceSpeed = () => 1.0f;

            public GetSettingValue<Bounds> sceneBounds => () => GameScene.Instance.SceneBounds;

            public GetSettingValue<bool> useSort = () => false;

            public NativeArray<LivingEntityData> Enteties => _enteties;

            public void Populate(int numberOfEntities, BaseLivingEntityConfig entetyConfig)
            {
                if (!_enteties.IsCreated || _enteties.Length != numberOfEntities)
                {
                    if (_enteties.IsCreated)
                        _enteties.Dispose();

                    _enteties = new NativeArray<LivingEntityData>(numberOfEntities, Allocator.Persistent);

                    for (int i = 0; i < _enteties.Length; i++)
                    {
                        _enteties[i] = CreateOrReinitLivingEntityData(entetyConfig);
                    }
                }
            }

            LivingEntityData CreateOrReinitLivingEntityData(BaseLivingEntityConfig entetyConfig)
            {
                Vector3 getPosition(Vector2 normalizedPosition)
                {
                    var sb = sceneBounds();
                    var center = sb.center;
                    var size = sb.size;
                    size = new Vector3(normalizedPosition.x * size.x, normalizedPosition.y * size.y);
                    return center + size;
                }

                LivingEntityData entityData = new LivingEntityData();

                var initData = entetyConfig.GetData();

                entityData.position = getPosition(initData.position);
                entityData.rotation = initData.rotation;
                entityData.scale = initData.scale;
                entityData.speed = initData.speed;
                entityData.baseColor = initData.color;
                entityData.layer = initData.layer;
                entityData.radius = initData.radius;
                entityData.alpha = 0f;

                return entityData;
            }

            struct UpdateEntityJobParallel : IJobParallelFor
            {
                public Bounds sceneBounds;

                public float deltaTime;

                public float referenceScale;

                public float referenceAlpha;

                public float referenceSpeed;

                public NativeArray<LivingEntityData> enteties;

                public void Execute(int index)
                {
                    var data = enteties[index];
                    var deltaPosition = (Vector2)(data.Rotation * Vector3.up * data.speed * referenceSpeed * deltaTime);

                    data.position += deltaPosition;
                    data.scaleFactor = referenceScale;
                    data.alphaFactor = referenceAlpha;

                    if (data.alpha < 1)
                    {
                        data.alpha = Mathf.Clamp(data.alpha + deltaTime * AlphaFadeOutSpeed, 0, 1);
                    }

                    var boundRadius = data.scaleFactor * data.radius;

                    data.isDestroyed = !Physics2DUtils.CircleWithin(sceneBounds, data.position, boundRadius);

                    enteties[index] = data;
                }
            }

            public void Update(BaseLivingEntityConfig entityConfig)
            {
                var tSceneBounds = sceneBounds();
                var deltaTime = Time.deltaTime;
                var referenceScale = entetiesReferenceScale();
                var referenceAlpha = entetiesReferenceAlpha();
                var referenceSpeed = entetiesReferenceSpeed();

                new UpdateEntityJobParallel
                {
                    sceneBounds = sceneBounds(),
                    deltaTime = Time.deltaTime,
                    referenceScale = entetiesReferenceScale(),
                    referenceAlpha = entetiesReferenceAlpha(),
                    referenceSpeed = entetiesReferenceSpeed(),
                    enteties = _enteties
                }.Schedule(_enteties.Length, 1024).Complete();

                for (int i = 0; i < _enteties.Length; i++)
                {
                    var entityData = _enteties[i];

                    if (entityData.isDestroyed)
                    {
                        _enteties[i] = CreateOrReinitLivingEntityData(entityConfig);
                    }
                }

                //if (useSort())
                //{
                //    _enteties.Sort(_sEntetiesComparer);
                //}
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

                    Gizmos.DrawLine(data.Position, (Vector3)data.position + data.Rotation * Vector3.up * boundRadius);
                }
            }

            public void Reset()
            {
                //if (_enteties.IsCreated)
                //{
                //    _enteties.Dispose();
                //    _enteties = default;
                //}
            }

            class LivingEntityDataComparer : IComparer<LivingEntityData>
            {
                public int Compare(LivingEntityData x, LivingEntityData y)
                {
                    return x.layer.CompareTo(y.layer);
                }
            }
        }

        public struct LivingEntityData
        {
            public Vector2 position;

            public float rotation;

            public Vector2 scale;

            public float scaleFactor;

            public float speed;

            public Color baseColor;

            public float alpha;

            public float alphaFactor;

            public int layer;

            public float radius;

            public bool isDestroyed;

            public Vector3 Position
            {
                get
                {
                    return ((Vector3)position).WithZ(-layer);
                }
            }

            public Vector3 Scale
            {
                get
                {
                    return new Vector3(scale.x, scale.y, 1.0f) * scaleFactor;
                }
            }

            public Quaternion Rotation
            {
                get
                {
                    return Quaternion.AngleAxis(rotation, Vector3.forward);
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
