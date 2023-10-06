using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Editor
{
    [InitializeOnLoad]
    public static class VoodooBuildValidator
    {
        static VoodooBuildValidator()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(HandleBuild);
        }
        
        private static void HandleBuild(BuildPlayerOptions options) 
        {	
            Build(options);	
        }
        
        // Disable warning "can be private" since this might be used by studios for their CI/CD
        // ReSharper disable once MemberCanBePrivate.Global
        public static BuildReport Build(BuildPlayerOptions options)
        {
            RefreshVoodooSettings(options.target);

            FirebaseBuildValidationCheck(options); 
            
            var checkResult = VoodooSauceSettingsEditor.IntegrationCheck();

            if (!EditorUserBuildSettings.development &&
                checkResult == VoodooSauceSettingsEditor.IntegrationCheckResult.ERROR) {

                Debug.LogError(
                    "Error in Integration Check and Development Mode is disabled.  "
                    + "You should not make a release build of your game until you resolve the Integration Check error(s)");
                return null; 
            }
            
            // if (PlatformUtils.UNITY_ANDROID && !Application.isBatchMode
            //                                 && VoodooAndroidBuildSettingsManager.GetAndroidBuildTarget()
            //                                 == VoodooSauceAndroidBuildTargetEnum.BuildTargetApi31) {
            //     if (!UnityEditor.EditorUtility.DisplayDialog("You are building with Build Target API 31",
            //             "Google play will start rejecting new apps and apps update with"
            //             + " Build Target 31 on 31st August 2023."
            //             + " Are you sure you want to proceed?",
            //             "Yes", "Cancel")) return null;
            // }
            
            if (!Application.isBatchMode && EditorUserBuildSettings.development) {
                if (!UnityEditor.EditorUtility.DisplayDialog("You are in a development build",
                    "You should disable the development build option if you want to make a release.",
                    "This is expected", "Cancel")) return null;
            }

            if (!Application.isBatchMode && VoodooSettings.Load().EnableSuperPremiumMode) {
                if (!UnityEditor.EditorUtility.DisplayDialog("You enabled Super Premium Mode",
                        "You should disable the Super Premium Mode option if you want to make a release since Super Premium Mode will cause no Ads to be displayed at all. \n\nPlease disable Super Premium Mode if you want to release to Playstore / Appstore",
                        "This is expected", "Cancel Build")) return null;
            }

            if (!RunPreprocessors()) {	
                Debug.LogError(
                    "Error Build Preprocessor: "
                    + "One of your build preprocessor (class that inherit IVoodooBuildPreprocessor) is failing. Please check the debug log above to investigate.");
                return null;	
            }
            
            return BuildPipeline.BuildPlayer(options);
        }

        private static bool RunPreprocessors() 
        {	
            var classes = AppDomain.CurrentDomain	
                                   .GetAssemblies()	
                                   .SelectMany(x => x.GetTypes())	
                                   .Where(x => typeof(IVoodooBuildPreprocessor).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)	
                                   .ToList();	
            foreach (var type in classes) {	
                Debug.Log($"Running {type}...");	
                	
                var instance = Activator.CreateInstance(type);	
                var methodInfo = type.GetMethod(nameof(IVoodooBuildPreprocessor.Build));	
                var result = methodInfo != null && (bool) methodInfo.Invoke(instance, null);	
                if (!result) return false;	
            }	
            return true;	
        }
        
        private static void FirebaseBuildValidationCheck(BuildPlayerOptions options)
        {
#if UNITY_IOS
            if (!Application.isBatchMode && options.locationPathName.Contains(" ")) {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Invalid iOS build folder path",
                    "The iOS build folder path contains \"space\" characters. \nPlease remove them and rebuild.",
                    null, null);
            }
#endif
        }
        
        private static void RefreshVoodooSettings(BuildTarget buildTarget)
        {
            /*
             * If the scripting symbol VS_DONT_DOWNLOAD_SETTINGS is provided the VS settings won't be refreshed at the build time.
             * The default behaviour is based on the previous behaviour that is to refresh the settings at every build step.
             */
#if VS_DONT_DOWNLOAD_SETTINGS
            Debug.Log("VoodooSauce Settings are not refreshed");
            CheckVoodooSettings(buildTarget);
#else
            Debug.Log("Refresh VoodooSauce Settings");

            EditorUtility.StartSyncBackgroundTask(
                VoodooSauceSettingsManager.RefreshVoodooSauceSettings(false), () => {
                    Debug.Log("Refresh VoodooSauce Settings DONE");
                    CheckVoodooSettings(buildTarget);
                });
#endif
        }
        private static void CheckVoodooSettings(BuildTarget buildTarget)
        {
            string store = VoodooSettings.Load().Store;
            
            if (!VoodooSauceSettingsHelper.IsCurrentStoreSupported(buildTarget)) {
                Debug.LogError($"The configuration '{store}/{buildTarget}' is not allowed to be built.");
                
                UnityEditor.EditorUtility.DisplayDialog("This configuration is not allowed to be built",
                    $"The VoodooSauce settings are not configured for the store '{store}' and the platform '{buildTarget}'.",
                    "Close");
                
                throw new BuildFailedException($"The configuration '{store}/{buildTarget}' is not allowed to be built, the VoodooSauce settings are not configured for the store '{store}' and the platform '{buildTarget}'.");
            }
            
            Debug.Log($"The configuration '{store}/{buildTarget}' is allowed to be built."); 
        }
    }
}