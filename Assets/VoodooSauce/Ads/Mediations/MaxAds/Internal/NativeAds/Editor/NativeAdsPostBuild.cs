#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.Editor
{
    
    public class NativeAdsPostBuild
    {
        private const string NATIVE_RECTANGLE_LAYOUT_PROJECT_PATH = "Libraries/VoodooSauce/Ads/Mediations/MaxAds/Internal/NativeAds/Plugins/iOS/NativeAdRectangleView.xib";
        private const string NATIVE_SQUARE_LAYOUT_PROJECT_PATH =
            "Libraries/VoodooSauce/Ads/Mediations/MaxAds/Internal/NativeAds/Plugins/iOS/NativeAdSquareView.xib";
        
        [PostProcessBuild(100)]
        public static void PostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetGuid = project.GetUnityMainTargetGuid();
            string rectLayoutGuid = project.FindFileGuidByProjectPath(NATIVE_RECTANGLE_LAYOUT_PROJECT_PATH);
            project.AddFileToBuild(targetGuid, rectLayoutGuid);
            string squareLayoutGuid = project.FindFileGuidByProjectPath(NATIVE_SQUARE_LAYOUT_PROJECT_PATH);
            project.AddFileToBuild(targetGuid, squareLayoutGuid);
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
}
#endif