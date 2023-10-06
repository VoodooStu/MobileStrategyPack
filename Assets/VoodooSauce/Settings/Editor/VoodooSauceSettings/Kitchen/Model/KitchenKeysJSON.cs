using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    /*
     * This class contains all keys handled by the Kitchen server.
     * It's important to add these keys to the 'VoodooSauceSettingsHelper.cs' class in order to initialize the settings values. 
     */
    
    [Serializable]
    public class KitchenKeysJSON
    {
        public KitchenValueJSON AdjustAppToken;
        public KitchenValueJSON AdjustSignatureLinkV2;
        public KitchenValueJSON AdMobAppId;
        public KitchenValueJSON AppleStoreId;
        public KitchenValueJSON FirebaseConfigLink;
        public KitchenValueJSON GameAnalyticsGameKey;
        public KitchenValueJSON GameAnalyticsSecretKey;
        public KitchenValueJSON MaxAdsBannerAdUnit;
        public KitchenValueJSON MaxAdsInterstitialAdUnit;
        public KitchenValueJSON MaxAdsRewardedVideoAdUnit;
        public KitchenValueJSON MaxAdsMrecAdUnit;
        public KitchenValueJSON MaxAdsNativeAdsAdUnit;
        public KitchenValueJSON MaxAdsAppOpenAdUnit;
        public KitchenValueJSON MaxAdsSessionCountThreshold;
        public KitchenValueJSON MaxAdsSecondaryBannerAdUnit;
        public KitchenValueJSON MaxAdsSecondaryInterstitialAdUnit;
        public KitchenValueJSON MaxAdsSecondaryRewardedVideoAdUnit;
        public KitchenValueJSON MaxAdsSecondaryAppOpenAdUnit;
        public KitchenValueJSON MaxPercentOfTotalCohorts;
        public KitchenValueJSON MaxSdkKey;
        public KitchenValueJSON IronSourceMediationAppKey;
        public KitchenValueJSON MixpanelProdToken;
        public KitchenValueJSON UseFirebaseAnalytics;
        public KitchenValueJSON UseRemoteConfig;
        public KitchenValueJSON UseVoodooAnalytics;

        public KitchenValueJSON EnableABTests;
        public KitchenValueJSON EnableCustomABTestsCountryCodes;
        public KitchenValueJSON AppRaterEnabled;
        public KitchenValueJSON CrossPromotionEnabled;
        public KitchenValueJSON BannerCloseButtonEnabled;
        public KitchenValueJSON IAPEnabled;
        
        public KitchenValueJSON UseConversionEvents;
        public KitchenValueJSON ConversionDaysUntilExpiration;
        public KitchenValueJSON ConversionEvent1;
        public KitchenValueJSON ConversionEvent2;
        public KitchenValueJSON ConversionEvent3;
        public KitchenValueJSON ConversionEvent4;
        public KitchenValueJSON ConversionEvent5;

        public KitchenValueJSON EnableReplaceRewardedOnCpm;
        public KitchenValueJSON EnableReplaceRewardedIfNotLoaded;
        
        public KitchenValueJSON audioAdNetwork;
        public KitchenValueJSON gameStartTriggerFrequency;
        public KitchenValueJSON coolDownBetweenAudioAds;
        public KitchenValueJSON killWhenFsOrRvStarts;
        public KitchenValueJSON killWhenGameFinishes;

        public KitchenValueJSON odeeoApiKey;
        public KitchenValueJSON odeeoButtonType;

        // AppOpen Ads Configuration
        public KitchenValueJSON appOpenAdSoftLaunchEnabled;
        public KitchenValueJSON appOpenMinimumBackgroundTime;
        public KitchenValueJSON aoToAoCooldown;
        public KitchenValueJSON fsToAoCooldown;
        public KitchenValueJSON rvToAoCooldown;
        public KitchenValueJSON aoToFsCooldown;

        public KitchenValueJSON embraceAppId;
        public KitchenValueJSON embraceApiToken;
        public KitchenValueJSON embraceUserPercentage;

        public KitchenValueJSON amazonSDKKey;
        public KitchenValueJSON amazonBannerAdUnitId;
        public KitchenValueJSON amazonInterstitialAdUnitId;
        public KitchenValueJSON amazonRewardedVideoAdUnitId;
    }
}