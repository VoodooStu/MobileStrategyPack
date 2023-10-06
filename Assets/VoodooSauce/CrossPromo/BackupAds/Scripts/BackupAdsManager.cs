using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Canvas;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Settings;
using Voodoo.Sauce.Internal.CrossPromo.Configuration;
using Voodoo.Sauce.Internal.CrossPromo.Mercury;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Parameters;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;
using Voodoo.Sauce.Privacy;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts
{
    internal class BackupAdsManager
    {
#region Properties
        private static BackupAdsManager _instance;

        private const string TAG = "BackupAdsManager";
        private const string BACKUP_FS_FORMAT = "FS_APPINSTALL";
        private const string K_PREFS_LATEST_WATERFALL_ID = "BackupFS_LatestWaterfallId";
        private const float DELAY_BEFORE_SHOWING_CLOSE_BUTTON_INTERSTITIAL = 5f;
        private const float DELAY_BEFORE_SHOWING_CLOSE_BUTTON_REWARDED = 15f;
        private string LatestWaterfallId {
            get => PlayerPrefs.GetString(K_PREFS_LATEST_WATERFALL_ID, null);
            set {
                PlayerPrefs.SetString(K_PREFS_LATEST_WATERFALL_ID, value);
                PlayerPrefs.Save();
            }
        }
        
        private BackupAdsConfig _currentConfig;
        
        private bool _wasBannerShown;
        private static bool _initialized;
        private AdLoadingState _wasDownloaded;

        private BackupAdsCanvas _canvasInstance;
        
        internal List<MercuryPromotedAsset> currentAssets;
        internal MercuryWaterfall mercuryWaterfall;
        internal BackupAdsInfo currentAdInfo;
        internal BackupAdsInfo lastDisplayedAdInfo;
        
        private Action<bool> _completeCallback;

        private PrivacyCore.GdprConsent _gdprConsent;

#endregion

#region Initialize

        public static BackupAdsManager Instance {
            get {
                if (_instance == null) {
                    var config = VoodooSauce.GetItem<BackupAdsConfig>();
                    
                    // Old config just in case
                    if (config == null) {
                        config = VoodooSauce.GetItemOrDefault<BackupInterstitialConfig>();
                    }
                    
                    _instance = new BackupAdsManager(config);
                }

                if (_instance != null) _initialized = true;
                return _instance;
            }
        }

        internal BackupAdsManager(BackupAdsConfig config) => _currentConfig = config;

        public void Initialize(PrivacyCore.GdprConsent gdprConsent)
        {
            _gdprConsent = gdprConsent;
            
            if (!IsEnabled()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Backup FS is not enabled, not initializing");
                MercuryCache.ClearCache();
                return;
            }
            
            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Initializing BackupAds...");

            currentAssets = new List<MercuryPromotedAsset>();
            GetGameInfoParameters parameters = MercuryAPI.CreateDefaultGameInfoParameters(BACKUP_FS_FORMAT, null, "no_waterfall", IsRestrictedPrivacy());
            MercuryAPI.GetGameInfo(parameters, OnGameInfoReceived, OnGameInfoError);
        }

#endregion

#region Mercury Event

        private void OnGameInfoReceived(MercuryRequestInfo info)
        {
            mercuryWaterfall = info.waterfall;

            if (mercuryWaterfall.promote_assets.Length == 0) {
                _wasDownloaded = AdLoadingState.Disabled;
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Backup FS is initialized but no assets were found.");
            } else {
                MercuryCache.SaveWaterfall(mercuryWaterfall);
                LatestWaterfallId = mercuryWaterfall.id;
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Mercury response: {info.data}");
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Using waterfall from Mercury: Assets: {mercuryWaterfall.GetPromotedAssets()}");
                
                _wasDownloaded = AdLoadingState.Loading;
                MercuryDownloader.DownloadWaterfall(mercuryWaterfall, AddAssetToWaterfall, true);   
            }

            BackupAdsAnalytics.TriggerMercuryInitFinished(info);
        }

        private void OnGameInfoError(MercuryRequestInfo info)
        {
            LoadWaterfallFromCache();
            string waterfallTxt = string.Join(", ", currentAssets.Select(x => x.game.name));
            VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, $"Init failed, waterfall from cache: {waterfallTxt}");
        }

#endregion
        
#region Utils

        private int WaterfallSort(MercuryPromotedAsset x, MercuryPromotedAsset y) => x.videoIndex.CompareTo(y.videoIndex);

        internal bool IsRestrictedPrivacy()
        {
            if (_gdprConsent == null) {
                return false;
            }
            
            return !_gdprConsent.ExplicitConsentGivenForAds && _gdprConsent.IsAdsEnforcement;
        }

        internal bool IsEnabled()
        {
            if (IsRestrictedPrivacy()) {
                return true;
            }
            
            return _currentConfig?.enabled ?? false;
        }

        internal bool ShouldShowWithoutInternet()
        { 
            if (IsRestrictedPrivacy()) {
                return true;
            }

            return _currentConfig?.enabledIfNoConnection ?? false;
        }

        internal AdLoadingState IsBackupAdAvailable()
        {
            if (!_initialized) {
                return AdLoadingState.NotInitialized;
            }

            if (!IsEnabled()) {
                return AdLoadingState.Disabled;
            }

            if (currentAssets != null && currentAssets.Count > 0) {
                return AdLoadingState.Loaded;
            }

            return _wasDownloaded;
        }
        
#endregion

#region Interstitial Lifecycle

        internal void ShowCrossPromoInterstitial(Action onComplete)
        {
            if (CanShowBackupInterstitial()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG,
                    $"ShowCrossPromoInterstitial: Enabled:{IsEnabled()}" + $"ShowNoConnection: {ShouldShowWithoutInternet()}");

                currentAdInfo = GetNextInterstitial();
                if (currentAdInfo != null) {
                    if (_canvasInstance == null)
                        _canvasInstance = Object.Instantiate(BackupAdsSettings.Settings.adsPrefab);
                
                    _wasBannerShown = AdsManager.Banner.IsShowing;
                    if (_wasBannerShown) {
                        VoodooSauce.HideBanner();
                    }

                    _completeCallback = reward => onComplete?.Invoke();
                    _canvasInstance.PlayAd(
                        currentAdInfo, DELAY_BEFORE_SHOWING_CLOSE_BUTTON_INTERSTITIAL,
                        OnInterstitialComplete,
                        OnInterstitialClose,
                        OnInterstitialClicked
                    );

                    currentAdInfo.TriggerAnalyticsImpression();
                    currentAdInfo.TriggerAdjustImpression();
                    return;
                }
            }

            onComplete?.Invoke();
        }

        private BackupAdsInfo GetNextInterstitial() => GetNextAsset(BackupAdsInfo.BackupAdType.BackupFS);

        public bool CanShowBackupInterstitial()
        {
            if (!_initialized || _currentConfig == null) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Can't display backup interstitial because BackupAds Manager is not initialized.");
                return false;
            }

            if (!IsEnabled()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Backup Ads not enabled");
                return false;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable && !ShouldShowWithoutInternet()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "Backup Ads Should Show Without Internet is not enable");
                return false;
            }
            
            if (currentAdInfo != null) {
                _wasDownloaded = AdLoadingState.Failed;
                VoodooLog.LogDebug(Module.ADS, TAG, $"Backup Ad is already playing. {currentAdInfo}");
                return false;
            }

            if (currentAssets == null || currentAssets.Count == 0) {
                VoodooLog.LogDebug(Module.ADS, TAG, $"No ads found.");
                return false;
            }

            return true;
        }
        
#endregion

#region Interstitial Event

        private void OnInterstitialClose()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Backup Interstitial closed.");
            currentAdInfo = null;
            if (_wasBannerShown) {
                VoodooSauce.ShowBanner();
            }
        }

        private void OnInterstitialComplete(bool reward)
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Backup Interstitial completed.");
            AdsManager.TriggerInterstitialAdConditionsDisplay();
            _completeCallback?.Invoke(false);
            _completeCallback = null;
        }

        private void OnInterstitialClicked()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Backup Interstitial clicked.");

            currentAdInfo?.TriggerAnalyticsClick();
            currentAdInfo?.TriggerAdjustClick();
            currentAdInfo?.TriggerAdjustRedirection();
        }

#endregion

#region RewardedVideo Lifecycle

        internal void ShowCrossPromoRewardedVideo(Action<bool> onComplete)
        {
            if (CanShowBackupRewardedVideo()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG,
                    $"ShowCrossPromoRewardedVideo: Enabled:{IsEnabled()}" + $"ShowNoConnection: {ShouldShowWithoutInternet()}");

                currentAdInfo = GetNextRewardedVideo();
                if (currentAdInfo != null) {
                    if (_canvasInstance == null)
                        _canvasInstance = Object.Instantiate(BackupAdsSettings.Settings.adsPrefab);
                
                    _wasBannerShown = AdsManager.Banner.IsShowing;
                    if (_wasBannerShown) {
                        VoodooSauce.HideBanner();
                    }

                    _completeCallback = onComplete;
                    _canvasInstance.PlayAd(
                        currentAdInfo, DELAY_BEFORE_SHOWING_CLOSE_BUTTON_REWARDED,
                        OnRewardedVideoComplete,
                        OnRewardedVideoClose,
                        OnRewardedVideoClicked
                    );

                    currentAdInfo.TriggerAnalyticsImpression();
                    currentAdInfo.TriggerAdjustImpression();
                    return;
                }
            }

            onComplete?.Invoke(false);
        }

        private BackupAdsInfo GetNextRewardedVideo() => GetNextAsset(BackupAdsInfo.BackupAdType.BackupRV);

        public bool CanShowBackupRewardedVideo()
        {
            // Same conditions as Interstitial for now
            return CanShowBackupInterstitial();
        }

#endregion
        
#region Rewarded Video Event

        private void OnRewardedVideoClose()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Backup RewardedVideo closed.");
            currentAdInfo = null;
            if (_wasBannerShown) {
                VoodooSauce.ShowBanner();
            }
            AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
        }

        private void OnRewardedVideoComplete(bool reward)
        {
            VoodooLog.LogDebug(Module.ADS, TAG, $"Backup RewardedVideo completed: {reward}");
            _completeCallback?.Invoke(reward);
            _completeCallback = null;
        }

        private void OnRewardedVideoClicked()
        {
            VoodooLog.LogDebug(Module.ADS, TAG, "Backup RewardedVideo clicked.");

            currentAdInfo?.TriggerAnalyticsClick();
            currentAdInfo?.TriggerAdjustClick();
            currentAdInfo?.TriggerAdjustRedirection();
        }

#endregion

#region Assets

        private void AddAssetToWaterfall(MercuryPromotedAsset asset)
        {
            if (asset == null) {
                VoodooLog.LogError(Module.CROSS_PROMO, TAG, "Promoted asset object null");
                return;
            }
            
            currentAssets.Add(asset);
            _wasDownloaded = AdLoadingState.Loaded;
        }

        private void LoadWaterfallFromCache()
        {
            _wasDownloaded = AdLoadingState.Loading;
            mercuryWaterfall = MercuryCache.GetWaterfall(LatestWaterfallId);
            
            if (mercuryWaterfall != null) {
                foreach (MercuryPromotedAsset asset in mercuryWaterfall.promote_assets) {
                    AddAssetToWaterfall(MercuryCache.GetAsset(asset.id));
                }
            }

            if (currentAssets.Count == 0) {
                VoodooLog.LogWarning(Module.ADS, TAG, "No video available in waterfall.");
                _wasDownloaded = AdLoadingState.Disabled;
            } else {
                _wasDownloaded = AdLoadingState.Loaded;
            }
            AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
        }

        private BackupAdsInfo GetNextAsset(BackupAdsInfo.BackupAdType adType)
        {
            if (currentAssets.Count <= 0) {
                LoadWaterfallFromCache();
            }

            if (currentAssets.Count > 0) {
                var index = 0;
                currentAssets.Sort(WaterfallSort);

                if (_currentConfig.randomizeWaterfall) index = Random.Range(0, currentAssets.Count);

                BackupAdsInfo nextAds = new BackupAdsInfo(currentAssets[index], adType, IsRestrictedPrivacy());
                lastDisplayedAdInfo = nextAds;
                currentAssets.RemoveAt(index);
                
                if (currentAssets.Count <= 0) {
                    LoadWaterfallFromCache();
                }
                
                return nextAds;
            }

            VoodooLog.LogWarning(Module.ADS, TAG, "No video available in waterfall.");
            return null;
        }

#endregion
    }
}