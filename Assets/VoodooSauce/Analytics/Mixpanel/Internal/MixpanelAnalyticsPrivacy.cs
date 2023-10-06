using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    public class MixpanelAnalyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "Mixpanel";

        public string PrivacyPolicyUrl => "https://mixpanel.com/legal/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}
