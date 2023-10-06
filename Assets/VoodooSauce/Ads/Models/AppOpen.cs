using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts;
using Voodoo.Sauce.Internal.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class AppOpen : BaseAd
    {
        private const string TAG = "AdsManager.AppOpen";

        private Action _callback;

        public enum AdType
        {
            soft_launch
        }

        private enum InternalAdState
        {
            CAN_NOT_SHOW,
            CAN_SHOW,
            NOT_AVAILABLE,
            NOT_LOADED
        }

        internal AppOpen(IMediationAdapter adapter) : base(adapter) { }
        
        internal override void Initialize() => MediationAdapter.InitializeAppOpen();

        private InternalAdState CanShow(bool ignoreConditions = false)
        {
            if (VoodooPremium.IsPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User is premium, not displaying appOpen.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User has an active subscription, ads disabled.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (State == AdLoadingState.Disabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen has been disabled.");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen is not initialized yet");
                return InternalAdState.NOT_AVAILABLE;
            }

            if (!ignoreConditions && !AdDisplayConditions.AreAppOpenConditionsMet()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen ad display conditions not met.");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (!IsAvailable()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "AppOpen ad is not loaded yet.");
                return InternalAdState.NOT_LOADED;
            }

            return InternalAdState.CAN_SHOW;
        }

        /// <summary>
        /// The regular method to call in order to display an appOpen
        /// </summary>
        /// <param name="onComplete">Callback to be called once the appOpen is closed</param>
        /// <param name="ignoreConditions">Indicates whether the appOpen display condition should be ignored</param>
        /// <param name="tag">A tag the game developer can pass to distinguish between the kind of appOpen
        /// that could be displayed in his game</param>
        internal void Show(Action onComplete = null, bool ignoreConditions = false)
        {
            InternalAdState showCheckResult = CanShow(ignoreConditions);
            VoodooAdInfo adInfo = MediationAdapter.GetAppOpenInfo();

            string adNetworkName = adInfo.AdNetworkName;
            string waterfallTestName = adInfo.WaterfallTestName;

            switch (showCheckResult) {
                case InternalAdState.CAN_SHOW: {
                    VoodooLog.LogDebug(Module.ADS, TAG, "Displaying an appOpen ad ...");
                    
                    AnalyticsManager.TrackAppOpenTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdLoaded = true,
                        AdCount = IncrementAdCount(),
                        AdNetworkName = adNetworkName,
                        WaterfallTestName = waterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid
                    });

                    if (onComplete != null)
                        onComplete = AdDisplayConditions.AppOpenDisplayed + onComplete;
                    else
                        onComplete = AdDisplayConditions.AppOpenDisplayed;

                    AnalyticsManager.TrackShowAppOpen(new AdShownEventAnalyticsInfo {
                        AdNetworkName = adNetworkName,
                        WaterfallTestName = waterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        AdCount = AdCount(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                    });

                    _callback = onComplete;
                    StartShowing();
                    MediationAdapter.ShowAppOpen();
                    break;
                }
                case InternalAdState.NOT_LOADED:
                    AnalyticsManager.TrackAppOpenTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdLoaded = false,
                        AdNetworkName = adNetworkName,
                        AdLoadingTime = LoadingTimeMilliseconds(),
                        AdCount = AdCount(),
                        Creative = adInfo.Creative,
                        ImpressionId = Uuid,
                        InterstitialCpm = adInfo.Revenue                
                    });
                    onComplete?.Invoke();
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
            MediationAdapter.InitializeAppOpen();
        }

        protected override void StartShowing()
        {
            base.StartShowing();
            AdDisplayConditions.ResetBackgroundTimer();
        }

        private static int AdCount() => AnalyticsStorageHelper.Instance.GetShowAppOpenCount();

        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementShowAppOpenCount();
        
        private  static int IncrementWatchedAdCount() => AnalyticsStorageHelper.Instance.IncrementAppOpenWatchedCount();

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
            AnalyticsManager.TrackAppOpenLoadRequest(new AdAnalyticsInfo {ImpressionId = Uuid});
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
            VoodooAdInfo adInfo = MediationAdapter.GetAppOpenInfo();
            AnalyticsManager.TrackAppOpenClick(new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
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
            VoodooAdInfo adInfo = MediationAdapter.GetAppOpenInfo();
            AnalyticsManager.TrackAppOpenShowFailed(new AdShowFailedEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
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
        }

        public void OnDismissed()
        {
            TriggerCallback();
            VoodooAdInfo adInfo = MediationAdapter.GetAppOpenInfo();
            AnalyticsManager.TrackAppOpenDismiss(new AdClosedEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
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
            VoodooAdInfo adInfo = MediationAdapter.GetAppOpenInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.AppOpen;
            info.AdCount = AdCount();
            info.AppVersion = AppVersion;
            info.AdLoadingTime = LoadingTimeMilliseconds();
            info.ImpressionId = UuidBeingShown;
            AnalyticsManager.TrackImpression(info);
        }
        
#endregion
    }
}