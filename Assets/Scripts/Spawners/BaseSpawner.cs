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
        IDrawer _drawer;

        public bool IsSelected { get; private set; } = false;

        protected ISpawnerContext Context { get; private set; }

        public void SetSelected(bool selected)
        {
            if (!Application.isPlaying)
                return;

            IsSelected = selected;

            if (selected)
            {
                OnSpawnerSelected();
            } else
            {
                OnSpawnerDeselected();
            }
        }

        protected virtual void OnEnable()
        {
            Context = GameScene.Instance;
            Context.OnLivingEntityDataChanged += OnLivingEntityDataChanged;
        }

        private void OnDisable()
        {
            if (Context == null)
                return;

            Context.OnLivingEntityDataChanged -= OnLivingEntityDataChanged;
            Context = null;    
        }

        public abstract void OnSpawnerSelected();

        public abstract void OnSpawnerDeselected();

        protected virtual void OnLivingEntityDataChanged()
        {

        }

        protected interface IDrawer
        {
            void DrawStat(int index, string text);

            void DrawStatFrame(int numberOfStats);

            void DrawName(string name);

            void DrawGetNumber(string label, ref int number);
        }

        class Drawer : IDrawer
        {
            GUIStyle _labelStyle;

            GUIStyle _frameStyle;

            GUIStyle _textStyle;

            Rect _statRect;

            float _offset;

            Texture2D _texture;

            Font _statFont;

            Font _nameFont;

            public Drawer()
            {
                _labelStyle = new GUIStyle(GUI.skin.label);

                _statFont = _labelStyle.font;
                _nameFont = Resources.Load<Font>("CommonUnityLibrary/NormalFont/Roboto-BoldItalic");

                _texture = new Texture2D(1, 1);
                _texture.wrapMode = TextureWrapMode.Repeat;
                _texture.SetPixel(0, 0, Color.black.WithAlpha(0.2f));
                _texture.Apply();

                _frameStyle = new GUIStyle();
                _frameStyle.normal.background = _texture;

                _textStyle = new GUIStyle(GUI.skin.textField);

                var windowHeight = 200 * UICoeff;
                var windowWidth = 800 * UICoeff;
                var margin = 100 * UICoeff;

                _statRect = new Rect(Screen.width - windowWidth - margin, margin, windowWidth, windowHeight);
                _offset = 4 * UICoeff;
            }

            public void DrawStatFrame(int numberOfStats)
            {
                var tRect = new Rect(_statRect);
                var margin = 32 * UICoeff;

                tRect.x -= margin;
                tRect.y -= margin;
                tRect.height = _labelStyle.fontSize * numberOfStats + 50 * UICoeff * numberOfStats + margin;
                tRect.width += margin * 2;

                GUI.Box(tRect, _texture, _frameStyle);
            }

            public void DrawStat(int index, string text)
            {
                var topOffset = _labelStyle.fontSize * index + 50 * UICoeff * index;

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                // Draw shadow

                var tRect = new Rect(_statRect);
                tRect.center += Vector2.one * _offset + Vector2.up * topOffset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, text, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(_statRect);
                tRect.center += Vector2.up * topOffset;

                GUI.Label(tRect, text, _labelStyle);
            }

            public void DrawName(string name)
            {
                var marginVertical = 200 * UICoeff;
                var marginHorizontal = 100 * UICoeff;
                var tRect = new Rect(marginHorizontal, Screen.height - (marginHorizontal + marginVertical), 2000 * UICoeff, marginVertical);

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.LowerLeft;
                _labelStyle.font = _nameFont;

                // Draw frame

                var size = _labelStyle.CalcSize(new GUIContent(name));
                var margin = 32 * UICoeff;
                var frameRect = new Rect(tRect.x - margin, tRect.y + (marginVertical - size.y) - margin, size.x + margin * 2, size.y + margin * 2);

                GUI.Box(frameRect, _texture, _frameStyle);

                // Draw shadow

                _labelStyle.normal.textColor = Color.black;

                var tRectShadow = new Rect(tRect);
                tRectShadow.center += Vector2.one * _offset;

                GUI.Label(tRectShadow, name, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                GUI.Label(tRect, name, _labelStyle);
            }

            string _numberText;

            public void DrawGetNumber(string label, ref int number)
            {
                var topOffset = 800 * UICoeff;
                var margin = 32 * UICoeff;
                var padding = 16 * UICoeff;
                var labelSize = _labelStyle.CalcSize(new GUIContent(label));

                /// Draw Frame

                var tRect = new Rect(_statRect);

                tRect.center += Vector2.up * topOffset - Vector2.one * margin;
                tRect.size = new Vector2(
                    margin * 2 + _statRect.width,
                    margin * 3 + padding * 2 + labelSize.y * 2
                    );

                GUI.Box(tRect, _texture, _frameStyle);

                /// Label

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                // Draw shadow

                tRect = new Rect(_statRect);

                tRect.y += topOffset;
                tRect.center += Vector2.one * _offset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, label, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(_statRect);
                tRect.y += topOffset;

                GUI.Label(tRect, label, _labelStyle);

                /// Draw textField

                tRect = new Rect(_statRect);

                tRect.y += topOffset + labelSize.y + margin;
                tRect.height = labelSize.y + padding * 2;

                if (_numberText == null) {
                    _numberText = number.ToString();
                }

                _textStyle.fontSize = (int)(60 * UICoeff);
                _textStyle.alignment = TextAnchor.MiddleLeft;
                _textStyle.font = _statFont;
                _textStyle.padding.left = _textStyle.padding.right = _textStyle.padding.top = _textStyle.padding.bottom = (int) padding;

                var text = GUI.TextField(tRect, _numberText, 10, _textStyle);

                if (int.TryParse(text, out int result))
                {
                    number = result;
                    _numberText = text;
                }
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

        protected virtual void PerformOnGUI(IDrawer drawer)
        {
        }
    }

}
