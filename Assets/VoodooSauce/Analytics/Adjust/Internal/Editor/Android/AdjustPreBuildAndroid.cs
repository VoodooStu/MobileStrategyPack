#if UNITY_ANDROID

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Internal.Utils;

public class AdjustPreBuildAndroid : IPreprocessBuildWithReport
{
    public int callbackOrder => 99;

    private const string ADJUST_MANIFEST_FILE_PATH =
        "Assets/VoodooSauce/Analytics/Adjust/Internal/Editor/Android/AndroidManifest.xml";

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) {
            return;
        }
        
        ManifestUtils.Add(ADJUST_MANIFEST_FILE_PATH);
    }
}

#endif