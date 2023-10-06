using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class FacebookMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "FacebookAudienceNetwork";

        public string PrivacyPolicyUrl => "https://www.facebook.com/about/privacy";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
