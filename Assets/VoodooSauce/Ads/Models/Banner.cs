using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IAP;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class Banner : BaseAd
    {
        private const string TAG = "AdsManager.Banner";
        private const int BANNER_HEIGHT = 50;
        private float _bannerHeightCachedValue;
        private bool _haveBannerHeightCachedValue;
        private Action _autoShowAfterInitAction;
        private Action<float> _callback;
        
        public bool IsShowing { get; private set; }

        private enum InternalAdState
        {
            NOT_INITIALIZED_YET,
            CAN_NOT_SHOW,
            CAN_SHOW
        }
        
        public Banner(IMediationAdapter adapter) : base(adapter) { }

        internal override void Initialize() => MediationAdapter.InitializeBanner();
        
        internal new bool IsAvailable() => State != AdLoadingState.Disabled;
        
        internal float GetNativeBannerHeightInDp()
        {
            if (_haveBannerHeightCachedValue && _bannerHeightCachedValue > 0f) {
                return _bannerHeightCachedValue;
            }

            _bannerHeightCachedValue = MediationAdapter.GetNativeBannerHeight();
            _haveBannerHeightCachedValue = true;
            return _bannerHeightCachedValue;
        }

        internal float GetNativeBannerHeightInPx()
        {
            float height = GetNativeBannerHeightInDp();
            if (height < 0)
                return -1f;
            return height * (ScreenSizeUtils.GetDpi() / 160);
        }

        internal float GetMediationScreenDensity() => MediationAdapter.GetScreenDensity();

        private InternalAdState CanShow()
        {
            if (State == AdLoadingState.Disabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Banner has been disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooPremium.IsPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User is premium, not displaying Banner");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User has an active subscription, ads disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }
            
            if (!MediationAdapter.IsSdkInitialized() || State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Banner is not initialized yet");
                return InternalAdState.NOT_INITIALIZED_YET;
            }
            
            if (IsShowing) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Banner is already showing");
                return InternalAdState.CAN_NOT_SHOW;
            }

            return InternalAdState.CAN_SHOW;
        }
        
        public void Show(Action<float> onBannerDisplayed)
        {
            InternalAdState showBannerCheckResult = CanShow();
            switch (showBannerCheckResult) {
                case InternalAdState.CAN_NOT_SHOW:
                    return;
                case InternalAdState.NOT_INITIALIZED_YET:
                    _autoShowAfterInitAction = () => { Show(onBannerDisplayed); };
                    return;
                case InternalAdState.CAN_SHOW:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Showing Banner ...");
                    IsShowing = true;
                    _callback = onBannerDisplayed;
                    _autoShowAfterInitAction = null;
                    MediationAdapter.ShowBanner();
                    MediationAdapter.SetBannerBackgroundVisibility(true, VoodooSettings.Load().BannerBackgroundColor);
                    return;
            }
        }

        public void Hide()
        {
            _autoShowAfterInitAction = null;
            switch (State) {
                case AdLoadingState.Disabled:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Banner has been disabled");
                    return;
                case AdLoadingState.NotInitialized:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Mrec is not initialized yet");
                    return;
                default: {
                    if (!IsShowing) {
                        VoodooLog.LogDebug(Module.ADS, TAG, "Banner is not displayed");
                        return;
                    }

                    _callback = null;
                    VoodooLog.LogDebug(Module.ADS, TAG, "Hiding banner ...");
                    IsShowing = false;
                    MediationAdapter.HideBanner();
                    MediationAdapter.SetBannerBackgroundVisibility(false,  VoodooSettings.Load().BannerBackgroundColor);
                    return;
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            if (!MediationAdapter.IsSdkInitialized()) {
                return;
            }
            MediationAdapter.InitializeBanner();
        }

        public override void Disable()
        {
            base.Disable();
            _autoShowAfterInitAction = null;
            BannerBackground.Hide();
            if (!MediationAdapter.IsSdkInitialized()) {
                return;
            }
            MediationAdapter.HideBanner();
            MediationAdapter.DestroyBanner();
        }
        
        private  void TriggerCallback()
        {
            if (_callback == null) {
                return;
            }

            float bannerHeight = GetNativeBannerHeightInDp();
            if (bannerHeight <= 0)
                bannerHeight = BANNER_HEIGHT;
            
            _callback.Invoke(bannerHeight);
        }
        
        private void TriggerAutoShowActionIfNeeded()
        {
            if (_autoShowAfterInitAction == null) {
                return;
            }
            _autoShowAfterInitAction?.Invoke();
            _autoShowAfterInitAction = null;
        }

#region Ad callbacks

        internal override void OnInitialized(bool success)
        {
            base.OnInitialized(success);
            if (!success) {
                return;
            }
            OnLoading();
            TriggerAutoShowActionIfNeeded();
        }

        public override void OnLoadSuccess()
        {
            //Banner is refreshed, so the native height wont be valid anymore, we shouldn't return from cache then
            _haveBannerHeightCachedValue = false;
            
            base.OnLoadSuccess();
            TriggerCallback();
            LoadingTime.Restart();
            
            BannerBackground.UpdateHeight(GetNativeBannerHeightInDp(), GetMediationScreenDensity());
            
            // As the banner is refreshed every X seconds, its unique identifier must be updated here.
            // As only two events are generated with the banner (show and click), it's ok to generate the id now.
            GenerateUuid();
            
            VoodooAdInfo adInfo = MediationAdapter.GetBannerInfo();
            AnalyticsManager.TrackBannerShown(new AdEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
            });
        }

        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Restart();
        }

        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetBannerInfo();
            AnalyticsManager.TrackBannerClick(new AdEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdNetworkName = adInfo.AdNetworkName,
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
            });
        }

        public void OnImpression(ImpressionInfoType type)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetBannerInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.Banner;
            info.AdLoadingTime = LoadingTimeMilliseconds();
            info.AppVersion = AppVersion;
            info.ImpressionId = Uuid;
            // info.AdCount = AdCount(); No AdCount for banner yet
            // info.AdTag = Type; No AdTag for banner yet
            AnalyticsManager.TrackImpression(info);
        }
        
#endregion
    }
}