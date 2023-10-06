#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Voodoo.Sauce.Common.Utils.Data.ScreenUtilsConfig
{
    public class VoodooScreenUtilsConfig: ScriptableObject
    {
        public const string NAME = "VoodooScreenUtilsConfig";
        
        public static VoodooScreenUtilsConfig Load() => Resources.Load<VoodooScreenUtilsConfig>(NAME);

        public bool IsAndroidRenderOutsideSafeArea;
        
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}