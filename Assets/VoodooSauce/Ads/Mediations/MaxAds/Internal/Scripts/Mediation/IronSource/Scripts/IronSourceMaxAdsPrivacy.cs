using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class IronSourceMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "IronSource";

        public string PrivacyPolicyUrl => "https://developers.ironsrc.com/ironsource-mobile/air/ironsource-mobile-privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
