#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Voodoo.Sauce.Common.Utils.Data.ScreenUtilsConfig.Editor
{

    public class VoodooScreenUtilsAndroidPrebuild: IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;
        public void OnPreprocessBuild(BuildReport report)
        {
            VoodooScreenUtilsConfig config = VoodooScreenUtilsConfig.Load();
            config.IsAndroidRenderOutsideSafeArea = PlayerSettings.Android.renderOutsideSafeArea;
            config.Save();
        }
    }
}

#endif