using UnityEngine;

using m039.Common;
using static m039.Common.UIUtils;

namespace GP4
{
    public interface ISpawnerContext
    {
        BaseLivingEntityConfig LivingEntityConfig { get; }

        event System.Action OnLivingEntityDataChanged;

        bool GUIVisibility { get; }
    }

    public abstract class BaseSpawner : MonoBehaviour
    {
        #region Inspector

        public bool useGizmos = true;

        #endregion

        public int numberOfEntities => GameScene.Instance.numberOfEntities;

        public float entetiesReferenceSpeed => GameScene.Instance.entetiesReferenceSpeed;

        public float entetiesReferenceScale => GameScene.Instance.entetiesReferenceScale;

        public float entetiesReferenceAlpha => GameScene.Instance.entetiesReferenceAlpha;

        Drawer _drawer;

        protected ISpawnerContext Context { get; private set; }

        protected virtual void OnEnable()
        {
            Context = GameScene.Instance;
            Context.OnLivingEntityDataChanged += OnLivingEntityDataChanged;
        }

        protected virtual void OnDisable()
        {
            if (Context == null)
                return;

            Context.OnLivingEntityDataChanged -= OnLivingEntityDataChanged;
            Context = null;    
        }

        protected virtual void OnLivingEntityDataChanged()
        {
        }

        protected class Drawer
        {
            public void DrawInfo(string name)
            {
                GameScene.Instance.notificationMessage.SetMessage(name);
            }
        }

        void OnGUI()
        {
            if (_drawer == null)
            {
                _drawer = new Drawer();
            }

            if (Context.GUIVisibility)
            {
                PerformOnGUI(_drawer);
            }
        }

        protected virtual void PerformOnGUI(Drawer drawer)
        {
        }
    }

}
