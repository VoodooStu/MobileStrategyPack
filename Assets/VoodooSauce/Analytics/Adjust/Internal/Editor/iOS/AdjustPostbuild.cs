#if UNITY_IOS || UNITY_TVOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class AdjustPostbuild
    {
        private const string PLIST_FILE_NAME = "Info.plist";
        private const string AD_ATTRIBUTION_REPORT_ENDPOINT = "NSAdvertisingAttributionReportEndpoint";
        private const string AD_ATTRIBUTION_REPORT_ENDPOINT_ADJUST = "https://adjust-skadnetwork.com/";

        private static string ADJUST_SIGNATURE_COMMAND = $"-force_load $(PROJECT_DIR)/Libraries{AdjustHelper.SIGNATURE_FOLDER}/iOS/{AdjustHelper.IOS_FILE_NAME}";

        [PostProcessBuild(100)]
        public static void PostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;
            
            // add signature folder to libraries paths
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            if (!string.IsNullOrEmpty(unityFrameworkTargetGuid)) {
                project.AddBuildProperty(unityFrameworkTargetGuid, "OTHER_LDFLAGS", "-ObjC");
                project.AddBuildProperty(unityFrameworkTargetGuid, "OTHER_LDFLAGS", ADJUST_SIGNATURE_COMMAND);
            }
            else
            {
                string targetGuid = project.GetUnityMainTargetGuid();
                project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", ADJUST_SIGNATURE_COMMAND);
            }
            
            project.WriteToFile(projectPath);
            
            // add Adjust URL to receive winning iOS 15 postbacks 
            string plistPath = $"{buildPath}/{PLIST_FILE_NAME}";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;

            rootDict.SetString(AD_ATTRIBUTION_REPORT_ENDPOINT, AD_ATTRIBUTION_REPORT_ENDPOINT_ADJUST);

            File.WriteAllText(plistPath, plist.WriteToString());

        }

    }
}
#endif