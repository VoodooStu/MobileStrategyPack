using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class LineMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Line";

        public string PrivacyPolicyUrl => "https://line.me/en/terms/policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
