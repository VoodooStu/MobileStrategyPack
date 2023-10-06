using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class UnityAdsMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "UnityAds";

        public string PrivacyPolicyUrl => "https://unity3d.com/legal/privacy-policy";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
