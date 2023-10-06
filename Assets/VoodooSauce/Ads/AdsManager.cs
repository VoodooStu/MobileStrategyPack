using System.Linq;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Ads.FakeMediation;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal static class AdsManager
    {
#region Properties

        private const string TAG = "AdsManager";

        private static Banner _banner;
        public static Banner Banner => _banner ?? (_banner = new Banner(MediationAdapter));

        private static Interstitial _interstitial;
        public static Interstitial Interstitial => _interstitial ?? (_interstitial = new Interstitial(MediationAdapter));

        private static RewardedVideo _rewardedVideo;
        public static RewardedVideo RewardedVideo => _rewardedVideo ?? (_rewardedVideo = new RewardedVideo(MediationAdapter));

        private static Mrec _mrec;
        public static Mrec Mrec => _mrec ?? (_mrec = new Mrec(MediationAdapter)); 
        
        private static NativeAds _nativeAds;
        public static NativeAds NativeAds => _nativeAds ?? (_nativeAds = new NativeAds(MediationAdapter));

        private static RewardedInterstitialVideo _rewardedInterstitialVideo;
        public static RewardedInterstitialVideo RewardedInterstitialVideo => _rewardedInterstitialVideo ??
            (_rewardedInterstitialVideo = new RewardedInterstitialVideo(MediationAdapter));

        private static AppOpen _appOpen;
        public static AppOpen AppOpen => _appOpen ?? (_appOpen = new AppOpen(MediationAdapter));
        
        private static bool _enableReplaceRewardedOnCpm;
        public static bool EnableReplaceRewardedOnCpm => _enableReplaceRewardedOnCpm;
        
        private static bool _enableReplaceRewardedIfNotLoaded;
        public static bool EnableReplaceRewardedIfNotLoaded => _enableReplaceRewardedIfNotLoaded;
            
        private static AdDisplayConditions _adDisplayConditions;
        public static IFakeAdsManager fakeAdsManager;

        private static IMediationAdapter _mediationAdapter;
        public static IMediationAdapter MediationAdapter {
            get {
                if (_mediationAdapter != null) {
                    return _mediationAdapter;
                }

                if (AreFakeAdsEnabled()) {
                    _mediationAdapter = new FakeMediationAdapter();
                } else {
                    _mediationAdapter = MediationAdapterSelector.CreateMediationInstance();
                }

                return _mediationAdapter;
            }
        }
        
        private static AdsKeys _currentAdKeys;
        private static bool IsCcpaApplicable => VoodooSauceCore.GetPrivacy().IsCcpaApplicable();

#endregion

#region Methods

        /// <summary>
        /// Select the mediation before the Ads and Analytics modules are initialized (to handle the mediation parameter earlier
        /// in the VS lifecycle)
        /// The mediation name is used in the VAN events
        /// This method doesn't initialize the mediation but just selects randomly one of the available ones
        /// </summary>
        /// <returns>A string instance representing the selected mediation</returns>
        internal static string GetSelectedMediationName() => MediationAdapter.GetMediationType().ToString().ToLowerInvariant();

        internal static bool AreFakeAdsEnabled() => fakeAdsManager != null && fakeAdsManager.IsEnabled();

        private static bool IsFullScreenAdShowing() =>
            Interstitial.IsShowing() ||
            RewardedVideo.IsShowing() ||
            RewardedInterstitialVideo.IsShowing()||
            AppOpen.IsShowing();

        public static void OnApplicationPause(bool pauseStatus)
        {
            MediationAdapter.OnApplicationPause(pauseStatus);
            AppOpen.AdDisplayConditions?.OnApplicationPause(pauseStatus, IsFullScreenAdShowing());

            if (!pauseStatus) {
                AppOpen.Show();
            }
        }
        
        private static bool HasPaidToHideAds() => VoodooSauce.IsPremium() || VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct();
        
#endregion
        
#region Initialization

        public static void Initialize(VoodooSettings settings)
        {
            VoodooLog.LogDebug(Module.ADS, TAG, $"Mediation init start: {MediationAdapter.GetMediationType().ToString()}");

            if (_adDisplayConditions != null) {
                return;
            }
            
            if (settings.UseRemoteConfig) {
                var adFrequencyConfiguration = VoodooSauce.GetItemOrDefault<AdFrequencyConfiguration>();
                var appOpenConfiguration = VoodooSauce.GetItemOrDefault<AppOpenAdConfig>();
                _adDisplayConditions = new AdDisplayConditions(
                    adFrequencyConfiguration.delayInSecondsBeforeFirstInterstitialAd,
                    adFrequencyConfiguration.delayInSecondsBeforeSessionFirstInterstitialAd,
                    adFrequencyConfiguration.delayInSecondsBetweenInterstitialAds,
                    adFrequencyConfiguration.maxGamesBetweenInterstitialAds,
                    adFrequencyConfiguration.delayInSecondsBetweenRewardedVideoAndInterstitial,
                    appOpenConfiguration.minimumBackgroundTime,
                    appOpenConfiguration.aoToAoCooldown,
                    appOpenConfiguration.fsToAoCooldown,
                    appOpenConfiguration.rvToAoCooldown,
                    appOpenConfiguration.aoToFsCooldown
                );
                _enableReplaceRewardedOnCpm = adFrequencyConfiguration.enableReplaceRewardedIfInterstitialCpmHigher;
                _enableReplaceRewardedIfNotLoaded = adFrequencyConfiguration.enableReplaceRewardedIfNotLoaded;
            } else {
                _adDisplayConditions = new AdDisplayConditions(
                    settings.AppOpenAdConfig.minimumBackgroundTime,
                    settings.AppOpenAdConfig.aoToAoCooldown,
                    settings.AppOpenAdConfig.fsToAoCooldown,
                    settings.AppOpenAdConfig.rvToAoCooldown,
                    settings.AppOpenAdConfig.aoToFsCooldown);
                _enableReplaceRewardedOnCpm = settings.EnableReplaceRewardedOnCpm;
                _enableReplaceRewardedIfNotLoaded = settings.EnableReplaceRewardedIfNotLoaded;
            }
            
            UpdateAdDisplayConditions();
                
            if (VoodooPremium.IsPremium()) {
                _enableReplaceRewardedOnCpm = false;
                _enableReplaceRewardedIfNotLoaded = false;
            }
        }

        public static void InitializeMediation(VoodooSettings settings, bool consent)
        {
            int sessionCount = AnalyticsStorageHelper.Instance.GetAppLaunchCount();
            AdUnitsRemoteConfigs remoteAdUnits = VoodooSauce.GetItems<AdUnitsRemoteConfigs>().FirstOrDefault();
            AdUnitsManager.Initialize(settings, sessionCount, remoteAdUnits);

            _currentAdKeys = AdUnitsManager.adsKeys;
            
            if (HasPaidToHideAds()) {
                VoodooLog.LogWarning(Module.ADS, TAG, "AppOpen can not be enabled because the player is premium or bought the noads");
                DisableAppOpen();
            } else if (!_currentAdKeys.enableAppOpenAdSoftLaunch) {
                VoodooLog.LogWarning(Module.ADS, TAG, "AppOpen can not be enabled because the feature is disabled for this game");
                DisableAppOpen();
            } else {
                EnableAppOpen();
            }

            MediationAdapter.Initialize(_currentAdKeys, HasPaidToHideAds(), consent, IsCcpaApplicable, OnSdkInitialized);
        }

        private static void OnSdkInitialized()
        {
            if (!HasPaidToHideAds()) {
                Banner.Initialize();
                Interstitial.Initialize();
                Mrec.Initialize();
                NativeAds.Initialize();
                AppOpen.Initialize();
            }

            if (!VoodooSuperPremium.IsSuperPremium()) {
                RewardedVideo.Initialize();
            }
        }

        public static void SetConsent(bool consent)
        {
            MediationAdapter.SetConsent(consent, IsCcpaApplicable);
        }

        public static void SetAdUnit(AdUnitType type, string adUnit)
        {
            MediationAdapter.SetAdUnit(type, adUnit);
        }

#endregion

#region AppOpen Methods

        private static void DisableAppOpen()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen disabled");
            
            _adDisplayConditions.DisableAppOpenAd();
            UpdateAdDisplayConditions();
            AppOpen.Disable();
        }

        private static void EnableAppOpen()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen feature enabled");
            
            _adDisplayConditions.EnableAppOpenAd();
            UpdateAdDisplayConditions();
            AppOpen.Enable();
        }

#endregion

#region Ads Display Conditions

        public static void SetInterstitialAdsDisplayConditions(int delayInSecondsBeforeFirstInterstitialAd,
                                                               int delayInSecondsBeforeSessionFirstInterstitial,
                                                               int delayInSecondsBetweenInterstitialAds,
                                                               int maxGamesBetweenInterstitialAds,
                                                               int delayInSecondsBetweenRewardedVideoAndInterstitial,
                                                               float delayInSecondsBetweenAppOpenAdAndInterstitial)
        {
            _adDisplayConditions = new AdDisplayConditions(_adDisplayConditions,
                delayInSecondsBeforeFirstInterstitialAd,
                delayInSecondsBeforeSessionFirstInterstitial,
                delayInSecondsBetweenInterstitialAds,
                maxGamesBetweenInterstitialAds,
                delayInSecondsBetweenRewardedVideoAndInterstitial,
                delayInSecondsBetweenAppOpenAdAndInterstitial);

            UpdateAdDisplayConditions();
        }

        internal static string GetInterstitialConditionSettings() => _adDisplayConditions?.InterstitialConditionsToString() ?? "";
        
        internal static string GetAppOpenConditionSettings() => _adDisplayConditions?.AppOpenConditionsToString() ?? "";

        internal static string GetInterstitialConditionStatusString() => _adDisplayConditions?.GetInterstitialConditionStatusString() ?? "";

        internal static string GetInterstitialConditionTimeString() => _adDisplayConditions == null
            ? "Ads not initialized"
            : _adDisplayConditions.GetInterstitialConditionTimeString();

        internal static bool AreInterstitialDisplayConditionsMet() => _adDisplayConditions != null && _adDisplayConditions.AreInterstitialConditionsMet();

        internal static void TriggerInterstitialAdConditionsDisplay() => _adDisplayConditions?.InterstitialDisplayed();

        public static void IncrementGamesPlayed()
        {
            _adDisplayConditions?.IncrementGamesPlayed();
        }

        private static void UpdateAdDisplayConditions()
        {
            Banner.AdDisplayConditions = _adDisplayConditions;
            Interstitial.AdDisplayConditions = _adDisplayConditions;
            RewardedVideo.AdDisplayConditions = _adDisplayConditions;
            AppOpen.AdDisplayConditions = _adDisplayConditions;
        }
        
#endregion

#region Amazon Ads

        public static bool EnableAmazonTesting() => MediationAdapter.EnableAmazonTesting();

#endregion
    }
}