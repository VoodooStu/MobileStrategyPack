using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class OguryPresageMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Ogury";

        public string PrivacyPolicyUrl => "https://ogury.com/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}