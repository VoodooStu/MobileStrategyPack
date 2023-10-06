using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Editor
{
    /*
     * This class provides "backwards compatibility" support: it's used to translate the new Kitchen settings to the legacy VoodooSettings.
     * As soon as the Unity VoodooSauce Settings UI is updated this class will be updated too.
     */
    public static class VoodooSauceSettingsHelper
    {
        public static VoodooSettings ReloadSettings() => LoadSettings("");

        /// <summary>
        /// This method merges the kitchen settings into the VS Settings asset file.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        /// <exception cref="SettingsNotCreatedException"></exception>
        public static VoodooSettings LoadSettings(string store)
        {
            // Do nothing if the kitchen settings are not cached.
            KitchenSettingsJSON kitchenSettings = KitchenSettingsJSON.Load();
            if (!kitchenSettings.IsInitialized) {
                throw new SettingsNotCreatedException(
                    "The new VS settings for this game are not cached or are not created yet. Please refresh the VS settings or ask the game manager to create them.");
            }

            VoodooSettings voodooSettings = VoodooSettings.Load();

            // If the store is not provided so the current settings are refreshed. 
            if (string.IsNullOrEmpty(store)) {
                store = voodooSettings.Store;

                // If the store is empty, that means that it is the first time this script is running
                // for the VS Settings V2. So the store is set to the settings here.
                if (string.IsNullOrEmpty(store)) {
                    store = VoodooSettings.DEFAULT_STORE;
                }
            }

            // Check if the store is supported and is cached.
            if (!kitchenSettings.IsStoreSupported(store)) {
                throw new StoreNotSupportedException($"The store '{store}' is not supported.");
            }

            Debug.Log($"Loading the settings from the store '{store}'.");

            // Select the iOS and Android store settings to load.
            KitchenStoreJSON iosStore = kitchenSettings.GetSettingsFromStoreAndPlatform(store, KitchenStoreJSON.PLATFORM_IOS)
                ?? new KitchenStoreJSON();
            KitchenStoreJSON androidStore = kitchenSettings.GetSettingsFromStoreAndPlatform(store, KitchenStoreJSON.PLATFORM_ANDROID)
                ?? new KitchenStoreJSON();

            KitchenKeysJSON iosKeys = iosStore.settings;
            KitchenKeysJSON androidKeys = androidStore.settings;

            voodooSettings.Store = store;
            voodooSettings.IOSBundleID = iosStore.BundleId;
            voodooSettings.AndroidBundleID = androidStore.BundleId;
            voodooSettings.LastUpdateDate = kitchenSettings.LastUpdate.ToString("dd/MM/yyyy HH:mm:ss");

            // ios
            voodooSettings.AppleStoreId = iosKeys.AppleStoreId.value;
            voodooSettings.AdjustIosAppToken = iosKeys.AdjustAppToken.value;
            voodooSettings.AdMobIosAppId = iosKeys.AdMobAppId.value;
            voodooSettings.GameAnalyticsIosGameKey = iosKeys.GameAnalyticsGameKey.value;
            voodooSettings.GameAnalyticsIosSecretKey = iosKeys.GameAnalyticsSecretKey.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.bannerAdUnit = iosKeys.MaxAdsBannerAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.interstitialAdUnit = iosKeys.MaxAdsInterstitialAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.rewardedVideoAdUnit = iosKeys.MaxAdsRewardedVideoAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.mrecAdUnit = iosKeys.MaxAdsMrecAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.nativeAdsAdUnit = iosKeys.MaxAdsNativeAdsAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsAdUnits.appOpenAdUnit = iosKeys.MaxAdsAppOpenAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.sessionCountThreshold = iosKeys.MaxAdsSessionCountThreshold.IntergerValue();
            voodooSettings.MaxAdsIosAdsKeys.maxAdsSecondaryAdUnits.bannerAdUnit = iosKeys.MaxAdsSecondaryBannerAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsSecondaryAdUnits.interstitialAdUnit = iosKeys.MaxAdsSecondaryInterstitialAdUnit.value;
            voodooSettings.MaxAdsIosAdsKeys.maxAdsSecondaryAdUnits.rewardedVideoAdUnit = iosKeys.MaxAdsSecondaryRewardedVideoAdUnit.value;
            voodooSettings.MaxPercentOfTotalIosCohorts = iosKeys.MaxPercentOfTotalCohorts.FloatValue();
            voodooSettings.UseMixpanelIos = iosKeys.EnableABTests.BoolValue();
            voodooSettings.EnableCustomIosABTestsCountryCodes = iosKeys.EnableCustomABTestsCountryCodes.BoolValue();
            voodooSettings.iOSAppRaterEnabled = iosKeys.AppRaterEnabled.BoolValue();
            voodooSettings.iOSCrossPromotionEnabled = iosKeys.CrossPromotionEnabled.BoolValue();
            voodooSettings.iOSBannerCloseButtonEnabled = iosKeys.BannerCloseButtonEnabled.BoolValue();
            voodooSettings.iOSIAPEnabled = iosKeys.IAPEnabled.BoolValue();
            voodooSettings.MaxIosSdkKey = iosKeys.MaxSdkKey.value;
            voodooSettings.IronSourceMediationIosAppKey= iosKeys.IronSourceMediationAppKey.value;
            voodooSettings.MixpanelIosProdToken = iosKeys.MixpanelProdToken.value;
            voodooSettings.UseIosFirebaseAnalytics = iosKeys.UseFirebaseAnalytics.BoolValue();
            voodooSettings.UseIosRemoteConfig = iosKeys.UseRemoteConfig.BoolValue();
            voodooSettings.UseIosVoodooAnalytics = iosKeys.UseVoodooAnalytics.BoolValue();
            voodooSettings.ConversionIosEvents = BuildConversionEventsSettings(iosKeys);
            voodooSettings.EnableIosReplaceRewardedOnCpm = iosKeys.EnableReplaceRewardedOnCpm.BoolValue();
            voodooSettings.EnableIosReplaceRewardedIfNotLoaded = iosKeys.EnableReplaceRewardedIfNotLoaded.BoolValue();
            voodooSettings.EnableIosAppOpenAdSoftLaunch = iosKeys.appOpenAdSoftLaunchEnabled.BoolValue();
            voodooSettings.iOSAppOpenAdConfig = BuildAppOpenAdConfig(iosKeys);
            voodooSettings.iOSAudioAdConfig = BuildAudioAdConfig(iosKeys);
            voodooSettings.iOSOdeeoConfig = BuildOdeeoConfig(iosKeys);
            voodooSettings.embraceIosApiToken = iosKeys.embraceApiToken.value;
            voodooSettings.embraceIosAppId = iosKeys.embraceAppId.value;
            voodooSettings.embraceIosUserPercentage = iosKeys.embraceUserPercentage.FloatValue();
            voodooSettings.amazonIosKey = new AmazonKey
            {
                sdkKey = iosKeys.amazonSDKKey.value,
                bannerAdUnit = iosKeys.amazonBannerAdUnitId.value,
                interstitialAdUnit = iosKeys.amazonInterstitialAdUnitId.value,
                rewardedVideoAdUnit = iosKeys.amazonRewardedVideoAdUnitId.value
            };
            
            // android
            voodooSettings.AdjustAndroidAppToken = androidKeys.AdjustAppToken.value;
            voodooSettings.AdMobAndroidAppId = androidKeys.AdMobAppId.value;
            voodooSettings.GameAnalyticsAndroidGameKey = androidKeys.GameAnalyticsGameKey.value;
            voodooSettings.GameAnalyticsAndroidSecretKey = androidKeys.GameAnalyticsSecretKey.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.bannerAdUnit = androidKeys.MaxAdsBannerAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.interstitialAdUnit = androidKeys.MaxAdsInterstitialAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.rewardedVideoAdUnit = androidKeys.MaxAdsRewardedVideoAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.mrecAdUnit = androidKeys.MaxAdsMrecAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.nativeAdsAdUnit = androidKeys.MaxAdsNativeAdsAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsAdUnits.appOpenAdUnit = androidKeys.MaxAdsAppOpenAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.sessionCountThreshold = androidKeys.MaxAdsSessionCountThreshold.IntergerValue();
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsSecondaryAdUnits.bannerAdUnit = androidKeys.MaxAdsSecondaryBannerAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsSecondaryAdUnits.interstitialAdUnit = androidKeys.MaxAdsSecondaryInterstitialAdUnit.value;
            voodooSettings.MaxAdsAndroidAdsKeys.maxAdsSecondaryAdUnits.rewardedVideoAdUnit = androidKeys.MaxAdsSecondaryRewardedVideoAdUnit.value;
            voodooSettings.MaxPercentOfTotalAndroidCohorts = androidKeys.MaxPercentOfTotalCohorts.FloatValue();
            voodooSettings.UseMixpanelAndroid = androidKeys.EnableABTests.BoolValue();
            voodooSettings.EnableCustomAndroidABTestsCountryCodes = androidKeys.EnableCustomABTestsCountryCodes.BoolValue();
            voodooSettings.AndroidAppRaterEnabled = androidKeys.AppRaterEnabled.BoolValue();
            voodooSettings.AndroidCrossPromotionEnabled = androidKeys.CrossPromotionEnabled.BoolValue();
            voodooSettings.AndroidBannerCloseButtonEnabled = androidKeys.BannerCloseButtonEnabled.BoolValue();
            voodooSettings.AndroidIAPEnabled = androidKeys.IAPEnabled.BoolValue();
            voodooSettings.MaxAndroidSdkKey = androidKeys.MaxSdkKey.value;
            voodooSettings.IronSourceMediationAndroidAppKey = androidKeys.IronSourceMediationAppKey.value;
            voodooSettings.MixpanelAndroidProdToken = androidKeys.MixpanelProdToken.value;
            voodooSettings.UseAndroidFirebaseAnalytics = androidKeys.UseFirebaseAnalytics.BoolValue();
            voodooSettings.UseAndroidRemoteConfig = androidKeys.UseRemoteConfig.BoolValue();
            voodooSettings.UseAndroidVoodooAnalytics = androidKeys.UseVoodooAnalytics.BoolValue();
            voodooSettings.ConversionAndroidEvents = BuildConversionEventsSettings(androidKeys);
            voodooSettings.EnableAndroidReplaceRewardedOnCpm = androidKeys.EnableReplaceRewardedOnCpm.BoolValue();
            voodooSettings.EnableAndroidReplaceRewardedIfNotLoaded =
                androidKeys.EnableReplaceRewardedIfNotLoaded.BoolValue();
            voodooSettings.EnableAndroidAppOpenAdSoftLaunch = androidKeys.appOpenAdSoftLaunchEnabled.BoolValue();
            voodooSettings.AndroidAppOpenAdConfig = BuildAppOpenAdConfig(androidKeys);
            voodooSettings.AndroidAudioAdConfig = BuildAudioAdConfig(androidKeys);
            voodooSettings.AndroidOdeeoConfig = BuildOdeeoConfig(androidKeys);
            voodooSettings.embraceAndroidApiToken = androidKeys.embraceApiToken.value;
            voodooSettings.embraceAndroidAppId = androidKeys.embraceAppId.value;
            voodooSettings.embraceAndroidUserPercentage = androidKeys.embraceUserPercentage.FloatValue();
            voodooSettings.amazonAndroidKey = new AmazonKey
            {
                sdkKey = androidKeys.amazonSDKKey.value,
                bannerAdUnit = androidKeys.amazonBannerAdUnitId.value,
                interstitialAdUnit = androidKeys.amazonInterstitialAdUnitId.value,
                rewardedVideoAdUnit = androidKeys.amazonRewardedVideoAdUnitId.value
            };
            
            // Update the player settings before copying the files
            // to avoid the firebase warning about the different bundle ids in the settings and the configuration file.
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, iosStore.BundleId);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, androidStore.BundleId);

            // Save the settings.
            UnityEditor.EditorUtility.SetDirty(voodooSettings);
            AssetDatabase.SaveAssets();

            // Move the cached files to their final locations.
            string iOSCachedFilesDirectory = iosStore.id != null ? CachedSettingsHelper.GetStoreFilesDirectory(iosStore.id) : null;
            string androidCachedFilesDirectory = androidStore.id != null ? CachedSettingsHelper.GetStoreFilesDirectory(androidStore.id) : null;

            // Move the Adjust files.
            string iosAdjustFilename = GetFileNameFromUrl(iosKeys.AdjustSignatureLinkV2.value);
            if (string.IsNullOrEmpty(iosAdjustFilename)) {
                AdjustHelper.RemoveIOSFiles();
            } else if (iOSCachedFilesDirectory != null) {
                AdjustHelper.CopyIOSFile(Path.Combine(iOSCachedFilesDirectory, iosAdjustFilename));
            }

            string androidAdjustFilename = GetFileNameFromUrl(androidKeys.AdjustSignatureLinkV2.value);
            if (string.IsNullOrEmpty(androidAdjustFilename)) {
                AdjustHelper.RemoveAndroidFiles();
            } else if (androidCachedFilesDirectory != null) {
                AdjustHelper.CopyAndroidFile(Path.Combine(androidCachedFilesDirectory, androidAdjustFilename));
            }

            // Move the Firebase files.
            string iosFirebaseFilename = GetFileNameFromUrl(iosKeys.FirebaseConfigLink.value);
            if (string.IsNullOrEmpty(iosFirebaseFilename)) {
                FirebaseHelper.RemoveIOSFiles();
            } else if (iOSCachedFilesDirectory != null) {
                FirebaseHelper.CopyIOSFile(Path.Combine(iOSCachedFilesDirectory, iosFirebaseFilename));
            }

            string androidFirebaseFilename = GetFileNameFromUrl(androidKeys.FirebaseConfigLink.value);
            if (string.IsNullOrEmpty(androidFirebaseFilename)) {
                FirebaseHelper.RemoveAndroidFiles();
            } else if (androidCachedFilesDirectory != null) {
                FirebaseHelper.CopyAndroidFile(Path.Combine(androidCachedFilesDirectory, androidFirebaseFilename));
            }

            return voodooSettings;
        }

        /// <summary>
        /// Returns true it the current store
        /// is configured to be built for the platform.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsCurrentStoreSupported(BuildTarget target)
        {
            VoodooSettings voodooSettings = VoodooSettings.Load();
            KitchenSettingsJSON kitchenSettings = KitchenSettingsJSON.Load();

            string platform;
            switch (target) {
                case BuildTarget.iOS:
                    platform = KitchenStoreJSON.PLATFORM_IOS;
                    break;
                case BuildTarget.Android:
                    platform = KitchenStoreJSON.PLATFORM_ANDROID;
                    break;
                default:
                    platform = KitchenStoreJSON.PLATFORM_IOS;
                    break;
            }

            // If the store does not exits it means that this configuration is not not allowed to build for this store and this platform.
            return kitchenSettings.GetSettingsFromStoreAndPlatform(voodooSettings.Store, platform) != null;
        }

        private static ConversionEventsSettings BuildConversionEventsSettings(KitchenKeysJSON keys)
        {
            string[] kitchenValues = {
                keys.ConversionEvent1.value, keys.ConversionEvent2.value, keys.ConversionEvent3.value, keys.ConversionEvent4.value,
                keys.ConversionEvent5.value
            };
            return ConversionEventsSettings.Build(keys.UseConversionEvents.BoolValue(), keys.ConversionDaysUntilExpiration.value, kitchenValues);
        }
        
        private static AppOpenAdConfig BuildAppOpenAdConfig(KitchenKeysJSON keys) => new AppOpenAdConfig
        {
            minimumBackgroundTime = keys.appOpenMinimumBackgroundTime.FloatValue(),
            aoToAoCooldown = keys.aoToAoCooldown.FloatValue(),
            fsToAoCooldown = keys.fsToAoCooldown.FloatValue(),
            rvToAoCooldown = keys.rvToAoCooldown.FloatValue(),
            aoToFsCooldown = keys.aoToFsCooldown.FloatValue()
        };

        private static AudioAdConfig BuildAudioAdConfig(KitchenKeysJSON keys)
        {
            return new AudioAdConfig
            {
                adNetwork = keys.audioAdNetwork.value,
                gameStartTriggerFrequency = keys.gameStartTriggerFrequency.IntergerValue(),
                coolDownBetweenAudioAds = keys.coolDownBetweenAudioAds.IntergerValue(),
                killWhenFsOrRvStarts = keys.killWhenFsOrRvStarts.BoolValue(),
                killWhenGameFinishes = keys.killWhenGameFinishes.BoolValue()
            };
        }

        private static OdeeoConfig BuildOdeeoConfig(KitchenKeysJSON keys)
        {
            return new OdeeoConfig
            {
                appKey = keys.odeeoApiKey.value,
                buttonType = keys.odeeoButtonType.value
            };
        }

        private static string GetFileNameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
    }
}