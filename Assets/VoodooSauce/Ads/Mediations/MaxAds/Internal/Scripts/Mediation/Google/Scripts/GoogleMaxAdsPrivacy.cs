using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class GoogleMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "AdMob";

        public string PrivacyPolicyUrl => "https://policies.google.com/privacy";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
