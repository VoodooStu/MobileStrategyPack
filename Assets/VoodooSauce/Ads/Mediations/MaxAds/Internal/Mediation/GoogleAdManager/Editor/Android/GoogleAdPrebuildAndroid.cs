#if UNITY_ANDROID
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation.GoogleAd.Editor
{
    public class GoogleAdPrebuildAndroid : IPreprocessBuildWithReport
    {
        private const string GOOGLE_AD_MANIFEST_PATH =
            AdsConstants.MAX_MEDIATION_FOLDER_PATH + "/Internal/Mediation/GoogleAdManager/Editor/Android/GoogleAdManifest.xml";

        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            //Add Mediation manifest declarations to the application manifest
            ManifestUtils.Add(GOOGLE_AD_MANIFEST_PATH);
            var adsKeys = new Dictionary<string, string> {{"[GOOGLE_AD_MANAGER_ENABLED]", "true"}};
            ManifestUtils.Replace(adsKeys);
        }
    }
}

#endif