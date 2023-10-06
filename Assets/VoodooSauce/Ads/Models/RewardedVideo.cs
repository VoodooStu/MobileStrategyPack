using System;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public class RewardedVideo : BaseAd
    {
        private const string TAG = "AdsManager.RewardedVideo";

        // Fired when a rewarded video is loaded. Includes a boolean indicating if the load succeeded or not.
        internal event Action<bool> OnRewardedVideoAvailabilityChangeEvent;
        internal event Action OnShow;
        private Action<bool> _callback;

        private enum InternalAdState
        {
            GRANT_REWARDS,
            SHOW_INTERSTITIAL,
            NOT_AVAILABLE,
            CAN_SHOW
        }

        internal RewardedVideo(IMediationAdapter adapter) : base(adapter) { }

        internal override void Initialize() => MediationAdapter.InitializeRewardedVideo();

        internal new bool IsAvailable() => VoodooSuperPremium.IsSuperPremium() || 
            State == AdLoadingState.Loaded ||
            AdsManager.RewardedInterstitialVideo.GetRewardedReplaceState() != RewardedReplaceState.DoNotReplaceRewarded ||
            BackupAdsManager.Instance.CanShowBackupRewardedVideo();

        private InternalAdState CanShow()
        {
            if (VoodooSuperPremium.IsSuperPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Super Premium mode is activated. Ads are not shown but rewards are granted.");
                return InternalAdState.GRANT_REWARDS;
            }

            if (State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is not initialized yet");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (AdsManager.RewardedInterstitialVideo.GetRewardedReplaceState() != RewardedReplaceState.DoNotReplaceRewarded) {
                RewardedReplaceState state = AdsManager.RewardedInterstitialVideo.GetRewardedReplaceState();
                VoodooLog.LogDebug(Module.ADS, TAG, "Displaying Interstitial instead because: "+state);
                return InternalAdState.SHOW_INTERSTITIAL;
            }
            
            if (State == AdLoadingState.Initialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is initialized but there is no ad loaded.");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (State == AdLoadingState.Loading)
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is loading.");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (State == AdLoadingState.Failed)
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video loading failed.");
                return InternalAdState.NOT_AVAILABLE;
            }
            
            if (State == AdLoadingState.Disabled)
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is disabled.");
                return InternalAdState.NOT_AVAILABLE;
            }
            
            if (State == AdLoadingState.Loaded)
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is ready to show.");
                return InternalAdState.CAN_SHOW;
            }

            VoodooLog.LogDebug(Module.ADS, TAG, "Rewarded video is not available.");
            return InternalAdState.NOT_AVAILABLE;
        }

        /// <summary>
        /// The regular method to call in order to display a rewarded
        /// </summary>
        /// <param name="onComplete">Callback to be called once the rewarded video is closed</param>
        /// <param name="tag">A tag the game developer can pass to distinguish between the kind of rewarded videos
        /// that could be displayed in his game</param>
        internal void Show(Action<bool> onComplete, string tag = null)
        {
            Type = tag;
            _callback = onComplete;

            InternalAdState rvState = CanShow();
            switch (rvState) {
                case InternalAdState.CAN_SHOW:
                    VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
                    AnalyticsManager.TrackShowRewardedVideo(new AdShownEventAnalyticsInfo {
                        AdTag = tag,
                        AdNetworkName = adInfo.AdNetworkName,
                        WaterfallTestName = adInfo.WaterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        AdLoadingTime = (int) LoadingTime.TotalMilliseconds,
                        AdCount = IncrementAdCount(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                    });

                    StartShowing();
                    OnShow?.Invoke();
                    MediationAdapter.ShowRewardedVideo();
                    break;
                case InternalAdState.NOT_AVAILABLE:
                    BackupAdsManager.Instance.ShowCrossPromoRewardedVideo(TriggerCallback);
                    break;
                case InternalAdState.SHOW_INTERSTITIAL:
                    RewardedReplaceState state = AdsManager.RewardedInterstitialVideo.GetRewardedReplaceState();
                    AdsManager.RewardedInterstitialVideo.Show(onComplete, state, tag);
                    break;
                case InternalAdState.GRANT_REWARDS:
                    TriggerCallback(true);
                    break;
                default:
                    TriggerCallback(false);
                    break;
            }
        }
        
        private static int AdCount() => AnalyticsStorageHelper.Instance.GetShowRewardedVideoCount();
        
        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementShowRewardedVideoCount();
        
        private static int WatchedAdCount() => AnalyticsStorageHelper.Instance.GetRewardedVideoWatchedCount();
        
        private static int IncrementWatchedAdCount() => AnalyticsStorageHelper.Instance.IncrementRewardedVideoWatchedCount();

        private void TriggerCallback(bool watched)
        {
            AdDisplayConditions.RewardedVideoDisplayed(watched);

            if (_callback == null) {
                return;
            }
            _callback.Invoke(watched);
            _callback = null;
        }

        internal void AvailabilityUpdateFromRewardedInterstitial() => OnRewardedVideoAvailabilityChangeEvent?.Invoke(IsAvailable());
        
#region Ad callbacks

        public override void OnLoading()
        {
            base.OnLoading();
            AnalyticsManager.TrackRewardedVideoLoadRequest(new AdAnalyticsInfo {ImpressionId = Uuid});
        }
        
        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Stop();
            OnRewardedVideoAvailabilityChangeEvent?.Invoke(false);
        }

        public override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            LoadingTime.Stop();
            OnRewardedVideoAvailabilityChangeEvent?.Invoke(true);
        }

        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
            AnalyticsManager.TrackRewardedVideoClick(new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdCount = AdCount(),
                AdNetworkName = adInfo.AdNetworkName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                Creative = adInfo.Creative,
                ImpressionId = UuidBeingShown,
            });
        }

        public void OnFailedToShow(VoodooErrorAdInfo errorAdInfo)
        {
            TriggerCallback(false);
            VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
            AnalyticsManager.TrackRewardedVideoShowFailed(new AdShowFailedEventAnalyticsInfo {
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

        public void OnDismissed(bool rewardedReceived)
        {
            TriggerCallback(rewardedReceived);
            int adWatchedCount = rewardedReceived ? IncrementWatchedAdCount() : WatchedAdCount();

            VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
            AnalyticsManager.TrackRewardedVideoClose(new AdClosedEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
                    AdTag = Type,
                    AdReward = rewardedReceived,
                    AdCount = AdCount(),
                    AdWatchedCount = adWatchedCount,
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
            VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.RewardedVideo;
            info.AdCount = AdCount();
            info.AdTag = Type;
            info.AdLoadingTime = LoadingTimeMilliseconds();
            info.AppVersion = AppVersion;
            info.ImpressionId = UuidBeingShown;
            AnalyticsManager.TrackImpression(info);
        }

        internal void OnButtonShown(string rewardedType)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetRewardedVideoInfo();
            VoodooAdInfo interstitialAdInfo = MediationAdapter.GetInterstitialInfo();
            AnalyticsManager.TrackRewardedVideoButtonShown(new RewardButtonShownEventAnalyticsInfo {
                AdTag = rewardedType,
                AdLoadingTime = (int) LoadingTime.TotalMilliseconds,
                AdLoaded = IsAvailable(),
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdCount = AdCount(),
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
                InterstitialCpm = interstitialAdInfo.Revenue,
                RewardedVideoCpm = adInfo.Revenue                    
            });
        }
        
#endregion
    }
}