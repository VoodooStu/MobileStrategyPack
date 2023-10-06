using Voodoo.Sauce.Privacy;
using UnityEngine.Scripting;

// ReSharper disable once CheckNamespace
namespace  Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class BidMachineMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "BidMachine";

        public string PrivacyPolicyUrl => "https://bidmachine.io/privacy-policy/";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}