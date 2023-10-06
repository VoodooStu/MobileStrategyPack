using Voodoo.Sauce.Privacy;
using UnityEngine.Scripting;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    [Preserve]
    public class AmazonMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Amazon";

        public string PrivacyPolicyUrl => "https://aps.amazon.com/aps/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}