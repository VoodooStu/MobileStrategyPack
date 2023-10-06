#if UNITY_IOS || UNITY_TVOS

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation.Applovin.Editor
{
    public static class FirebaseCrashlyticsPostBuildiOS
    {
        //Disable Firebase Crashlytics by default so we are sure no event is sent before the user consented
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            var firebaseCrashlyticsKey = new Dictionary<string, object> {
                {"FIREBASE_ANALYTICS_COLLECTION_ENABLED", false}};
            InfoPlistUtils.UpdateRootDict(buildPath, firebaseCrashlyticsKey);
        }
    }
}

#endif