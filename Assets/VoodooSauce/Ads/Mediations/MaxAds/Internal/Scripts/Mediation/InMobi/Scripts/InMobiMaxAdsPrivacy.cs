using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class InMobiMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "InMobi";

        public string PrivacyPolicyUrl => "https://www.inmobi.com/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
