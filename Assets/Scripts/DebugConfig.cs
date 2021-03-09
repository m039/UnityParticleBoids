using UnityEngine;
using m039.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GP4
{

    [CreateAssetMenu(menuName = Consts.MenuItemRoot + "/DebugConfig")]
    public partial class DebugConfig : SingletonScriptableObject<DebugConfig>
    {

#if UNITY_EDITOR
        public bool Test => true;
#else
        public bool Test => false;
#endif

#if UNITY_EDITOR
        [MenuItem(Consts.MenuItemRoot + "/Debug Config")]
        static void OpenDebugConfig()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(Instance));
        }
#endif

        protected override bool UseResourceFolder => true;

        protected override string PathToResource => "DebugConfig";
    }

}
