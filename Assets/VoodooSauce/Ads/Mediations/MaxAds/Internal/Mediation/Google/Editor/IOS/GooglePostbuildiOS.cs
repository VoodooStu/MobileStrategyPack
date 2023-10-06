#if UNITY_IOS || UNITY_TVOS

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation.Google.Editor
{
    public static class GooglePostbuildiOS
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            var adsKeys = new Dictionary<string, object> {
                {"gad_preferred_webview", "wkwebview"}
            };
            InfoPlistUtils.UpdateRootDict(buildPath, adsKeys);
        }
    }
}

#endif