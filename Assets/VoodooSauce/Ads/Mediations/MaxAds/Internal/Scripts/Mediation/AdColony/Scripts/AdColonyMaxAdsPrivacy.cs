using Voodoo.Sauce.Privacy;
using UnityEngine.Scripting;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class AdColonyMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "AdColony";

        public string PrivacyPolicyUrl => "https://www.adcolony.com/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}