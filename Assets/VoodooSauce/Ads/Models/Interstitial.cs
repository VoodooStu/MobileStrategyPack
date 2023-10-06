using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts;
using Voodoo.Sauce.Internal.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class Interstitial : BaseAd
    {
        internal event Action OnShow;
        
        private const string TAG = "AdsManager.Interstitial";

        private Action _callback;

        private enum InternalAdState
        {
            CAN_NOT_SHOW,
            CAN_SHOW,
            NOT_AVAILABLE
        }

        internal Interstitial(IMediationAdapter adapter) : base(adapter) { }
        
        internal override void Initialize() => MediationAdapter.InitializeInterstitial();
        
        internal new bool IsAvailable() => base.IsAvailable() || BackupAdsManager.Instance.CanShowBackupInterstitial();

        private InternalAdState CanShow(bool ignoreConditions = false)
        {
            if (VoodooPremium.IsPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User is premium, not displaying interstitial.");
                return InternalAdState.CAN_NOT_SHOW;
            }
            
            if (State == AdLoadingState.Disabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Interstitial has been disabled.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User has an active subscription, ads disabled.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Interstitial is not initialized yet");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (!ignoreConditions && !AdDisplayConditions.AreInterstitialConditionsMet()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Interstitial ad display conditions not met.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (!IsAvailable()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Interstitial ad is not loaded yet.");
                return InternalAdState.NOT_AVAILABLE;
            }

            return InternalAdState.CAN_SHOW;
        }

        /// <summary>
        /// The regular method to call in order to display an interstitial
        /// </summary>
        /// <param name="onComplete">Callback to be called once the interstitial is closed</param>
        /// <param name="ignoreConditions">Indicates whether the interstitial display condition should be ignored</param>
        /// <param name="tag">A tag the game developer can pass to distinguish between the kind of interstitial
        /// that could be displayed in his game</param>
        internal void Show(Action onComplete, bool ignoreConditions = false, string tag = null)
        {
            InternalAdState showFsCheckResult = CanShow(ignoreConditions);
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();

            string adNetworkName = adInfo.AdNetworkName;
            string waterfallTestName = adInfo.WaterfallTestName;

            VoodooAdInfo rewardedVideoAdInfo;

            switch (showFsCheckResult) {
                case InternalAdState.CAN_SHOW: {
                    VoodooLog.LogDebug(Module.ADS, TAG, "Displaying an interstitial ad ...");

                    rewardedVideoAdInfo = MediationAdapter.GetRewardedVideoInfo();
                    
                    AnalyticsManager.TrackInterstitialTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdTag = tag,
                        AdLoaded = true,
                        AdCount = IncrementAdCount(),
                        AdNetworkName = adNetworkName,
                        WaterfallTestName = waterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                        InterstitialCpm = adInfo.Revenue,
                        RewardedVideoCpm = rewardedVideoAdInfo.Revenue
                    });

                    if (onComplete != null)
                        onComplete = AdDisplayConditions.InterstitialDisplayed + onComplete;
                    else
                        onComplete = AdDisplayConditions.InterstitialDisplayed;

                    AnalyticsManager.TrackShowInterstitial(new AdShownEventAnalyticsInfo {
                        AdTag = tag,
                        AdNetworkName = adNetworkName,
                        WaterfallTestName = waterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        AdCount = AdCount(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                    });

                    Type = tag;
                    _callback = onComplete;
                    StartShowing();
                    OnShow?.Invoke();
                    MediationAdapter.ShowInterstitial();
                    break;
                }
                case InternalAdState.NOT_AVAILABLE:
                    
                    rewardedVideoAdInfo = MediationAdapter.GetRewardedVideoInfo();
                    
                    AnalyticsManager.TrackInterstitialTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdTag = tag,
                        AdLoaded = false,
                        AdNetworkName = adNetworkName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        AdCount = AdCount(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                        InterstitialCpm = adInfo.Revenue,
                        RewardedVideoCpm = rewardedVideoAdInfo.Revenue                    
                    });
                    if (ignoreConditions || (AdDisplayConditions != null && AdDisplayConditions.AreInterstitialConditionsMet()))
                    {
                        BackupAdsManager.Instance.ShowCrossPromoInterstitial(onComplete);
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }
                    break;
                default:
                    onComplete?.Invoke();
                    break;
            }
        }

        public override void Enable()
        {
            base.Enable();
            if (!MediationAdapter.IsSdkInitialized()) {
                return;
            }
            MediationAdapter.InitializeInterstitial();
        }

        private static int AdCount() => AnalyticsStorageHelper.Instance.GetShowInterstitialCount();
        
        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementShowInterstitialCount();
        
        private  static int IncrementWatchedAdCount() => AnalyticsStorageHelper.Instance.IncrementInterstitialWatchedCount();

        private void TriggerCallback()
        {
            if (_callback == null) {
                return;
            }
            _callback.Invoke();
            _callback = null;
        }
        
#region Ad callbacks

        public override void OnLoading()
        {
            base.OnLoading();
            AnalyticsManager.TrackInterstitialLoadRequest(new AdAnalyticsInfo {ImpressionId = Uuid});
        }
        
        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Stop();
        }

        public override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            LoadingTime.Stop();
        }

        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            AnalyticsManager.TrackInterstitialClick(new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                AdCount = AdCount(),
                Creative = adInfo.Creative,
                ImpressionId = UuidBeingShown,
            });
        }

        public void OnFailedToShow(VoodooErrorAdInfo errorAdInfo)
        {
            TriggerCallback();
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            AnalyticsManager.TrackInterstitialShowFailed(new AdShowFailedEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdCount = AdCount(),
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                Creative = adInfo.Creative,
                ErrorCode = errorAdInfo.ErrorCode,
                ErrorMessage = errorAdInfo.ErrorMessage,
                AdNetworkErrorCode = errorAdInfo.AdNetworkErrorCode,
                AdNetworkErrorString = errorAdInfo.AdNetworkErrorMessage,
                ImpressionId = Uuid,
            });
            
            VoodooSauce.SetCrashReportingCustomData(VoodooAnalyticsConstants.ERROR_MESSAGE, errorAdInfo.ErrorMessage);
            VoodooSauce.SetCrashReportingCustomData(VoodooAnalyticsConstants.AD_NETWORK_ERROR_MESSAGE, errorAdInfo.AdNetworkErrorMessage);
        }

        public void OnDismissed()
        {
            TriggerCallback();
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            AnalyticsManager.TrackInterstitialDismiss(new AdClosedEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
                    AdTag = Type,
                    AdCount = AdCount(),
                    AdWatchedCount = IncrementWatchedAdCount(),
                    AdNetworkName = adInfo.AdNetworkName,
                    WaterfallTestName = adInfo.WaterfallTestName,
                    WaterfallName = adInfo.WaterfallName,
                    AdLoadingTime = LoadingTimeMilliseconds(),
                    Creative = adInfo.Creative,
                    ImpressionId = UuidBeingShown,
                }
            );
            
            StopShowing();
        }

        public void OnImpression(ImpressionInfoType type)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.Interstitial;
            info.AdCount = AdCount();
            info.AdTag = Type;
            info.AppVersion = AppVersion;
            info.AdLoadingTime = LoadingTimeMilliseconds();
            info.ImpressionId = UuidBeingShown;
            AnalyticsManager.TrackImpression(info);
        }
        
#endregion
    }
}