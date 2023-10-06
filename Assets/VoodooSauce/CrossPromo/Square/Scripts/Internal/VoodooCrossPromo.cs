using System;
using UnityEngine.Networking;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.Canvas;
using Voodoo.Sauce.Internal.CrossPromo.Configuration;
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Internal.CrossPromo.Utils;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace Voodoo.Sauce.Internal.CrossPromo
{
    internal static class VoodooCrossPromo
    {
        public const string TAG = "CrossPromotion";

        /// <summary>
        /// Information about the cross promotion
        /// </summary>
        public static CrossPromoInfo Info = new CrossPromoInfo { CrossPromoIsReady = false, Format = "BigSquare" };

        /// <summary>
        /// Rest Client for making API call
        /// </summary>
        private static CrossPromoAPI _api;

        /// <summary>
        /// Check if the init method was called
        /// </summary>
        private static bool _isInit;

        private static CrossPromoConfigurationHelper _configurationHelper = new CrossPromoConfigurationHelper();

        /// <summary>
        /// Customs Callbacks given by the function Show
        /// </summary>
        private static Action<AssetModel> _onSuccess;

        private static Action<CPShowFailType> _onFailure;

        private static bool _showCalledBeforeInit;
        public static CrossPromoConfigurationHelper Configuration => _configurationHelper;

        /// <summary>
        /// Init the cross promotion
        /// Download the file in the cache and make API call to retrieve information about
        /// the cross promotion
        /// </summary>
        public static void Init()
        {
            if (_isInit) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG, "You already init the cross promotion.");
                return;
            }

            _isInit = true;

            var config = VoodooSauce.GetItem<CrossPromoConfiguration>();
            bool crossPromoEnabled = CrossPromoSettings.IsCrossPromoEnabled();
            _configurationHelper.Initialize(config, crossPromoEnabled);

            if (!crossPromoEnabled) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG, "Cross promo is disabled in settings.");
                return;
            }

            if (!_configurationHelper.ShouldBeEnabled()) {
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG,
                    "Cross promo is disabled in VoodooTune configuration.");
                return;
            }

            if (VoodooSauce.IsPremium()) {
                VoodooLog.LogDebug(Module.CROSS_PROMO, TAG, "User is PREMIUM, cross promo is disabled.");
                return;
            }

            try {
                RegisterInternalCallbacks();

                _api = CrossPromoAPI.CreateGameObject();
                CacheManager.CreateCacheIfNoExist();
                _api.GetGameInfoAndDownload();
            } catch (Exception e) {
                VoodooLog.LogError(Module.CROSS_PROMO, TAG, e.ToString());
            }

            if (_showCalledBeforeInit)
                Show(_onSuccess, _onFailure);
        }

        private static void RegisterInternalCallbacks()
        {
            CrossPromoEvents.OnInitComplete += InvokeInitCompleteCallback_Internal;
            CrossPromoEvents.OnClick += InvokeOnClickCallback_Internal;
            CrossPromoEvents.OnShown += InvokeOnShownCallback_Internal;
            CrossPromoEvents.OnError += InvokeOnErrorReceivedCallback_Internal;
        }

        /// <summary>
        /// Display an ad
        /// </summary>
        /// <param name="onSuccess">Raise when a video can be displayed</param>
        /// <param name="onFailure">Raise when no ad can be displayed</param>
        public static void Show(Action<AssetModel> onSuccess, Action<CPShowFailType> onFailure)
        {
            Info.GenerateUuid();

            if (!_isInit) {
                _showCalledBeforeInit = true;
            }

            try {
                _onSuccess = onSuccess;
                _onFailure = onFailure;
                if (!_isInit) {
                    onFailure?.Invoke(CPShowFailType.NotInitialized);
                    return;
                }

                if (VoodooSauce.IsPremium()) {
                    onFailure?.Invoke(CPShowFailType.UserIsPremium);
                    return;
                }

                if (!_configurationHelper.ShouldBeShown()) {
                    onFailure?.Invoke(CPShowFailType.ShouldNotBeShown);
                    return;
                }

                if (_api != null) {
                    if (!_api.IsRetrievingInfo() && !Info.HasInternet) {
                        _api.GetGameInfoAndDownload();
                    }

                    if (AFormatManager.Instance == null) {
                        _api.WaitForFirstFrame(CrossPromoDisplayEvents.TriggerShow);
                    } else {
                        CrossPromoDisplayEvents.TriggerShow();
                    }
                }
            } catch (Exception e) {
                CrossPromoEvents.TriggerErrorEvent(e);
            }
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Hide the current video
        /// </summary>
        public static void Hide()
        {
            if (!_isInit)
                _showCalledBeforeInit = false;
            try {
                if (!_isInit)
                    return;

                if (AFormatManager.Instance == null) {
                    if (_api != null) {
                        _api.WaitForFirstFrame(CrossPromoDisplayEvents.TriggerHide);
                    }
                } else {
                    CrossPromoDisplayEvents.TriggerHide();
                }
            } catch (Exception e) {
                CrossPromoEvents.TriggerErrorEvent(e);
            }
        }

        private static void InvokeInitCompleteCallback_Internal(string format)
        {
            Info.CrossPromoIsReady = true;
        }

        private static void InvokeOnShownCallback_Internal(AssetModel asset)
        {
            _onSuccess?.Invoke(asset);

            if (asset == null) {
                return;
            }

            if (!PlatformUtils.UNITY_EDITOR) {
                if (PlatformUtils.UNITY_IOS && IOSCrossPromoWrapper.CheckGoodIosVersion()) {
                    IOSCrossPromoWrapper.LoadNativeStore(asset.game.apple_id);
                }

                if (!string.IsNullOrEmpty(asset.tracking_impression)) {
                    UnityWebRequest r = UnityWebRequest.Get(asset.tracking_impression);
                    r.SendWebRequest();
                }
            }

            AnalyticsStorageHelper.Instance.IncrementShowCrossPromoCount();
            CrossPromoAnalyticsInfo cpAnalyticsInfo = asset.ToAnalyticsModel(
                AnalyticsStorageHelper.Instance.GetGameCount(),
                AnalyticsStorageHelper.Instance.GetShowCrossPromoCount(),
                Info.Uuid);
            AnalyticsManager.TrackCrossPromoShown(cpAnalyticsInfo);

            if (_api != null) {
                _api.BufferVideosInCache();
            }
        }

        private static void InvokeOnClickCallback_Internal(AssetModel asset)
        {
            if (asset == null)
                return;

            CrossPromoAnalyticsInfo cpAnalyticsInfo = asset.ToAnalyticsModel(
                AnalyticsStorageHelper.Instance.GetGameCount(),
                AnalyticsStorageHelper.Instance.GetShowCrossPromoCount(),
                Info.Uuid);
            AnalyticsManager.TrackCrossPromoClick(cpAnalyticsInfo);
        }

        private static void InvokeOnErrorReceivedCallback_Internal(Exception e)
        {
            if (e == null) {
                _onFailure?.Invoke(CPShowFailType.NoVideoToDisplay);
                VoodooLog.LogWarning(Module.CROSS_PROMO, TAG,
                    "No video to display. Check your internet connection, " +
                    " you already installed all the games or there is no game to promote.");
            } else {
                _onFailure?.Invoke(CPShowFailType.UnknownError);
                VoodooLog.LogError(Module.CROSS_PROMO, TAG, e.Message);
                AnalyticsManager.TrackCrossPromoError(e.Message);
            }
        }
    }

    public enum CPShowFailType
    {
        UnknownError,
        NotInitialized,
        UserIsPremium,
        NoVideoToDisplay,
        ShouldNotBeShown
    }
}