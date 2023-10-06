using Voodoo.Sauce.Privacy;
using UnityEngine.Scripting;

// ReSharper disable once CheckNamespace
namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class AdjustAnalyticsPrivacy : IPrivacyLink
    {
        public string SDKName => "Adjust";

        public string PrivacyPolicyUrl => "https://www.adjust.com/terms/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.Analytics;
    }
}