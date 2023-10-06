// ReSharper disable CheckNamespace
using System.IO;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class BuildHistoryDataExtractHelper
    {
        private const string PACKAGE_MANIFEST_PATH = "/Packages/manifest.json";
        public static string GetAndroidTargetApi()
        {
            var settings = Resources.Load<VoodooSettings>(VoodooSettings.NAME);
            return settings.AndroidBuildTargetEnum.ToString();
        }

        public static string GetVsVersion()
        {
            var version = Resources.Load<VoodooVersion>(VoodooVersion.NAME);
            return version.ToString();
        }

        public static string GetApplicationVersion() => Application.version;

        public static bool IsDevelopmentBuild() => EditorUserBuildSettings.development;

        public static bool IsBatchMode() => Application.isBatchMode;

        public static string GetPackageJson()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return File.ReadAllText(projectPath + PACKAGE_MANIFEST_PATH);
        }

        public static string GetBuildNumber()
        {
            var buildNumber = "";
            if(PlatformUtils.UNITY_IOS) 
                buildNumber = PlayerSettings.iOS.buildNumber;
            
            if(PlatformUtils.UNITY_ANDROID) 
                buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();

            return buildNumber;
        }
        
    }
}