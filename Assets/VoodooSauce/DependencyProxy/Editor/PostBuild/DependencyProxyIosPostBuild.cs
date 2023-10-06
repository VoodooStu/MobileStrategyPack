using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
// ReSharper disable ConvertToConstant.Local
namespace Voodoo.Sauce.DependencyProxy.Editor.PostBuild
{
    public static class DependencyProxyIosPostBuild
    {
        private const string PODFILE_SOURCE_URL_ORIGINAL = "source 'https://github.com/CocoaPods/Specs.git'";
        private const string PODFILE_SOURCE_URL_PROXY = "source 'https://"+DependencyProxyManager.NEXUS_PROXY_HOST+"/repository/cocoapods-proxy'";
#if UNITY_IOS
        private static readonly bool IS_IOS = true;
#else
        private static readonly bool IS_IOS = false;
#endif
        [PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!IS_IOS) return;
            if (!DependencyProxyManager.IsDependencyProxyEnabled()) return;
            var podfilePath = pathToBuiltProject + "/Podfile";
            if (!File.Exists(podfilePath))
            {
                Debug.LogError(
                    "No Podfile found in the generated project, please check your console log to check the error that might happened during the build.");
                return;
            }
            UpdatePodfileWithProxy(podfilePath);
            CreateNetrcFileIfDoesntExist();
        }

        private static void UpdatePodfileWithProxy(string podfilePath)
        {
            var podfileFileContent = File.ReadAllText(podfilePath);
            podfileFileContent = podfileFileContent.Replace(PODFILE_SOURCE_URL_ORIGINAL, PODFILE_SOURCE_URL_PROXY);
            File.WriteAllText(podfilePath, podfileFileContent);
        }

        private static void CreateNetrcFileIfDoesntExist()
        {
#if !UNITY_EDITOR_OSX //If editor is not macos no need to maintain netrc file
            Debug.Log("No need to create .netrc file since it is not macos editor. And iOS build wont be performed in this machine");
            return;
#endif
            var netrcFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.netrc";
            var netrcFileContent = "";
            if (File.Exists(netrcFilePath))
            {
                //Check if it have nexus credentials configured, if it have then no need to to anything
                netrcFileContent = File.ReadAllText(netrcFilePath);
                if (netrcFileContent.Contains($"machine {DependencyProxyManager.NEXUS_PROXY_HOST}")) return;
            }
            
            //Add config to netrc if it doesnt have or if it doesnt exist
            netrcFileContent += $"machine {DependencyProxyManager.NEXUS_PROXY_HOST} login {DependencyProxyManager.GetNexusRepoUsername()} password {DependencyProxyManager.GetNexusRepoPassword()}\n";
            
            try
            {
                File.WriteAllText(netrcFilePath, netrcFileContent);
            }
            catch (Exception ex)
            { 
                Debug.LogException(ex); 
                Debug.LogError("Creation of Netrc files failed, please create .netrc file manually by going to " + 
                               $"terminal and execute: \n echo 'machine {DependencyProxyManager.NEXUS_PROXY_HOST} login {DependencyProxyManager.GetNexusRepoUsername()} password {DependencyProxyManager.GetNexusRepoPassword()}' >>  ~/.netrc");
            }
        }
    }
}