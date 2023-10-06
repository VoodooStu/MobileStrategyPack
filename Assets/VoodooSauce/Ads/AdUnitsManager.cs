using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Ads
{
    internal static class AdUnitsManager
    {
        internal static AdsKeys adsKeys;

        internal static void Initialize(VoodooSettings settings, int sessionCount, AdUnitsRemoteConfigs remoteAdUnits)
        {
            adsKeys = PlatformUtils.UNITY_ANDROID ? settings.MaxAdsAndroidAdsKeys : settings.MaxAdsIosAdsKeys;
            adsKeys.enableFakeAds = settings.EnableInEditorUnityAds;
            adsKeys.enableFakeInterstitialAds = settings.EnableInEditorFSAds;
            adsKeys.enableFakeRewardedVideoAds = settings.EnableInEditorRVAds;
            adsKeys.enableFakeAppOpenAds = settings.EnableInEditorAOAds;
            adsKeys.maxSdkKey = settings.MaxSdkKey;
            adsKeys.ironSourceMediationAppKey = settings.GetIronSourceMediationAppKey();
            adsKeys.enableAppOpenAdSoftLaunch = settings.EnableAppOpenAdSoftLaunch;
            
            adsKeys.InitAdUnits(sessionCount, remoteAdUnits);
        }
    }
}