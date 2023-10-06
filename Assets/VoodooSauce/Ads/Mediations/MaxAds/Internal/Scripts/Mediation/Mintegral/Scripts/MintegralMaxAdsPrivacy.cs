using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class MintegralMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Mintegral";

        public string PrivacyPolicyUrl => "https://www.mintegral.com/en/privacy";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
