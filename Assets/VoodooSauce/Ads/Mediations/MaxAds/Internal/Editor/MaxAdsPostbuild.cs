using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppLovinMax.Scripts.IntegrationManager.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    public class MaxAdsPostbuild
    {
        private static string PluginMediationDirectory
        {
            get
            {
                var pluginParentDir = AppLovinIntegrationManager.MediationSpecificPluginParentDirectory;
                return Path.Combine(pluginParentDir, "MaxSdk/Mediation/");
            }
        }
        
        private static readonly List<string> EmbedSwiftStandardLibrariesNetworks = new List<string>
        {
            "Facebook",
            "MoPub"
        };
        
        [PostProcessBuild(int.MaxValue)]
        public static void MaxAdsPostProcess(BuildTarget buildTarget, string buildPath)
        {
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var unityMainTargetGuid = project.GetUnityMainTargetGuid();
            AddSwiftStandardLibrariesOnRunpathIfNeeded(buildPath, project, unityMainTargetGuid);
            project.WriteToFile(projectPath);
        }
        
        
        /// <summary>
        /// Add "/usr/lib/swift" string in the top of runpath if the project are using swift library
        /// </summary>
        private static void AddSwiftStandardLibrariesOnRunpathIfNeeded(string buildPath, PBXProject project, string mainTargetGuid)
        {
            var maxMediationDirectory = PluginMediationDirectory;
            var hasEmbedSwiftStandardLibrariesNetworksInProject = EmbedSwiftStandardLibrariesNetworks.Any(network => Directory.Exists(Path.Combine(maxMediationDirectory, network)));
            if (!hasEmbedSwiftStandardLibrariesNetworksInProject) return;
            
            //Unity 2019.3 and above have 2 TargetGuid where we need to add /usr/lib/swift
            string unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            string runPathSearchPaths = project.GetBuildPropertyForAnyConfig(unityFrameworkTargetGuid, 
            "LD_RUNPATH_SEARCH_PATHS");
            if (!runPathSearchPaths.Contains("/usr/lib/swift"))
            {
                //We need to make sure "/usr/lib/swift" is on top of everything
                runPathSearchPaths = runPathSearchPaths.Insert(0, string.IsNullOrEmpty(runPathSearchPaths) ? "" : " ");
                runPathSearchPaths = runPathSearchPaths.Insert(0, "/usr/lib/swift");
                project.SetBuildProperty(unityFrameworkTargetGuid, "LD_RUNPATH_SEARCH_PATHS", runPathSearchPaths);
            }
        }
    }
}
#endif