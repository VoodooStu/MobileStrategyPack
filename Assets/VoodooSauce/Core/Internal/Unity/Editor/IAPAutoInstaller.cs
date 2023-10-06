using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core.Editor
{
    /// <summary>
    /// This class detects if Unity Purchasing is installed or not and adapt the project:
    /// <list type="bullet">
    /// <item>
    /// <description>If Unity Purchasing is not present in the project, remove the IAP module</description>
    /// </item>
    /// <item>
    /// <description>If the Unity Purchasing version is inferior to the minimal version supported, update it</description>
    /// </item>
    /// </list>
    /// </summary>
    public class IAPAutoInstaller : AssetPostprocessor
    {
        private const string TAG = "IAPAutoInstaller";
        private const string VOODOO_SAUCE_HEADER = "VoodooSauce | ";
        private const string IAP_MODULE_ASSET_PATH = "Assets/VoodooSauce/IAP";
        private const string UNITY_PACKAGE_EXTENSION = ".unitypackage";
        private const string DEPENDENCIES_KEY = "dependencies";
        private const string UNITY_PURCHASING_KEY = "com.unity.purchasing";
        private const string UNITY_PURCHASING_MINIMAL_VERSION = "4.8.0";
        
        private static string VoodooSaucePath => Application.dataPath + "/VoodooSauce";
        private static string IAPModulePath => VoodooSaucePath + "/IAP";
        private static string IAPPackagePath => VoodooSaucePath + "/Core/Internal/Unity/Package/VoodooSauceIAP_" + UNITY_PURCHASING_MINIMAL_VERSION;
        private static string DefaultGooglePlayTangleV3PackagePath => VoodooSaucePath + "/Core/Internal/Unity/Package/DefaultGooglePlayTangleV3";
        private static string UnityPurchasingScriptsPath =>
            (Application.dataPath + "/Scripts/UnityPurchasing").Replace('/', Path.DirectorySeparatorChar);
        private static string UnityPurchasingPluginPath => Application.dataPath + "/Plugins/UnityPurchasing";
        private static string UdpPluginPath => Application.dataPath + "/Plugins/UDP";
        private static string ManifestFile => Directory.GetCurrentDirectory() + "/Packages/manifest.json";
        private static bool CheckVoodooSauceLocation() => Directory.Exists(VoodooSaucePath.Replace('/', Path.DirectorySeparatorChar));

        

        [InitializeOnLoadMethod]
        private static void CheckAndInstall()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets,
                                                   string[] deletedAssets,
                                                   string[] movedAssets,
                                                   string[] movedFromAssetPaths)
        {
            CheckAndInstall();
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            
            Dictionary<object, object> manifest = JsonConvert.DeserializeObject<Dictionary<object, object>>(File.ReadAllText(ManifestFile));
            Dictionary<object, string> dependencies = JsonConvert.DeserializeObject<Dictionary<object, string>>(manifest[DEPENDENCIES_KEY].ToString());
            
            if (dependencies.ContainsKey(UNITY_PURCHASING_KEY)) {
                string unityPurchasingCurrentVersion = dependencies[UNITY_PURCHASING_KEY];
                if (unityPurchasingCurrentVersion.CompareVersionTo(UNITY_PURCHASING_MINIMAL_VERSION) < 0) {
                    dependencies[UNITY_PURCHASING_KEY] = UNITY_PURCHASING_MINIMAL_VERSION;
                    manifest[DEPENDENCIES_KEY] = dependencies;
                    File.WriteAllText(ManifestFile, JsonConvert.SerializeObject(manifest, Formatting.Indented));
                }
                InstallVoodooSauceIAP();
                RemoveInAppPurchasingAndUdpPlugin();
                AddDefaultGooglePlayTangle();
            } else if (RemoveIAP()) {
                AssetDatabase.Refresh();
            }
        }

        private static void RemoveFileAndItsMeta(string path)
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
        }

        /// <summary>
        /// Update the IAP unity package. Only works on VoodooSauce test app project.
        /// </summary>
        private static void UpdateIAPUnityPackage()
        {
            if (Application.identifier == VoodooConstants.TEST_APP_BUNDLE) {
                AssetDatabase.ExportPackage(IAP_MODULE_ASSET_PATH,
                    IAPPackagePath,
                    ExportPackageOptions.Recurse);
            }
        }

        /// <summary>
        /// Remove the whole IAP module.
        /// </summary>
        /// <returns>True if something has been deleted.</returns>
        private static bool RemoveIAP()
        {
            var hasDeleted = false;
            if (Directory.Exists(IAPModulePath.Replace('/', Path.DirectorySeparatorChar))) {
                UpdateIAPUnityPackage();
                RemoveFileAndItsMeta(IAPModulePath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | IAP module removed.");
                hasDeleted = true;
            }

            hasDeleted |= RemoveInAppPurchasingAndUdpPlugin();
            hasDeleted |= RemoveDefaultGooglePlayTangle();

            return hasDeleted;
        }

        /// <summary>
        /// Remove the UnityPurchasing and UDP plugins.
        /// </summary>
        /// <returns>True if something has been deleted.</returns>
        private static bool RemoveInAppPurchasingAndUdpPlugin()
        {
            var hasDeleted = false;
            if (Directory.Exists(UnityPurchasingPluginPath.Replace('/', Path.DirectorySeparatorChar)))
            {
                RemoveFileAndItsMeta(UnityPurchasingPluginPath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | Unity purchasing plugin removed.");
                hasDeleted = true;
            }

            if (Directory.Exists(UdpPluginPath.Replace('/', Path.DirectorySeparatorChar))) {
                RemoveFileAndItsMeta(UdpPluginPath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | UDP plugin removed.");
                hasDeleted = true;
            }

            return hasDeleted;
        }

        private static void InstallVoodooSauceIAP()
        {
            if (!Directory.Exists(IAPModulePath.Replace('/', Path.DirectorySeparatorChar)) && CheckVoodooSauceLocation()) {
                ImportPackage(IAPPackagePath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | IAP Module installed.");
            }
        }

        private static void ImportPackage(string path)
        {
            string finalPath = path;
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows OS doesn't like files without extensions. We need to add it before importing the package.
                // Plus, we must keep the file without extension in case of the studio is git-ignoring *.unitypackage.
                finalPath = path + UNITY_PACKAGE_EXTENSION;
                try
                {
                    FileUtil.CopyFileOrDirectory(path, finalPath);
                }
                catch (IOException)
                {
                    // The file already exists.
                }
            }
            AssetDatabase.ImportPackage(finalPath, false);
        }

        /// <summary>
        /// Remove the GoogleTangle.cs file if it exists outside of the Assets/Plugins/UnityPurchasing directory
        /// </summary>
        /// <returns>True if it has been deleted.</returns>
        private static bool RemoveDefaultGooglePlayTangle()
        {
            if (Directory.Exists(UnityPurchasingScriptsPath)) {
                RemoveFileAndItsMeta(UnityPurchasingScriptsPath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | Default GooglePlayTangle removed.");
                return true;
            }

            return false;
        }

        private static void AddDefaultGooglePlayTangle()
        {
            if (!Directory.Exists(UnityPurchasingScriptsPath)) {
                ImportPackage(DefaultGooglePlayTangleV3PackagePath);
                Debug.Log(VOODOO_SAUCE_HEADER + TAG + " | Default GooglePlayTangle installed.");
            }
        }
    }
}