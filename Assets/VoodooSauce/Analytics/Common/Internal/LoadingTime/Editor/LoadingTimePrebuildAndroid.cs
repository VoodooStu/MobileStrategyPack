#if UNITY_ANDROID
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.LoadingTime.Editor
{
    public class LoadingTimePrebuildAndroid : IPreprocessBuildWithReport
    {
        private const string ManifestPath = "Assets/VoodooSauce/Analytics/Common/Internal/LoadingTime/Editor/LoadingTimeAndroidManifest.xml";

        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            //Add  manifest declarations to the application manifest
            ManifestUtils.Add(ManifestPath);
        }
    }
}

#endif