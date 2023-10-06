using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class AutoUpdateManager
    {
        private const string TAG = "AutoUpdateManager"; 
        private const string EPrefVersionMetadataJson = "VS_versionMetadata";

        public delegate void OnLoadAutoUpdateDone(Package package);

        public static void LoadAutoUpdate(OnLoadAutoUpdateDone callback)
        {
            AutoUpdateAPI.LoadVoodooVersion(delegate(MetadataResponse response) {
                if (response != null) {
                    EditorPrefs.SetString(EPrefVersionMetadataJson, JsonUtility.ToJson(response));
                }

                VersionMetadata versionMetadata = GetVersionToUpdate();
                HotfixMetadata hotfixMetadata = GetHotfixToUpdate();

                if (!MetadataIsNullOrEmpty(versionMetadata)) {
                    callback?.Invoke(versionMetadata.ToPackage());
                } else if (!MetadataIsNullOrEmpty(hotfixMetadata)) {
                    callback?.Invoke(hotfixMetadata.ToPackage());
                } else {
                    callback?.Invoke(null);
                }
            });
        }

        public static void DownloadAndInstallPackage(Package package)
        {
            AutoUpdatePackageInstaller.DownloadAndInstallPackage(package, delegate { EditorPrefs.SetBool(package.Version, true); });
        }

        private static VersionMetadata GetVersionToUpdate()
        {
            MetadataResponse metadata = GetMetadata();
            if (metadata == null) return null;

            Version currentVersion;
            try { 
                currentVersion = new Version(VoodooSauce.Version());            
            } catch (Exception e) {
                VoodooLog.LogDebug(Module.COMMON, TAG, e.Message);
                return null;
            }
            
            string applicationIdentifier = PlayerSettings.applicationIdentifier;
            return metadata.Versions.FirstOrDefault(version =>
                new Version(version.Version).CompareTo(currentVersion) > 0 && VersionIsApplicable(version, applicationIdentifier));
        }

        private static HotfixMetadata GetHotfixToUpdate()
        {
            MetadataResponse metadata = GetMetadata();
            if (metadata == null) return null;

            string applicationIdentifier = PlayerSettings.applicationIdentifier;
            return metadata.Hotfixes.FirstOrDefault(version =>
                !PackageIsAlreadyInstalled(version.Version) && version.VoodooSauceVersion.Contains(VoodooSauce.Version())
                && VersionIsApplicable(version, applicationIdentifier));
        }

        private static bool VersionIsApplicable(Metadata version, string applicationIdentifier)
        {
            bool isInWhiteList = version.BundleIdsWhiteList != null && version.BundleIdsWhiteList.Contains(applicationIdentifier);
            bool isNotIgnored = (version.BundleIdsWhiteList == null || version.BundleIdsWhiteList.Count == 0)
                && (version.BundleIdsToIgnore == null || !version.BundleIdsToIgnore.Contains(applicationIdentifier));
            return isInWhiteList || isNotIgnored;
        }

        private static bool MetadataIsNullOrEmpty(Metadata metadata) => metadata == null || string.IsNullOrEmpty(metadata.Version);

        private static MetadataResponse GetMetadata()
        {
            string metadataJson = EditorPrefs.GetString(EPrefVersionMetadataJson);
            return JsonUtility.FromJson<MetadataResponse>(metadataJson);
        }

        private static bool PackageIsAlreadyInstalled(string version) => EditorPrefs.GetBool(version, false);

        public static void SavePackageInstalled(string version)
        {
            EditorPrefs.SetBool(version, true);
        }
    }
}