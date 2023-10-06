using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Sauce.Privacy;
using AdnConstants = Voodoo.Sauce.Internal.Analytics.AdnAnalyticsConstants;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AdnAnalyticsProvider : IAnalyticsProvider
    {
        private const string TAG = "AdnAnalyticsProvider";
        private readonly DateTime _startTime = DateTime.Now;
        private bool _isInitialized;
        private string _unityVersion;
        private string _vsVersion;
        private string _firstAppLaunchDate;
        private int _appOpenCountSinceInstall;
        private string _vanUserId;
        private string _vanImpressionId;
        private string _vanFsImpressionId;
        private string _vanRvImpressionId;
        private string _appStoreId;
        private bool _isRvSkipped;
        private int _playTime;
        private int _loadRequestCount;
        private int _gameCount;
        private int _gameWonCount;
        private AdClick _adClick;
        private AdRevenue _totalRevenue;
        private double? _adRevenue;
        private PerfMetrics _perfMetrics;

        private AdSessionQueue _adSessions;

        public void Instantiate(string mediation) { }

        public void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild)
        {
            try {
                AdnSdk.SubscribeOnSdkInitializedEvent(() => { UnityThreadExecutor.Execute(OnSdkInitialized); });
                //Send Cross session data
                InitSessionInfo();
                SendSdkExtrasParameters();
            } catch (Exception e) {
                VoodooLog.LogError(Module.ADS, TAG, "AdnAnalyticsProvider initialization error: " + e.Message);
                VoodooSauce.LogException(e);
            }
        }

        private void OnSdkInitialized()
        {
            try {
                if (_isInitialized) return;
                _isInitialized = true;
                RegisterEvents();
            } catch (Exception e) {
                VoodooLog.LogError(Module.ADS, TAG, "AdnAnalyticsProvider initialization error: " + e.Message);
                VoodooSauce.LogException(e);
            }
        }

        private void InitSessionInfo()
        {
            IAnalyticStorage analyticStorage = AnalyticsStorageHelper.Instance;
            _totalRevenue = new AdRevenue();
            _adClick = new AdClick();
            _perfMetrics = new PerfMetrics();
            _adSessions = new AdSessionQueue();
            _firstAppLaunchDate = analyticStorage.GetFirstAppLaunchDate()?.ToIsoFormat();
            _appOpenCountSinceInstall = analyticStorage.GetAppLaunchCount();
            _unityVersion = Application.unityVersion;
            _vsVersion = VoodooConfig.Version();
            _vanUserId = AnalyticsUserIdHelper.GetUserId();
            if (PlatformUtils.UNITY_IOS) {
                _appStoreId = VoodooSettings.Load().AppleStoreId;
            }
        }

        private void RegisterEvents()
        {
            AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            AnalyticsManager.OnImpressionTrackedEvent += OnImpressionTrackedEvent;
            AnalyticsManager.OnShowInterstitialEvent += OnShowInterstitialEvent;
            AnalyticsManager.OnInterstitialClickedEvent += OnInterstitialClickedEvent;
            AnalyticsManager.OnInterstitialDismissedEvent += OnInterstitialDismissedEvent;
            AnalyticsManager.OnShowRewardedVideoEvent += OnShowRewardedVideoEvent;
            AnalyticsManager.OnRewardedVideoClickedEvent += OnRewardedVideoClickedEvent;
            AnalyticsManager.OnRewardedVideoClosedEvent += OnRewardedVideoClosedEvent;
            AnalyticsManager.OnTrackPerformanceMetricsEvent += TrackPerformanceMetrics;
            AnalyticsManager.OnInterstitialLoadRequestEvent += OnInterstitialLoadRequestEvent;
            AnalyticsManager.OnRewardedVideoLoadRequestEvent += OnRewardedLoadRequestEvent;
        }

        private void OnGameFinished(GameFinishedParameters parameters)
        {
            _gameCount++;
            if (parameters.status) _gameWonCount++;
            _playTime += parameters.gameDuration / AdnConstants.MILLIS_PER_SECOND;
        }

        private void OnImpressionTrackedEvent(ImpressionAnalyticsInfo info)
        {
            _adRevenue = info.Revenue;
            switch (info.AdUnitFormat) {
                case ImpressionAdUnitFormat.RewardedVideo:
                    if (info.Revenue != null) _totalRevenue.RvRevenue += (double) info.Revenue;
                    break;
                case ImpressionAdUnitFormat.Interstitial:
                    if (info.Revenue != null) _totalRevenue.FsRevenue += (double) info.Revenue;
                    break;
            }
        }

        private void OnInterstitialClickedEvent(AdClickEventAnalyticsInfo info)
        {
            _adClick.IsFsClicked = true;
        }

        private void OnShowInterstitialEvent(AdShownEventAnalyticsInfo info)
        {
            _adClick.IsFsClicked = false;
        }

        private void OnInterstitialDismissedEvent(AdClosedEventAnalyticsInfo info)
        {
            _adSessions.UpdateAdSessions(AdnConstants.FS, info, _adRevenue, _adClick.IsFsClicked, false);
        }

        private void OnRewardedVideoClickedEvent(AdClickEventAnalyticsInfo info)
        {
            _adClick.IsRvClicked = true;
        }

        private void OnShowRewardedVideoEvent(AdShownEventAnalyticsInfo info)
        {
            //Reset previous ad values
            _adClick.IsRvClicked = false;
            _isRvSkipped = false;
        }

        private void OnRewardedVideoClosedEvent(AdClosedEventAnalyticsInfo info)
        {
            _isRvSkipped = !info.AdReward ?? true;
            _adSessions.UpdateAdSessions(AdnConstants.RV, info, _adRevenue, _adClick.IsRvClicked, _isRvSkipped);
        }

        private void OnInterstitialLoadRequestEvent(AdAnalyticsInfo info)
        {
            _vanFsImpressionId = info.ImpressionId;
            OnLoadRequestEvent(info);
        }

        private void OnRewardedLoadRequestEvent(AdAnalyticsInfo info)
        {
            _vanRvImpressionId = info.ImpressionId;
            OnLoadRequestEvent(info);
        }

        private void OnLoadRequestEvent(AdAnalyticsInfo info)
        {
            _loadRequestCount++;
            _vanImpressionId = info.ImpressionId;
            //Send ExtraParameters Before each FS/RV load
            SendSdkExtrasParameters();
        }

        private void TrackPerformanceMetrics(PerformanceMetricsAnalyticsInfo metrics)
        {
            _perfMetrics.BadFrames = metrics.BadFrames;
            _perfMetrics.TerribleFrames = metrics.TerribleFrames;
        }

        private void SendSdkExtrasParameters()
        {
            try {
                SessionInfo sessionInfo = AnalyticsSessionManager.Instance().SessionInfo;
                IAnalyticStorage analyticStorage = AnalyticsStorageHelper.Instance;
                var extraParams = new Dictionary<string, object>();
                extraParams.AddIfNotNull(AdnConstants.UNITY_VERSION, _unityVersion);
                extraParams.AddIfNotNull(AdnConstants.VSAUCE_VERSION, _vsVersion);
                extraParams.AddIfNotNull(AdnConstants.SESSION_ID, sessionInfo.id);
                extraParams.AddIfNotNull(AdnConstants.INSTALL_DATE, _firstAppLaunchDate);
                extraParams.AddIfNotNull(AdnConstants.APP_OPEN_COUNT_SINCE_INSTALL, _appOpenCountSinceInstall);
                extraParams.AddIfNotNull(AdnConstants.SESSION_COUNT_SINCE_INSTALL, sessionInfo.count);
                extraParams.AddIfNotNull(AdnConstants.GAME_COUNT_SINCE_INSTALL, analyticStorage.GetGameCount());
                extraParams.AddIfNotNull(AdnConstants.FS_COUNT_SINCE_INSTALL, analyticStorage.GetShowInterstitialCount());
                extraParams.AddIfNotNull(AdnConstants.RV_COUNT_SINCE_INSTALL, analyticStorage.GetShowRewardedVideoCount());
                extraParams.AddIfNotNull(AdnConstants.VAN_USER_ID, _vanUserId);
                extraParams.AddIfNotNull(AdnConstants.VAN_IMPRESSION_ID, _vanImpressionId);
                extraParams.AddIfNotNull(AdnConstants.VAN_FS_IMPRESSION_ID, _vanFsImpressionId);
                extraParams.AddIfNotNull(AdnConstants.VAN_RV_IMPRESSION_ID, _vanRvImpressionId);
                extraParams.AddIfNotNull(AdnConstants.HAS_SKIP_PRIOR_RV, _isRvSkipped ? 1 : 0);
                extraParams.AddIfNotNull(AdnConstants.PLAY_TIME, _playTime);
                extraParams.AddIfNotNull(AdnConstants.TOTAL_TIME, TimeSinceStartup());
                extraParams.AddIfNotNull(AdnConstants.REV_GEN, _totalRevenue.ToDictionary());
                extraParams.AddIfNotNull(AdnConstants.COUNT_REQ, _loadRequestCount);
                extraParams.AddIfNotNull(AdnConstants.GAME_COUNT, _gameCount);
                extraParams.AddIfNotNull(AdnConstants.PERF_METRICS, _perfMetrics.ToDictionary());
                extraParams.AddIfNotNull(AdnConstants.GAME_COUNT_SUCCESS, _gameWonCount);
                extraParams.AddIfNotNull(AdnConstants.HAS_LAST_CLICK, _adClick.ToDictionary());
                extraParams.AddIfNotNull(AdnConstants.LAST_X_SESSION_DESC, _adSessions.LatestSessionsDictionaryList());
                extraParams.AddIfNotNull(AdnConstants.FIRST_X_SESSION_DESC, _adSessions.FirstSessionsDictionaryList());
                extraParams.AddIfNotNull(AdnConstants.APPSTORE_ID, _appStoreId);

                string extraParamsJson = JsonConvert.SerializeObject(extraParams);
                AdnSdk.SetBidTokenExtraParams(extraParamsJson);
            } catch (Exception e) {
                VoodooLog.LogError(Module.ADS, TAG, "AdnAnalyticsProvider SetExtrasParameters error: " + e.Message);
                VoodooSauce.LogException(e);
            }
        }

        private int TimeSinceStartup()
        {
            double time = (DateTime.Now - _startTime).TotalSeconds;
            if (time > 0) return (int) time;
            return 0;
        }
    }
}