#if UNITY_IOS || UNITY_TVOS
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation.GoogleAd.Editor
{
    public static class GoogleAdPostbuildiOS
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            var adsKeys = new Dictionary<string, object> {{"GADIsAdManagerApp", true}};
            InfoPlistUtils.UpdateRootDict(buildPath, adsKeys);
        }
    }
}

#endif