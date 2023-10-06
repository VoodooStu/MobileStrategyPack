using System;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class Mrec : BaseAd
    {
        private const string TAG = "AdsManager.Mrec";

        private Action _autoShowAfterInitAction;

        private enum InternalAdState
        {
            NOT_INITIALIZED_YET,
            CAN_NOT_SHOW,
            CAN_SHOW
        }

        public Mrec(IMediationAdapter adapter) : base(adapter) { }
        
        internal override void Initialize() => MediationAdapter.InitializeMrec();

        private InternalAdState CanShow()
        {
            if (State == AdLoadingState.Disabled) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Mrec has been disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooPremium.IsPremium()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User is premium, not displaying Mrec");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (VoodooSauceCore.GetInAppPurchase().IsSubscribedProduct()) {
                VoodooLog.LogDebug(Module.ADS, TAG, "User has an active subscription, ads disabled");
                return InternalAdState.CAN_NOT_SHOW;
            }

            if (State == AdLoadingState.NotInitialized) {
                VoodooLog.LogDebug(Module.ADS, TAG, "Mrec is not initialized yet");
                return InternalAdState.NOT_INITIALIZED_YET;
            }

            return InternalAdState.CAN_SHOW;
        }

        internal void Show(float x, float y, string tag)
        {
            InternalAdState showMrecCheckResult = CanShow();
            switch (showMrecCheckResult) {
                case InternalAdState.CAN_NOT_SHOW:
                    break;
                case InternalAdState.NOT_INITIALIZED_YET:
                    _autoShowAfterInitAction = () => { Show(x, y, tag); };
                    break ;
                case InternalAdState.CAN_SHOW:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Showing Mrec ...");
                    Type = tag;
                    _autoShowAfterInitAction = null;
                    MediationAdapter.ShowMrec(x, y);
                    break ;
            }
        }

        internal void Hide()
        {
            _autoShowAfterInitAction = null;
            switch (State) {
                case AdLoadingState.Disabled:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Mrec has been disabled");
                    break;
                case AdLoadingState.NotInitialized:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Mrec is not initialized yet");
                    break;
                default: {
                    MediationAdapter.HideMrec();
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
            MediationAdapter.InitializeMrec();
        }

        public override void Disable()
        {
            base.Disable();
            _autoShowAfterInitAction = null;
            MediationAdapter.HideMrec();
            MediationAdapter.DestroyMrec();
        }

        private static int AdCount() => AnalyticsStorageHelper.Instance.GetMrecCount();

        private static int IncrementAdCount() => AnalyticsStorageHelper.Instance.IncrementMrecCount();

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
            base.OnLoadSuccess();
            LoadingTime.Stop();

            VoodooAdInfo adInfo = MediationAdapter.GetMrecInfo();
            AnalyticsManager.TrackMrecShown(new AdShownEventAnalyticsInfo {
                    AdUnit = adInfo.AdUnit,
                    AdTag = Type,
                    AdCount = IncrementAdCount(),
                    AdNetworkName = adInfo.AdNetworkName,
                    AdLoadingTime = LoadingTimeMilliseconds(),
                    WaterfallTestName = adInfo.WaterfallTestName,
                    WaterfallName = adInfo.WaterfallName,
                    Creative = adInfo.Creative,
                    ImpressionId = Uuid,
                }
            );
        }

        public override void OnLoadFailed()
        {
            base.OnLoadFailed();
            LoadingTime.Stop();
        }

        public void OnClicked()
        {
            VoodooAdInfo adInfo = MediationAdapter.GetMrecInfo();
            AnalyticsManager.TrackMrecClick(new AdClickEventAnalyticsInfo {
                AdUnit = adInfo.AdUnit,
                AdTag = Type,
                AdCount = AdCount(),
                AdNetworkName = adInfo.AdNetworkName,
                AdLoadingTime = LoadingTimeMilliseconds(),
                WaterfallTestName = adInfo.WaterfallTestName,
                WaterfallName = adInfo.WaterfallName,
                Creative = adInfo.Creative,
                ImpressionId = Uuid,
            });
        }

        public void OnImpression(ImpressionInfoType type)
        {
            VoodooAdInfo adInfo = MediationAdapter.GetMrecInfo();
            ImpressionAnalyticsInfo info = adInfo.ToInfoType(type);
            info.AdUnitFormat = ImpressionAdUnitFormat.Mrec;
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