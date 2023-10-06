using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Analytics
{
    public class GameAnalyticsIntegrationCheck : IIntegrationCheck
    {
        private const string GANoIOSKeyError = "VoodooSauce Settings is missing iOS GameAnalytics keys";
        private const string GANoAndroidAndSecretKeyError =
            "VoodooSauce Settings needs both Android game and secret keys! Leave both fields empty to disable Android analytics";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();

            bool iosKeysHadBeenSet = !string.IsNullOrEmpty(settings.GameAnalyticsIosGameKey)
                && !string.IsNullOrEmpty(settings.GameAnalyticsIosSecretKey);
            bool useIos = !string.IsNullOrEmpty(settings.GameAnalyticsIosGameKey)
                || !string.IsNullOrEmpty(settings.GameAnalyticsIosSecretKey);

            if (useIos && !iosKeysHadBeenSet) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, GANoIOSKeyError));
            }

            bool androidKeysHadBeenSet = !string.IsNullOrEmpty(settings.GameAnalyticsAndroidGameKey)
                && !string.IsNullOrEmpty(settings.GameAnalyticsAndroidSecretKey);
            bool useAndroid = !string.IsNullOrEmpty(settings.GameAnalyticsAndroidGameKey)
                || !string.IsNullOrEmpty(settings.GameAnalyticsAndroidSecretKey);

            if (useAndroid && !androidKeysHadBeenSet) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, GANoAndroidAndSecretKeyError));
            }

            return list;
        }
    }
}