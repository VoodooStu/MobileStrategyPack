using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Ads
{
    [Preserve]
    public class ChartboostMaxAdsPrivacy : IPrivacyLink
    {
        public string SDKName => "Chartboost";

        public string PrivacyPolicyUrl => "https://answers.chartboost.com/en-us/articles/200780269";

        public PrivacySDKType SDKType => PrivacySDKType.AdNetworkMaxAds;
    }
}
