using System.Collections.Generic;
using UnityEditor;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    public class MaxAdsIntegrationCheck : IIntegrationCheck
    {
        private const string SETTINGS_NO_MAX_SDK_KEY_ERROR = "VoodooSauce Settings is missing MaxAds SDK Key";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings) => IntegrationCheckWithBuildTarget(settings, EditorUserBuildSettings.activeBuildTarget);

        // This other method is used for the integration tests.
        public static List<IntegrationCheckMessage> IntegrationCheckWithBuildTarget(VoodooSettings settings, BuildTarget target)
        {
            var list = new List<IntegrationCheckMessage>();
            if ((string.IsNullOrEmpty(settings.MaxIosSdkKey) && target == BuildTarget.iOS) || (string.IsNullOrEmpty(settings.MaxAndroidSdkKey) && target == BuildTarget.Android)) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_NO_MAX_SDK_KEY_ERROR));
            }

            return list;
        }
    }
}