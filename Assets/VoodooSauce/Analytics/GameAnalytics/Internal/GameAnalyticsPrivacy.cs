using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class GameAnalyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "GameAnalytics";

        public string PrivacyPolicyUrl => "https://gameanalytics.com/privacy";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}
