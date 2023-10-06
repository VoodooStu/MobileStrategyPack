using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [System.Serializable]
    public class AdsKeys
    {
        [Tooltip("Used when session count is above threshold")]
        public AdUnits maxAdsAdUnits;

        [Tooltip("Used when session count is below threshold")]
        public AdUnits maxAdsSecondaryAdUnits;

        [Tooltip("Used to determine which Ad units will be used")]
        public int sessionCountThreshold;
        
        [HideInInspector]
        public string maxSdkKey;
        
        // IronSource Mediation
        public string ironSourceMediationAppKey;

        // FakeAds
        [HideInInspector]
        public bool enableFakeAds;
        [HideInInspector]
        public bool enableFakeInterstitialAds;
        [HideInInspector]
        public bool enableFakeRewardedVideoAds;
        [HideInInspector]
        public bool enableFakeAppOpenAds;
        
        // AppOpen
        [HideInInspector]
        public bool enableAppOpenAdSoftLaunch;

        public AdUnits AdUnits { get; private set; }

        public string BannerAdUnit => AdUnits.bannerAdUnit;
        public string InterstitialAdUnit => AdUnits.interstitialAdUnit;
        public string RewardedVideoAdUnit => AdUnits.rewardedVideoAdUnit;
        public string MrecAdUnit => AdUnits.mrecAdUnit;
        public string NativeAdsAdUnit => AdUnits.nativeAdsAdUnit;
        public string AppOpenAdUnit => AdUnits.appOpenAdUnit;

        internal void InitAdUnits(int sessionCount, AdUnitsRemoteConfigs remoteConfig)
        {
            bool pastThreshold = sessionCount > sessionCountThreshold;

            AdUnits = new AdUnits {
                bannerAdUnit = string.IsNullOrEmpty(remoteConfig?.bannerAdUnit) == false ? remoteConfig.bannerAdUnit :
                    string.IsNullOrEmpty(maxAdsSecondaryAdUnits?.bannerAdUnit) || pastThreshold ? maxAdsAdUnits.bannerAdUnit :
                    maxAdsSecondaryAdUnits.bannerAdUnit,
                interstitialAdUnit = string.IsNullOrEmpty(remoteConfig?.interstitialAdUnit) == false ? remoteConfig.interstitialAdUnit :
                    string.IsNullOrEmpty(maxAdsSecondaryAdUnits?.interstitialAdUnit) || pastThreshold ? maxAdsAdUnits.interstitialAdUnit :
                    maxAdsSecondaryAdUnits.interstitialAdUnit,
                rewardedVideoAdUnit = string.IsNullOrEmpty(remoteConfig?.rewardedVideoAdUnit) == false ? remoteConfig.rewardedVideoAdUnit :
                    string.IsNullOrEmpty(maxAdsSecondaryAdUnits?.rewardedVideoAdUnit) || pastThreshold ? maxAdsAdUnits.rewardedVideoAdUnit :
                    maxAdsSecondaryAdUnits.rewardedVideoAdUnit,
            };
            
            // Get mRec ad unit if Remote config is disabled or no configuration or feature enabled
            if (remoteConfig == null || remoteConfig.mrecEnabled) {
                AdUnits.mrecAdUnit = string.IsNullOrEmpty(remoteConfig?.mrecAdUnit) == false ? remoteConfig.mrecAdUnit : maxAdsAdUnits.mrecAdUnit;
            }

            // Get nativeAds ad unit if Remote config is disabled or no configuration or feature enabled
            if (remoteConfig == null || remoteConfig.nativeAdsEnabled) {
                AdUnits.nativeAdsAdUnit = string.IsNullOrEmpty(remoteConfig?.nativeAdsAdUnit) == false
                    ? remoteConfig.nativeAdsAdUnit
                    : maxAdsAdUnits.nativeAdsAdUnit;
            }

            // Get the appOpen ad unit from the VS Settings if the remote settings are disabled
            if (remoteConfig == null) {
                AdUnits.appOpenAdUnit = maxAdsAdUnits.appOpenAdUnit;
            } else {
                // Get the appOpen ad unit from the remote settings (or the VS Settings)
                AdUnits.appOpenAdUnit = !string.IsNullOrEmpty(remoteConfig.appOpenAdUnit) ? remoteConfig.appOpenAdUnit : maxAdsAdUnits.appOpenAdUnit;
                enableAppOpenAdSoftLaunch = remoteConfig.appOpenAdSoftLaunchEnabled;
            }

            // In every case the feature is disabled if there is no appOpen unit ad provided
            if (string.IsNullOrEmpty(AdUnits.appOpenAdUnit)) {
                enableAppOpenAdSoftLaunch = false;
            }

            if (!enableAppOpenAdSoftLaunch) {
                AdUnits.appOpenAdUnit = "";
            }
        }

        public override string ToString() => $"MaxAdsAdUnits: {maxAdsAdUnits}\n\n" +
            $"Threshold: {sessionCountThreshold}\n\n" +
            $"MaxAdsSecondaryAdUnits: {maxAdsSecondaryAdUnits}\n\n" +
            $"MaxAdsSdkKey: {maxSdkKey}\n\n" +
            $"EnableFakeInUnityAds: {enableFakeAds}";

        public bool HasBannerKey() => !string.IsNullOrEmpty(AdUnits.bannerAdUnit);
        public bool HasInterstitialKey() => !string.IsNullOrEmpty(AdUnits.interstitialAdUnit);
        public bool HasRewardedVideoKey() => !string.IsNullOrEmpty(AdUnits.rewardedVideoAdUnit);
        public bool HasMrecKey() => !string.IsNullOrEmpty(AdUnits.mrecAdUnit);
        public bool HasNativeAdsKey() => !string.IsNullOrEmpty(AdUnits.nativeAdsAdUnit);
        public bool HasAppOpenKey() => !string.IsNullOrEmpty(AdUnits.appOpenAdUnit);
    }
}