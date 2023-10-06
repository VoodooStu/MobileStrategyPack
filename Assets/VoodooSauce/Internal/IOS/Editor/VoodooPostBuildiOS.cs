#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    // ReSharper disable once IdentifierTypo
    public static class VoodooPostBuildiOS
    {
        private const string SKADNETWORK_ITEMS_PLIST_KEY = "SKAdNetworkItems";
        private const string SKADNETWORK_IDENTIFIER_PLIST_KEY = "SKAdNetworkIdentifier";
        private const string SDK_FOLDER_PATH = "VoodooSauce/";
        private const string SKADNETWORK_IDS_FILE_NAME = "SKAdNetworkIds";
        
        [PostProcessBuild(int.MinValue)]
        public static void UpdateXcodeSettings(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) {
                return;
            }
            
            XcodeUtils.UpdateInfoPlistFile(pathToBuiltProject, dict => {
                dict.SetString("NSCalendarsUsageDescription", "This app does not use the calendar.");
                Debug.Log("[VoodooSauce] set NSCalendarsUsageDescription to the settings");
                
                dict.SetString("NSPhotoLibraryUsageDescription", "This app does not use the photo library.");
                Debug.Log("[VoodooSauce] set NSPhotoLibraryUsageDescription to the settings");
                
                dict.SetString("NSLocationAlwaysUsageDescription", "This app uses location for ad targeting purposes.");
                Debug.Log("[VoodooSauce] set NSLocationAlwaysUsageDescription to the settings");
                
                dict.SetString("NSLocationWhenInUseUsageDescription", "This app uses location for ad targeting purposes.");
                Debug.Log("[VoodooSauce] set NSLocationWhenInUseUsageDescription to the settings");
            });

            XcodeUtils.UpdatePBXProjectFile(pathToBuiltProject, (project, mainTargetGuid, frameworkTargetGuid) => {
                // Needed by Adjust
                project.AddFrameworkToProject(mainTargetGuid, "iAd.framework", true); // iOS 4.0+ (deprecated)
                project.AddFrameworkToProject(mainTargetGuid, "AdServices.framework", true); // iOS 14.3+
                
                // Needed by IronSource
                project.AddFrameworkToProject(mainTargetGuid, "CoreTelephony.framework", false); // iOS 4.0+
                
                // Needed by Adjust, GameAnalytics and IronSource (forced to "Required" here)
                project.AddFrameworkToProject(mainTargetGuid, "AdSupport.framework", false); // iOS 6.0+
                
                Debug.Log("[VoodooSauce] add the dependencies frameworks");

                // Disable the bitcode (requirement since Xcode 14): "Because bitcode is now deprecated, builds for iOS, tvOS, and watchOS no longer include bitcode by default."
                // Release notes: https://developer.apple.com/documentation/xcode-release-notes/xcode-14-release-notes
                // More info: https://stackoverflow.com/questions/72543728/xcode-14-deprecates-bitcode-but-why
                DisableBitcode(project, mainTargetGuid, "main");
                DisableBitcode(project, frameworkTargetGuid, "framework");
            });
        }

        [PostProcessBuild(1)]
        public static void UpdateInfoPlistFile(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) {
                return;
            }

            XcodeUtils.UpdateInfoPlistFile(pathToBuiltProject, dict => {
                // Set encryption usage boolean
                const string encryptKey = "ITSAppUsesNonExemptEncryption";
                dict.SetBoolean(encryptKey, false);
                Debug.Log("[VoodooSauce] set ITSAppUsesNonExemptEncryption=false to the settings");

                // remove exit on suspend if it exists.
                const string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
                if (dict.values.ContainsKey(exitsOnSuspendKey)) {
                    dict.values.Remove(exitsOnSuspendKey);
                    Debug.Log("[VoodooSauce] remove UIApplicationExitsOnSuspend from the settings");
                }
            });
        }

        [PostProcessBuild(100)]
        public static void ConfigureAppTransportSecurity(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) {
                return;
            }

            XcodeUtils.UpdateInfoPlistFile(pathToBuiltProject, dict => {
                if (dict.values.ContainsKey("NSAppTransportSecurity")) {
                    dict.values.Remove("NSAppTransportSecurity");
                }

                var appTransportSecurity = new PlistElementDict();
                appTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);
                dict.values.Add("NSAppTransportSecurity", appTransportSecurity);
                Debug.Log("[VoodooSauce] fix NSAppTransportSecurity from the settings");
            });
        }

        [PostProcessBuild(int.MaxValue)]
        private static void AddSkAdNetworkIds(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) {
                return;
            }

            XcodeUtils.UpdateInfoPlistFile(pathToBuiltProject, dict => {
                dict.values.TryGetValue(SKADNETWORK_ITEMS_PLIST_KEY, out PlistElement skAdNetworkItems);
                var existingSkAdNetworkIds = new HashSet<string>();
            
                // Check if SKAdNetworkItems array is already in the Plist document and collect all the IDs that are already present.
                if (skAdNetworkItems != null && skAdNetworkItems.GetType() == typeof(PlistElementArray))
                {
                    IEnumerable<PlistElement> plistElementDictionaries = skAdNetworkItems.AsArray().values.Where(plistElement => plistElement.GetType() == typeof(PlistElementDict));
                    foreach (PlistElement plistElement in plistElementDictionaries)
                    {
                        plistElement.AsDict().values.TryGetValue(SKADNETWORK_IDENTIFIER_PLIST_KEY, out PlistElement existingId);
                        if (existingId == null || existingId.GetType() != typeof(PlistElementString) || string.IsNullOrEmpty(existingId.AsString())) {
                            continue;
                        }

                        existingSkAdNetworkIds.Add(existingId.AsString());
                    }
                }
                else
                {
                    skAdNetworkItems = dict.CreateArray(SKADNETWORK_ITEMS_PLIST_KEY);
                }
                
                // Get all the files containing the SKAdNetwork Ids to add.
                string rootSearchPath = Path.Combine(Application.dataPath, SDK_FOLDER_PATH);
                string[] files = Directory.GetFiles(rootSearchPath, SKADNETWORK_IDS_FILE_NAME, SearchOption.AllDirectories);

                // Gather all the properties.
                PlistElementArray skAdNetworkItemArray = skAdNetworkItems.AsArray();
                foreach (string file in files) {
                    try {
                        Debug.Log($"[VoodooSauce] Checking SKAdNetworkIds file: '{file}'");
                        
                        IEnumerable<string> skAdNetworkIds = File.ReadLines(file);
                        foreach (string skAdNetworkId in skAdNetworkIds) {
                            if (!XcodeUtils.IsSkAdNetworkId(skAdNetworkId)) {
                                Debug.LogError($"[VoodooSauce] wrong SKAdNetworkId: '{skAdNetworkId}'");
                                continue;
                            }
                            
                            if (existingSkAdNetworkIds.Contains(skAdNetworkId)) {
                                Debug.Log($"[VoodooSauce] SKAdNetworkId '{skAdNetworkId}' already in the list");
                                continue;
                            }
                            
                            PlistElementDict skAdNetworkItemDict = skAdNetworkItemArray.AddDict();
                            skAdNetworkItemDict.SetString(SKADNETWORK_IDENTIFIER_PLIST_KEY, skAdNetworkId);
                            
                            Debug.Log($"[VoodooSauce] SKAdNetworkId '{skAdNetworkId}' added");
                        }
                    } catch (Exception e) {
                        Debug.LogError($"[VoodooSauce] Cannot get the SKAdNetwork ids from the file '{file}': {e.Message}");
                    }
                }
            });
        }
        
        /*
         * Update the Xcode project by disabling the bitcode in a target.
         */
        private static void DisableBitcode(PBXProject project, string targetGuid, string targetType) 
        {
            if (string.IsNullOrEmpty(targetGuid)) {
                Debug.Log($"[VoodooSauce] {targetType} target not found");
                return;
            }
            
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            Debug.Log($"[VoodooSauce] set ENABLE_BITCODE=NO to the {targetType} target's build settings '{targetGuid}'");
        }

    }
}

#endif