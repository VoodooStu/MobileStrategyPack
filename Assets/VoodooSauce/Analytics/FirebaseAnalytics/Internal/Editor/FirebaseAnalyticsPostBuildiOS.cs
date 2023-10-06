#if UNITY_IOS || UNITY_TVOS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation.Applovin.Editor
{
    public static class FirebaseAnalyticsPostBuildiOS
    {
        private const string PODFILE_TARGET_START_WITH = "target ";
        
        //We need to add this after the podfile generation (BUILD_ORDER_GEN_PODFILE = 40) and before executing pod install (BUILD_ORDER_INSTALL_PODS = 50)
        private const int BUILD_ORDER_USE_MODULAR_HEADER = 41;
        private const string PODFILE_USE_MODULAR_HEADER = "use_modular_headers!";
       

        [PostProcessBuildAttribute(BUILD_ORDER_USE_MODULAR_HEADER)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            //Disable Firebase Analytics by default so we are sure no event is sent before the user consented
            var firebaseAnalyticsKey = new Dictionary<string, object> {
                {"FirebaseCrashlyticsCollectionEnabled", false}
            };
            InfoPlistUtils.UpdateRootDict(buildPath, firebaseAnalyticsKey);
            //Add modular header to the podfile
            AddModularHeaderPodfile(buildPath);
        }

        private static void AddModularHeaderPodfile(string buildPath)
        {
            string podfilePath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfilePath)) {
                Debug.LogError("FirebaseAnalyticsPostBuildiOS: Podfile not exist");
                return;
            }

            List<string> lines = File.ReadAllLines(podfilePath).ToList();
            int index = lines.FindIndex(StartWith);
            if(index == -1) return;
            lines.Insert(index,PODFILE_USE_MODULAR_HEADER);
            
            try {
                File.WriteAllLines(podfilePath, lines);
            } catch (Exception exception) {
                Debug.LogError(exception.Message);
            }
        }

        private static bool StartWith(string str) => str.TrimStart().StartsWith(PODFILE_TARGET_START_WITH);
    }
}

#endif