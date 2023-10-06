using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class ByteDanceMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Pangle";

        public string PrivacyPolicyUrl => "https://www.tiktok.com/legal/privacy-policy";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
