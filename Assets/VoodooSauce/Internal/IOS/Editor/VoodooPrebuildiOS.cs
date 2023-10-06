#if UNITY_IOS

using Google;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Globalization;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    // ReSharper disable once IdentifierTypo
    public class VoodooPrebuildiOS : IPreprocessBuildWithReport
    {
        // Since Xcode 14 the minimum version is 11.0: "Building for deployment to OS releases older than macOS 10.13, iOS 11, tvOS 11, and watchOS 4 is no longer supported."
        // Release notes: https://developer.apple.com/documentation/xcode-release-notes/xcode-14-release-notes
        private const float MIN_IOS_VERSION = 13.0f;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PrepareResolver();
            PreparePlayerSettings();
        }

        private static void PrepareResolver()
        {
            // force Play Services Resolver to generate Xcode project and not workspace
            IOSResolver.PodfileGenerationEnabled = true;
            IOSResolver.PodToolExecutionViaShellEnabled = true;
            IOSResolver.AutoPodToolInstallInEditorEnabled = true;
            IOSResolver.UseProjectSettings = true;
            IOSResolver.PodfileAddUseFrameworks = false;
            IOSResolver.PodfileStaticLinkFrameworks = false;
            IOSResolver.CocoapodsIntegrationMethodPref = IOSResolver.CocoapodsIntegrationMethod.Project;
            Debug.Log("[VoodooSauce] update Play Services Resolver");
        }

        private static void PreparePlayerSettings()
        {
            // enable insecure HTTP downloads (mandatory for few ad networks)
            PlayerSettings.iOS.allowHTTPDownload = true;
            Debug.Log("[VoodooSauce] set allowHTTPDownload=true to the player settings");

            // set iOS compatible min version
            var changeMinVersion = true;
            if (float.TryParse(PlayerSettings.iOS.targetOSVersionString, out float iosMinVersion)) {
                changeMinVersion = iosMinVersion < MIN_IOS_VERSION;
            }

            if (changeMinVersion) {
                PlayerSettings.iOS.targetOSVersionString = MIN_IOS_VERSION.ToString("F1",  CultureInfo.InvariantCulture);
                Debug.Log($"[VoodooSauce] set targetOSVersionString={PlayerSettings.iOS.targetOSVersionString} to the player settings");
            }

            // since Xcode 14 only the ARM64 architecture is available: "Building iOS projects with deployment targets for the armv7, armv7s, and i386 architectures is no longer supported."
            // architecture values: 0 - None, 1 - ARM64, 2 - Universal.
            // Release notes: https://developer.apple.com/documentation/xcode-release-notes/xcode-14-release-notes
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
            Debug.Log("[VoodooSauce] set architecture=arm64 to the player settings");
        }
    }
}

#endif