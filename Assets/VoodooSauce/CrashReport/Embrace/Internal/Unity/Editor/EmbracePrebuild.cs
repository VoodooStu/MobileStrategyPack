using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public class EmbracePrebuild : IPreprocessBuildWithReport
    {
#region Constants

        private const string EMBRACE_ASSET_FILES_DIRECTORY = "Assets/VoodooSauce/CrashReport/Embrace/Assets";
        
        // If the VS Settings are not up-to-date and don’t have the Embrace settings (i.e. the values are null),
        // then at the build time, when the Embrace script checks the appId and/or the API token, the build fails because these values are not valid.
        // So, a default value for the Embrace appId and the Embrace API token must be set up at this prebuild step.
        private const string EMBRACE_DEFAULT_APP_ID = "00000";
        private const string EMBRACE_DEFAULT_API_TOKEN = "00000000000000000000000000000000";

#endregion
        
        public int callbackOrder => int.MaxValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            try {
                // This Embrace configuration file should be created by the Embrace SDK at launch.
                // So, if this file does not exist of if the destination folder to save the Embrace asset files is not the right one
                // it is created and/or updated now.
                EmbracePrebuildBridge.SetDataDirectory(EMBRACE_ASSET_FILES_DIRECTORY);

                string embraceAssetFilesPath = Path.GetFullPath(EMBRACE_ASSET_FILES_DIRECTORY);
                if (!Directory.Exists(embraceAssetFilesPath)) {
                    Directory.CreateDirectory(embraceAssetFilesPath);
                    Debug.Log($"[VoodooSauce] directory '{embraceAssetFilesPath}' created");
                }

                // Now the appId and apiToken from Embrace can be saved into the .asset files.
                VoodooSettings settings = VoodooSettings.Load();
                EmbracePrebuildBridge.SetKeys(
                    GetValidEmbraceAppId(settings.embraceAndroidAppId), GetValidEmbraceApiToken(settings.embraceAndroidApiToken),
                    GetValidEmbraceAppId(settings.embraceIosAppId), GetValidEmbraceApiToken(settings.embraceIosApiToken));
            } catch (Exception e) {
                Debug.LogError($"[VoodooSauce] Can not generate the Embrace configuration files: {e.Message}");
            }
        }

        private static string GetValidEmbraceAppId(string appId) => string.IsNullOrEmpty(appId) ? EMBRACE_DEFAULT_APP_ID : appId;
        
        private static string GetValidEmbraceApiToken(string apiToken) => string.IsNullOrEmpty(apiToken) ? EMBRACE_DEFAULT_API_TOKEN : apiToken;
    }
}