using System;
using Voodoo.Sauce.Internal.Analytics;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public enum RewardedReplaceState
    {
        DoNotReplaceRewarded,
        InterstitialCpmHigher,
        RewardedNotLoaded
    }

    public class RewardedInterstitialVideo : BaseAd
    {
        private const string TAG = "AdsManager.RewardedInterstitialVideo";

        internal event Action OnShow;
        private Action<bool> _callback;
        private RewardedReplaceState _currentShowingRewardReplaceState;

        public RewardedInterstitialVideo(IMediationAdapter adapter) : base(adapter) { }

        internal override void Initialize() => MediationAdapter.InitializeInterstitial();

        /// <summary>
        /// The regular method to call in order to display an interstitial
        /// </summary>
        /// <param name="onComplete">Callback to be called once the interstitial is closed, true if reward granted in RV
        /// and false if otherwise</param>
        /// <param name="rewardedReplaceState">The state/reason why this Interstitial are shown instead of Rewarded</param>
        /// <param name="tag">A tag the game developer can pass to distinguish between the kind of interstitial
        /// that could be displayed in his game</param>
        internal void Show(Action<bool> onComplete, RewardedReplaceState rewardedReplaceState, string tag = null)
        {
            _currentShowingRewardReplaceState = rewardedReplaceState;
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            VoodooAdInfo rewardedVideoAdInfo = MediationAdapter.GetRewardedVideoInfo();

            string adNetworkName = adInfo.AdNetworkName;
            string waterfallTestName = adInfo.WaterfallTestName;
            var adLoadingTime = (int)LoadingTime.TotalMilliseconds;

            VoodooLog.LogDebug(Module.ADS, TAG, "Displaying a rewarded interstitial ad ...");

            var adTriggeredEventAnalyticsInfo = new AdTriggeredEventAnalyticsInfo {
                AdTag = tag,
                AdLoaded = true,
                AdCount = IncrementAdCount(),
                AdNetworkName = adNetworkName,
                WaterfallTestName = waterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = adLoadingTime,
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
                InterstitialCpm = adInfo.Revenue,
                RewardedVideoCpm = rewardedVideoAdInfo.Revenue                    
            };

            AddRvReplaceStateToAnalyticsInfo(adTriggeredEventAnalyticsInfo);
            AnalyticsManager.TrackInterstitialTriggered(adTriggeredEventAnalyticsInfo);

            var adShownEventAnalyticsInfo = new AdShownEventAnalyticsInfo {
                AdTag = tag,
                AdNetworkName = adNetworkName,
                WaterfallTestName = waterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = adLoadingTime,
                AdCount = AdCount(),
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
            };
            AddRvReplaceStateToAnalyticsInfo(adShownEventAnalyticsInfo);
            AnalyticsManager.TrackShowInterstitial(adShownEventAnalyticsInfo);

            Type = tag;
            _callback = onComplete;
            StartShowing();
            OnShow?.Invoke();
            MediationAdapter.ShowInterstitial(true);
        }

        public override void Enable()
        {
            base.Enable();
            MediationAdapter.InitializeInterstitial();
        }
        
        private static int AdCount() => AnalyticsStorageHelper.Instance.GetShowInterstitialCount();
        
        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementShowInterstitialCount();
        
        private static int IncrementWatchedAdCount() => AnalyticsStorageHelper.Instance.IncrementInterstitialWatchedCount();
        
        private void TriggerCallback(bool isAdsDisplayed)
        {
            if (_callback == null) {
                return;
            }
            _callback.Invoke(isAdsDisplayed);
            _callback = null;
        }

        public RewardedReplaceState GetRewardedReplaceState()
        {
            if (!IsAvailable())
                return RewardedReplaceState.DoNotReplaceRewarded;
            
            if (AdsManager.EnableReplaceRewardedIfNotLoaded && AdsManager.RewardedVideo.State != AdLoadingState.Loaded) {
                return RewardedReplaceState.RewardedNotLoaded;
            }

            if (!AdsManager.EnableReplaceRewardedOnCpm) 
                return RewardedReplaceState.DoNotReplaceRewarded;
            
            double interstitialCpm = MediationAdapter?.GetInterstitialInfo().Revenue ?? 0;
            double rewardedCpm = MediationAdapter?.GetRewardedVideoInfo().Revenue ?? 0;
            return interstitialCpm > rewardedCpm ? RewardedReplaceState.InterstitialCpmHigher : RewardedReplaceState.DoNotReplaceRewarded;
        }

        private void AddRvReplaceStateToAnalyticsInfo(AdAnalyticsInfo analyticsInfo)
        {
            bool rvReplaced = _currentShowingRewardReplaceState != RewardedReplaceState.DoNotReplaceRewarded;
            if (!rvReplaced) return;

            analyticsInfo.IsFsShownInsteadOfRv = true;
            switch (_currentShowingRewardReplaceState) {
                case RewardedReplaceState.InterstitialCpmHigher:
                    analyticsInfo.ReasonFsShownInsteadOfRv = "fs_higher_cpm";
                    break;
                case RewardedReplaceState.RewardedNotLoaded:
                    analyticsInfo.ReasonFsShownInsteadOfRv = "rv_not_loaded";
                    break;
            }
        }

#region Ad callbacks
        
        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Stop();
            AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
        }

        public override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            LoadingTime.Stop();
            AdsManager.RewardedVideo.AvailabilityUpdateFromRewardedInterstitial();
        }

        public void OnFailedToShow(VoodooErrorAdInfo errorAdInfo)
        {
            TriggerCallback(false);
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();

            var showFailedAnalyticsInfo = new AdShowFailedEventAnalyticsInfo {
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
            };

            AddRvReplaceStateToAnalyticsInfo(showFailedAnalyticsInfo);
            AnalyticsManager.TrackInterstitialShowFailed(showFailedAnalyticsInfo);
        }
        
        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            var analyticsInfo = new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = (int) AdsManager.Interstitial.LoadingTime.TotalMilliseconds,
                AdCount = AnalyticsStorageHelper.Instance.GetShowInterstitialCount(),
                Creative = adInfo.Creative,
                ImpressionId = UuidBeingShown,
            };

            AddRvReplaceStateToAnalyticsInfo(analyticsInfo);
            AnalyticsManager.TrackInterstitialClick(analyticsInfo);
        }

        public void OnDismissed()
        {
            TriggerCallback(true);
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            var analyticsInfo = new AdClosedEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdCount = AdCount(),
                AdWatchedCount = IncrementWatchedAdCount(),
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = (int) AdsManager.Interstitial.LoadingTime.TotalMilliseconds,
                Creative = adInfo.Creative,
                ImpressionId = UuidBeingShown,
            };

            AddRvReplaceStateToAnalyticsInfo(analyticsInfo);
            AnalyticsManager.TrackInterstitialDismiss(analyticsInfo);
            
            StopShowing();
        }

        public void OnImpression(ImpressionInfoType type)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetInterstitialInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.Interstitial;
            info.AdCount = AdCount();
            info.AdTag = Type;
            info.AdLoadingTime = (int)AdsManager.Interstitial.LoadingTime.TotalMilliseconds;
            info.ImpressionId = UuidBeingShown;
            AddRvReplaceStateToAnalyticsInfo(info);
            AnalyticsManager.TrackImpression(info);
        }

#endregion
    }
}