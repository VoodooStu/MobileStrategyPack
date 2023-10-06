using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class FyberMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Fyber";

        public string PrivacyPolicyUrl => "https://www.fyber.com/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
