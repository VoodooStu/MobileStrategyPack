using System;
using UnityEngine;
using Voodoo.Sauce.Ads;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IAP;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class NativeAds : BaseAd
    {
        private const string TAG = "AdsManager.NativeAd";

        private Guid? _currentNativeAd;
        private Action _autoShowAfterLoadAction;
        private Action _callback;

        private enum InternalAdState
        {
            NOT_INITIALIZED_YET,
            AD_NOT_LOADED_YET,
            CAN_NOT_SHOW,
            CAN_SHOW
        }

        public NativeAds(IMediationAdapter adapter) : base(adapter) { }
        
        internal override void Initialize() => MediationAdapter.InitializeNativeAds();

        private InternalAdState CanShow()
        {
            if (State == AdLoadingState.Disabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, "NativeAds has been disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooPremium.IsPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User is premium, not displaying NativeAds");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User has an active subscription, ads disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "NativeAds is not initialized yet");
                return InternalAdState.NOT_INITIALIZED_YET;
            }

            if (!IsAvailable()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "NativeAds is not loaded yet");
                return InternalAdState.AD_NOT_LOADED_YET;
            }

            return InternalAdState.CAN_SHOW;
        }

        internal void Show(NativeAdLayout layout, Rect adScreenBounds, string tag, Guid id)
        {
            if (_currentNativeAd != null && _currentNativeAd != id) {
                // Only one native ad can be displayed at a time.
                Hide(_currentNativeAd.Value);
            }

            _currentNativeAd = id;
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            var adLoadingTime = (int) LoadingTime.TotalMilliseconds;

            InternalAdState showNativeAdCheckResult = CanShow();
            switch (showNativeAdCheckResult) {
                case InternalAdState.NOT_INITIALIZED_YET:
                case InternalAdState.CAN_NOT_SHOW:
                    break;
                case InternalAdState.AD_NOT_LOADED_YET:
                    AnalyticsManager.TrackNativeAdTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdTag = tag,
                        AdLoaded = false,
                        AdCount = AdCount(),
                        ImpressionId = Uuid,
                    });

                    _autoShowAfterLoadAction = () => { Show(layout, adScreenBounds, tag, id); };
                    break ;
                case InternalAdState.CAN_SHOW:
                    AnalyticsManager.TrackNativeAdTriggered(new AdTriggeredEventAnalyticsInfo {
                        AdTag = tag,
                        AdLoaded = true,
                        AdCount = IncrementAdCount(),
                        AdNetworkName = adInfo.AdNetworkName,
                        AdLoadingTime = adLoadingTime,
                        Creative = adInfo.Creative,
                        WaterfallTestName = adInfo.WaterfallTestName,
                        WaterfallName = adInfo.WaterfallName,
                        ImpressionId = Uuid,
                    });
                    VoodooLog.LogDebug(Module.ADS, TAG, "Showing NativeAd ...");
                    Type = tag;
                    _autoShowAfterLoadAction = null;
                    MediationAdapter.ShowNativeAd(layout.GetKey(), adScreenBounds.x, adScreenBounds.y, adScreenBounds.width, adScreenBounds.height);
                    break;
            }
        }

        internal void Hide(Guid? id = null)
        {
            _autoShowAfterLoadAction = null;
            switch (State) {
                case AdLoadingState.Disabled:
                    VoodooLog.LogDebug(Module.ADS, TAG, "NativeAds has been disabled");
                    break;
                case AdLoadingState.NotInitialized:
                    VoodooLog.LogDebug(Module.ADS, TAG, "NativeAds is not initialized yet");
                    break;
                default: {
                    // If the parameter id is null, any native ad is closed.
                    // Otherwise, if the parameter id is specified, the current native ad is closed only if it has the same id.
                    if (id == null && _currentNativeAd != null || id != null && id == _currentNativeAd) {
                        _currentNativeAd = null;
                        VoodooLog.LogDebug(Module.ADS, TAG, "Hiding NativeAd ...");
                        MediationAdapter.HideNativeAd();
                    }

                    break;
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            if (!MediationAdapter.IsSdkInitialized()) {
                return;
            }
            MediationAdapter.InitializeNativeAds();
        }

        public override void Disable()
        {
            base.Disable();
            _autoShowAfterLoadAction = null;
            MediationAdapter.HideNativeAd();
        }

        private static int AdCount() => AnalyticsStorageHelper.Instance.GetShowNativeAdsCount();

        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementShowNativeAdsCount();

        private void TriggerCallback()
        {
            if (_callback == null) {
                return;
            }
            _callback.Invoke();
            _callback = null;
        }

        private void TriggerAutoShowActionIfNeeded()
        {
            if (_autoShowAfterLoadAction == null) {
                return;
            }
            UnityThreadExecutor.Execute(() => {
                _autoShowAfterLoadAction?.Invoke();
                _autoShowAfterLoadAction = null;
            });
        }
        
#region Ad callbacks

        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Stop();
        }

        public override void OnLoadSuccess()
        {
            base.OnLoadSuccess();
            State = AdLoadingState.Loaded;
            LoadingTime.Stop();
            TriggerAutoShowActionIfNeeded();
        }

        public void OnShown()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            AnalyticsManager.TrackNativeAdShown(new AdShownEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
                    AdTag = Type,
                    AdCount = AdCount(),
                    AdLoadingTime = LoadingTimeMilliseconds(),
                    AdNetworkName = adInfo.AdNetworkName,
                    WaterfallTestName = adInfo.WaterfallTestName,
                    WaterfallName = adInfo.WaterfallName,
                    ImpressionId = Uuid,
                }
            );
        }
        
        public void OnFailedToShow(VoodooErrorAdInfo errorAdInfo)
        {
            TriggerCallback();
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            AnalyticsManager.TrackNativeAdShowFailed(new AdShowFailedEventAnalyticsInfo {
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
        }

        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            AnalyticsManager.TrackNativeAdClick(new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdLoadingTime = LoadingTimeMilliseconds(),
                AdCount = AdCount(),
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                ImpressionId = Uuid,
            });
        }

        public void OnDismissed()
        {
            TriggerCallback();
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            AnalyticsManager.TrackNativeAdDismiss(new AdClosedEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
                    AdTag = Type,
                    AdLoadingTime = LoadingTimeMilliseconds(),
                    AdCount = AdCount(),
                    AdNetworkName = adInfo.AdNetworkName,
                    WaterfallTestName = adInfo.WaterfallTestName,
                    WaterfallName = adInfo.WaterfallName,
                    ImpressionId = Uuid,
                }
            );
        }

        public void OnImpression(ImpressionInfoType type)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetNativeAdsInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.NativeAds;
            info.AdCount = AdCount();
            info.AdTag = Type;
            info.AdLoadingTime = LoadingTimeMilliseconds();
            info.AppVersion = AppVersion;
            info.ImpressionId = Uuid;
            AnalyticsManager.TrackImpression(info);
        }
        
#endregion
    }
}