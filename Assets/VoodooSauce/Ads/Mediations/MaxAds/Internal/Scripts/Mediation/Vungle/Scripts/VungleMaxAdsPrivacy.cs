using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class VungleMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Vungle";

        public string PrivacyPolicyUrl => "https://vungle.com/privacy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
