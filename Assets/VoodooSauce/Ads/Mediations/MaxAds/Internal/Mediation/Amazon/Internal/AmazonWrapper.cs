using System;
using AmazonAds;
using LegacyAmazonConfig = Voodoo.Sauce.Internal.Ads.MaxMediation.AmazonConfig;
using NewAmazonConfig = Voodoo.Sauce.Internal.Ads.AmazonConfig;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    public static class AmazonWrapper
    {
        private const string TAG = "AmazonWrapper";
        private const string AMAZON_AD_ERROR = "amazon_ad_error";
        private const string AMAZON_AD_RESPONSE = "amazon_ad_response";

        private static NewAmazonConfig _config;
        private static NewAmazonConfig Config {
            get {
                if (_config != null) {
                    return _config;
                }

                var newConfig = VoodooSauce.GetItem<NewAmazonConfig>();
                if (newConfig != null) {
                    _config = newConfig;
                    VoodooLog.LogDebug(Module.ADS, TAG, $"Amazon configuration loaded from VT, enabled : ${_config.enabled}");
                    return _config;
                }
                
                var legacyConfig = VoodooSauce.GetItem<LegacyAmazonConfig>();
                if (legacyConfig != null) {
                    _config = legacyConfig;
                    VoodooLog.LogDebug(Module.ADS, TAG, $"Legacy Amazon configuration loaded from VT, enabled : ${_config.enabled}");
                    return _config;
                }
                
                _config = new NewAmazonConfig();
                VoodooLog.LogDebug(Module.ADS, TAG, $"Amazon configuration loaded locally, enabled : ${_config.enabled}");
                return _config;
            }            
        }

        private static AmazonKey _amazonKey;

        private static bool IsEnabled()
        {
            if (_amazonKey == null || !Config.enabled) return false;

            return !string.IsNullOrEmpty(_amazonKey.sdkKey) && (!string.IsNullOrEmpty(_amazonKey.bannerAdUnit) ||
                                                                !string.IsNullOrEmpty(_amazonKey.interstitialAdUnit) ||
                                                                !string.IsNullOrEmpty(_amazonKey.rewardedVideoAdUnit));
        }
        
        public static void Initialize(AmazonKey amazonKey)
        {
            _amazonKey = amazonKey;

            if (!Config.enabled) {
                VoodooLog.LogWarning(Module.ADS, TAG, "Amazon SDK can not be initialized because it's not enabled");
            } else {
                if (_amazonKey == null) {
                    VoodooLog.LogError(Module.ADS, TAG, "Amazon SDK can not be initialized because the keys are not configured");
                } else {
                    if (string.IsNullOrEmpty(_amazonKey.sdkKey)) {
                        VoodooLog.LogError(Module.ADS, TAG, "Amazon SDK can not be initialized because the SDK key is not configured");
                    }
                    if (string.IsNullOrEmpty(_amazonKey.bannerAdUnit) &&
                        string.IsNullOrEmpty(_amazonKey.interstitialAdUnit) &&
                        string.IsNullOrEmpty(_amazonKey.rewardedVideoAdUnit)) {
                        VoodooLog.LogError(Module.ADS, TAG, "Amazon SDK can not be initialized because none amazon ad unit id is configured");
                    }
                }
            }
            
            if (!IsEnabled()) return;
            
            Amazon.Initialize(_amazonKey.sdkKey);
            Amazon.EnableTesting(false);
            Amazon.EnableLogging(VoodooLog.IsDebugLogsEnabled);
            Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
            
            VoodooLog.LogDebug(Module.ADS, TAG, $"Amazon SDK initialized with SDK key: {_amazonKey.sdkKey}, banner ad unit id: {_amazonKey.bannerAdUnit}, interstitial ad unit id: {_amazonKey.interstitialAdUnit}, rewarded video ad unit id: {_amazonKey.rewardedVideoAdUnit}");
        }

        public static void InitializeBanner(AdUnits adUnits, Action callback)
        {
            if (!IsEnabled() || string.IsNullOrEmpty(_amazonKey.bannerAdUnit)) {
                callback?.Invoke();
                return;
            }
            
            VoodooLog.LogDebug(Module.ADS, TAG, "Amazon banner initialized");
            
            var apsBanner = new APSBannerAdRequest(320, 50, _amazonKey.bannerAdUnit);
            apsBanner.onSuccess += adResponse =>
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Amazon banner request succeeded");
                MaxSdk.SetBannerLocalExtraParameter(adUnits.bannerAdUnit, AMAZON_AD_RESPONSE,
                    adResponse.GetResponse());
                callback?.Invoke();
            };
            apsBanner.onFailedWithError += adError =>
            {
                VoodooLog.LogError(Module.ADS, TAG, 
                    $"Amazon banner request error. {adError.GetMessage()} || {adError.GetCode()}");
                MaxSdk.SetBannerLocalExtraParameter(adUnits.bannerAdUnit, AMAZON_AD_ERROR, adError.GetAdError());
                callback?.Invoke();
            };

            apsBanner.LoadAd();
        }

        public static void InitializeInterstitial(AdUnits adUnits, Action callback)
        {
            if (!IsEnabled() || string.IsNullOrEmpty(_amazonKey.interstitialAdUnit)) {
                callback?.Invoke();
                return;
            }
            
            VoodooLog.LogDebug(Module.ADS, TAG, "Amazon interstitial initialized");
            
            var interstitialAdRequest = new APSVideoAdRequest(320, 480, _amazonKey.interstitialAdUnit);
            interstitialAdRequest.onSuccess += adResponse => {
                VoodooLog.LogDebug(Module.ADS, TAG, "Amazon interstitial request succeeded");
                MaxSdk.SetInterstitialLocalExtraParameter(adUnits.interstitialAdUnit, AMAZON_AD_RESPONSE, adResponse.GetResponse());
                callback?.Invoke();
            };
            interstitialAdRequest.onFailedWithError += adError => {
                VoodooLog.LogError(Module.ADS, TAG, 
                    $"Amazon interstitial request error. {adError.GetMessage()} || {adError.GetCode()}");
                MaxSdk.SetInterstitialLocalExtraParameter(adUnits.interstitialAdUnit, AMAZON_AD_ERROR, adError.GetAdError());
                callback?.Invoke();
            };
            interstitialAdRequest.LoadAd();
        }

        public static void InitializeRewardedVideo(AdUnits adUnits, Action callback)
        {
            if (!IsEnabled() || string.IsNullOrEmpty(_amazonKey.rewardedVideoAdUnit)) {
                callback?.Invoke();
                return;
            }
            
            VoodooLog.LogDebug(Module.ADS, TAG, "Amazon rewarded video initialized");
            
            var rewardedVideoAdRequest = new APSVideoAdRequest(320, 480, _amazonKey.rewardedVideoAdUnit);
            rewardedVideoAdRequest.onSuccess += adResponse => {
                VoodooLog.LogDebug(Module.ADS, TAG, "Amazon rewarded video request succeeded");
                MaxSdk.SetRewardedAdLocalExtraParameter(adUnits.rewardedVideoAdUnit, AMAZON_AD_RESPONSE, adResponse.GetResponse());
                callback?.Invoke();
            };
            rewardedVideoAdRequest.onFailedWithError += adError => {
                VoodooLog.LogError(Module.ADS, TAG, 
                    $"Amazon rewarded video request error. {adError.GetMessage()} || {adError.GetCode()}");
                MaxSdk.SetRewardedAdLocalExtraParameter(adUnits.rewardedVideoAdUnit, AMAZON_AD_ERROR, adError.GetAdError());
                callback?.Invoke();
            };
            rewardedVideoAdRequest.LoadAd();
        }
    }
}
