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

        public int numberOfEntities = 10;

        public float entetiesReferenceSpeed = 5f;

        public float entetiesReferenceScale = 0.5f;

        [Range(0, 1f)]
        public float entetiesReferenceAlpha = 1f;

        public bool useGizmos = true;

        #endregion

        IDrawer _drawer;

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

        protected interface IDrawer
        {
            void DrawParametersFrame(int numberOfStats);

            void DrawInfo(string name);

            public void DrawAndGetParameter(int index, string text, ref float value);

            public void DrawAndGetParameter(int index, string text, ref int value);
        }

        class Drawer : IDrawer
        {
            GUIStyle _labelStyle;

            GUIStyle _frameStyle;

            GUIStyle _textStyle;

            Texture2D _frameTexture;

            Font _statFont;

            Font _nameFont;

            string[] _values = new string[32];

            float _offset;

            public Drawer()
            {
                _labelStyle = new GUIStyle(GUI.skin.label);

                _statFont = _labelStyle.font;
                _nameFont = LoadFont(FontCategory.SansSerif, FontStyle.Italic);

                _frameTexture = new Texture2D(1, 1);
                _frameTexture.wrapMode = TextureWrapMode.Repeat;
                _frameTexture.SetPixel(0, 0, Color.black.WithAlpha(0.2f));
                _frameTexture.Apply();

                _frameStyle = new GUIStyle();
                _frameStyle.normal.background = _frameTexture;

                _textStyle = new GUIStyle(GUI.skin.textField);

                _offset = 4 * UICoeff;
            }

            Rect StatRect
            {
                get
                {
                    var windowHeight = 200 * UICoeff;
                    var windowWidth = 800 * UICoeff;

                    return new Rect(Screen.width - windowWidth - UIMediumMargin, UIMediumMargin, windowWidth, windowHeight);
                }
            }

            public void DrawParametersFrame(int numberOfStats)
            {
                var statRect = StatRect;
                var tRect = new Rect(statRect);

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                var labelSize = _labelStyle.CalcSize(new GUIContent(" "));

                tRect.x -= UISmallMargin;
                tRect.y -= UISmallMargin;
                tRect.height = labelSize.y * numberOfStats + UISmallMargin * 2 * numberOfStats;
                tRect.width += UISmallMargin * 2;

                GUI.Box(tRect, _frameTexture, _frameStyle);
            }

            public void DrawAndGetParameter(int index, string text, ref float value)
            {
                DrawAndGetParameter(index, text, ref value, (str, def) => {
                    if (float.TryParse(str, out float result))
                    {
                        return result;
                    }
                    else
                    {
                        return def;
                    }
                });
            }

            public void DrawAndGetParameter(int index, string text, ref int value)
            {
                DrawAndGetParameter(index, text, ref value, (str, def) => {
                    if (int.TryParse(str, out int result))
                    {
                        return result;
                    }
                    else
                    {
                        return def;
                    }
                });
            }

            void DrawAndGetParameter<T>(int index, string text, ref T value, System.Func<string, T, T> tryParse)
            {
                var obj = _values[index];
                if (obj == null)
                {
                    obj = value.ToString();
                }

                _values[index] = DrawStatLabelWithTextField(index, text, obj);

                value = tryParse(_values[index], value);
            }

            string DrawStatLabelWithTextField(int index, string label, string textFieldLabel)
            {
                var statRect = StatRect;

                /// Draw a label

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.UpperLeft;
                _labelStyle.font = _statFont;

                var labelSize = _labelStyle.CalcSize(new GUIContent(label));
                var topOffset = labelSize.y * index + UISmallMargin * 2 * index;

                // Draw shadow

                var tRect = new Rect(statRect);
                tRect.center += Vector2.one * _offset + Vector2.up * topOffset;

                _labelStyle.normal.textColor = Color.black;

                GUI.Label(tRect, label, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                tRect = new Rect(statRect);
                tRect.center += Vector2.up * topOffset;

                GUI.Label(tRect, label, _labelStyle);

                /// Draw a text field
                ///

                var textFieldWidth = 300 * UICoeff;
                var textFieldHeight = labelSize.y;

                var textFieldRect = new Rect(
                    Screen.width - textFieldWidth - UIMediumMargin, UIMediumMargin + topOffset,
                    textFieldWidth, textFieldHeight
                    );

                _textStyle.fontSize = (int)(60 * UICoeff);
                _textStyle.alignment = TextAnchor.MiddleLeft;
                _textStyle.font = _statFont;
                _textStyle.padding.left = _textStyle.padding.right = _textStyle.padding.top = _textStyle.padding.bottom = (int)UISmallPadding;

                return GUI.TextField(textFieldRect, textFieldLabel, 10, _textStyle);
            }

            public void DrawInfo(string name)
            {
                var marginVertical = UIMediumMargin * 2;
                var marginHorizontal = UIMediumMargin;
                var tRect = new Rect(marginHorizontal, Screen.height - (marginHorizontal + marginVertical), 2000 * UICoeff, marginVertical);

                _labelStyle.fontSize = (int)(60 * UICoeff);
                _labelStyle.alignment = TextAnchor.LowerLeft;
                _labelStyle.font = _nameFont;

                // Draw frame

                var size = _labelStyle.CalcSize(new GUIContent(name));
                var margin = 32 * UICoeff;
                var frameRect = new Rect(tRect.x - margin, tRect.y + (marginVertical - size.y) - margin, size.x + margin * 2, size.y + margin * 2);

                GUI.Box(frameRect, _frameTexture, _frameStyle);

                // Draw shadow

                _labelStyle.normal.textColor = Color.black;

                var tRectShadow = new Rect(tRect);
                tRectShadow.center += Vector2.one * _offset;

                GUI.Label(tRectShadow, name, _labelStyle);

                // Draw text

                _labelStyle.normal.textColor = Color.white;

                GUI.Label(tRect, name, _labelStyle);
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
            drawer.DrawParametersFrame(4);

            drawer.DrawAndGetParameter(0, "Entities", ref numberOfEntities);
            drawer.DrawAndGetParameter(1, "Global Scale", ref entetiesReferenceScale);
            drawer.DrawAndGetParameter(2, "Global Alpha", ref entetiesReferenceAlpha);
            drawer.DrawAndGetParameter(3, "Global Speed", ref entetiesReferenceSpeed);
        }
    }

}
