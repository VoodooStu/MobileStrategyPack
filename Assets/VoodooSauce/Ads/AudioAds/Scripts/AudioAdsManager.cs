using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Voodoo.Analytics;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    internal class AudioAdsManager
    {
        private const string TAG = "AudioAdsManager";

        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
        internal static AudioAdsManager Instance => _instance ?? (_instance = new AudioAdsManager());
        private static AudioAdsManager _instance;
        
        internal bool IsEnabled { get; private set; }
        internal string AdNetworkName => _currentAudioAdNetwork?.Name;
        
        private AudioAdConfig _currentAudioAdConfig;
        private IAudioAdsNetwork _currentAudioAdNetwork;
        private int _gameStartCounter;
        private float _lastAudioAdTime = -float.MaxValue;
        private string _currentImpressionId;
        private AudioAdPosition? _audioAdPosition;
        private Vector2Int _audioAdOffset;
        private IAudioAdPositionBehaviour _audioAdPrefab;
        private IAudioAdsAnalyticsManager _audioAdsAnalyticsManager;
        private Func<bool> _checkPremiumOrIapSubscribedFunction;

        #region InternalMethods
        
        internal void Initialize(IVoodooSettings settings, [CanBeNull] AudioAdConfig voodooTuneConfig, bool consent, bool 
        isCcpaApplicable, [NotNull] Func<bool> checkPremiumOrIapSubscribedFunction, IAudioAdsAnalyticsManager audioAdsAnalyticsManager)
        {
            _checkPremiumOrIapSubscribedFunction = checkPremiumOrIapSubscribedFunction;
            if (_checkPremiumOrIapSubscribedFunction.Invoke())
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Audio ads module isn't initialized because the user is premium or has a subscription.");
                return;
            }

            if (IsEnabled)
            {
                // The AudioAdsManager is already initialized.
                return;
            }
            
            _audioAdsAnalyticsManager = audioAdsAnalyticsManager;

            // Retrieving audio ad configuration (either from Kitchen or VoodooTune).
            _currentAudioAdConfig = voodooTuneConfig ??
                                    (PlatformUtils.UNITY_IOS
                                        ? settings.GetIosAudioAdConfig
                                        : settings.GetAndroidAudioAdConfig);
            // !! We might remove this condition if we add another way to trigger an audio ad in the future.
            if (_currentAudioAdConfig.gameStartTriggerFrequency == 0)
            {
                VoodooLog.LogDebug(Module.ADS, TAG,
                    "The audio ad network won't be initialized because the game start frequency of the configuration is set to 0.");
                return;
            }

            if (AssignAudioAdNetwork())
            {
                IsEnabled = true;
                
                RegisterCallbacks();

                InitializeAudioAdNetwork(consent, isCcpaApplicable);
            }
        }

        internal void ShowOrHideAudioAd()
        {
            if (_currentAudioAdNetwork.IsShowingAd())
            {
                _currentAudioAdNetwork.CloseAudioAd();
            }
            else
            {
                ShowAudioAd();
            }
        }
        
        internal void SetAudioAdPosition(AudioAdPosition position, Vector2Int offset)
        {
            _audioAdPosition = position;
            _audioAdOffset = offset;
        }

        internal void LinkAudioAdPositionToPrefab(IAudioAdPositionBehaviour prefab)
        {
            _audioAdPrefab = prefab;
        }

        internal Vector2 GetTimer()
        {
            if (_currentAudioAdConfig == null)
            {
                return -Vector2.one;
            }

            var cooldown = _currentAudioAdConfig.coolDownBetweenAudioAds;

            return new Vector2(Mathf.Min(Time.realtimeSinceStartup - _lastAudioAdTime, cooldown),
                _currentAudioAdConfig.coolDownBetweenAudioAds);
        }

        internal State GetState()
        {
            if (!IsEnabled || _checkPremiumOrIapSubscribedFunction == null) {
                return State.Disabled;
            }
            
            if (_checkPremiumOrIapSubscribedFunction.Invoke())
            {
                return State.Disabled;
            }

            if (_currentAudioAdNetwork == null || _currentAudioAdConfig == null)
            {
                return State.Misconfigured;
            }

            if (_currentAudioAdNetwork.IsShowingAd())
            {
                return State.ShowingAd;
            }

            var audioAdPrefabActive = _audioAdPrefab != null && _audioAdPrefab.IsActiveAndEnabled;
            if (!audioAdPrefabActive && _audioAdPosition == null)
            {
                return State.PositionConfigMissing;
            }

            if (Time.realtimeSinceStartup - _lastAudioAdTime < _currentAudioAdConfig.coolDownBetweenAudioAds)
            {
                return State.TooEarly;
            }

            if (!_currentAudioAdNetwork.IsAudioAdAvailable())
            {
                return State.LoadingAd;
            }

            return State.Ready;
        }

        #endregion InternalMethods

        #region PrivateMethods
        
        private void RegisterCallbacks()
        {
            AnalyticsManager.OnGameStartedEvent += OnGameStart;
            if (_currentAudioAdConfig.killWhenGameFinishes)
            {
                AnalyticsManager.OnGameFinishedEvent += OnGameFinished;
            }

            if (_currentAudioAdConfig.killWhenFsOrRvStarts)
            {
                AdsManager.Interstitial.OnShow += OnRvOrFsShow;
                AdsManager.RewardedVideo.OnShow += OnRvOrFsShow;
                AdsManager.RewardedInterstitialVideo.OnShow += OnRvOrFsShow;
            }
        }

        private bool AssignAudioAdNetwork()
        {
            var audioNetworkName = _currentAudioAdConfig.adNetwork?.Trim().ToLower();
            if (string.IsNullOrEmpty(audioNetworkName))
            {
                VoodooLog.LogDebug(Module.ADS, TAG, "Audio ads module isn't initialized because there is no ad network name in the configuration.");
                return false;
            }
            
            // Researching the relevant audio ad network according to _currentAudioAdConfig.adNetwork.
            List<IAudioAdsNetwork> audioAdsNetworks = AssembliesUtils.InstantiateInterfaceImplementations<IAudioAdsNetwork>();
            foreach (var audioAdsNetwork in audioAdsNetworks)
            {
                if (audioAdsNetwork.Name == audioNetworkName)
                {
                    _currentAudioAdNetwork = audioAdsNetwork;
                    return true;
                }
            }

            VoodooLog.LogError(Module.ADS, TAG, "The audio ad network called " + audioNetworkName + " hasn't been found.");
            return false;
        }

        private void InitializeAudioAdNetwork(bool consent, bool isCcpaApplicable)
        {
            _currentAudioAdNetwork.SetOnAudioAdClick(OnAudioAdClick);
            _currentAudioAdNetwork.SetOnAudioAdShow(OnAudioAdShow);
            _currentAudioAdNetwork.SetOnAudioAdImpression(OnAudioAdImpression);
            _currentAudioAdNetwork.SetOnAudioAdWatched(OnAudioAdWatched);
            _currentAudioAdNetwork.SetOnAudioAdUserClose(OnAudioAdUserClose);
            _currentAudioAdNetwork.SetOnAudioAdFailed(OnAudioAdFailed);
            _currentAudioAdNetwork.Initialize(consent, isCcpaApplicable);
        }
        
        private void ShowAudioAd()
        {
            switch (GetState())
            {
                case State.Disabled:
                    VoodooLog.LogDebug(Module.ADS, TAG, "The Audio ad module is disabled. Therefore, no ads will be shown.");
                    break;
                case State.Misconfigured:
                    VoodooLog.LogDebug(Module.ADS, TAG, "The Audio ad module is misconfigured. Therefore, no ads will be shown.");
                    break;
                case State.TooEarly:
                    VoodooLog.LogDebug(Module.ADS, TAG, "No audio ad can be displayed because not enough time has passed since the last one.");
                    break;
                case State.PositionConfigMissing:
                    VoodooLog.LogError(Module.ADS, TAG, "You must add the AudioAdPosition prefab (Assets/VoodooSauce/Ads/AudioAds) in your scene or call VoodooSauce.SetAudioAdPosition before showing an audio ad.");
                    break;
                case State.ShowingAd:
                    VoodooLog.LogDebug(Module.ADS, TAG, "An audio ad is already showing. No new ad will be shown.");
                    break;
                case State.LoadingAd:
                    VoodooLog.LogDebug(Module.ADS, TAG, "Can't display an audio ad because it's not loaded yet.");
                    
                    _audioAdsAnalyticsManager.TrackAudioAdTrigger(InitializeAudioAdAnalyticsInfo(new AudioAdTriggerAnalyticsInfo()));

                    break;
                case State.Ready:
                    _currentImpressionId = Guid.NewGuid().ToString();
                    _lastAudioAdTime = Time.realtimeSinceStartup;
                    AnalyticsStorageHelper.Instance.IncrementAudioAdCount();
                    
                    var info = InitializeAudioAdAnalyticsInfo(new AudioAdTriggerAnalyticsInfo());
                    info.AdLoaded = true;
                    _audioAdsAnalyticsManager.TrackAudioAdTrigger(info);

                    if (_audioAdPrefab != null && _audioAdPrefab.IsActiveAndEnabled)
                    {
                        _currentAudioAdNetwork.ShowAudioAd(_audioAdPrefab);
                    }
                    else
                    {
                        _currentAudioAdNetwork.ShowAudioAd(_audioAdPosition.Value, _audioAdOffset);
                    }

                    break;
            }
        }

        private T InitializeAudioAdAnalyticsInfo<T>(T info) where T: AudioAdAnalyticsInfo
        {
            info.networkName = _currentAudioAdNetwork.Name;
            info.impressionId = _currentImpressionId;
            info.adCount = AnalyticsStorageHelper.Instance.GetAudioAdCount();
            return info;
        }
        
        #endregion
        
        #region Events
        
        internal void OnApplicationPause(bool pauseStatus)
        {
            _currentAudioAdNetwork?.OnApplicationPause(pauseStatus);
        }

        internal void OnRvOrFsShow()
        {
            if (_currentAudioAdNetwork == null) return;
            
            if (_currentAudioAdConfig.killWhenFsOrRvStarts)
            {
                _currentAudioAdNetwork.CloseAudioAd();
            }
            _currentAudioAdNetwork.OnFullscreenAdShow();
        }
        
        internal void OnGameStart(GameStartedParameters parameters)
        {
            _gameStartCounter++;
            if (_currentAudioAdConfig != null && _gameStartCounter >= _currentAudioAdConfig.gameStartTriggerFrequency)
            {
                ShowAudioAd();
                _gameStartCounter = 0;
            }
        }

        internal void OnGameFinished(GameFinishedParameters parameters)
        {
            if (_currentAudioAdNetwork != null && _currentAudioAdConfig?.killWhenGameFinishes == true)
            {
                _currentAudioAdNetwork.CloseAudioAd();
            }
        }

        private void OnAudioAdClick()
        {
            UnityThreadExecutor.Execute(() =>
                _audioAdsAnalyticsManager.TrackAudioAdClicked(InitializeAudioAdAnalyticsInfo(new AudioAdAnalyticsInfo())));
        }

        private void OnAudioAdShow()
        {
            UnityThreadExecutor.Execute(() =>
                _audioAdsAnalyticsManager.TrackAudioAdShown(InitializeAudioAdAnalyticsInfo(new AudioAdAnalyticsInfo())));
        }

        private void OnAudioAdImpression(double revenue)
        {
            UnityThreadExecutor.Execute(() =>
            {
                AudioAdImpressionAnalyticsInfo eventParams =
                    InitializeAudioAdAnalyticsInfo(new AudioAdImpressionAnalyticsInfo());
                eventParams.revenue = revenue;
                _audioAdsAnalyticsManager.TrackAudioAdImpression(eventParams);
            });
        }

        private void OnAudioAdWatched()
        {
            UnityThreadExecutor.Execute(() => {
                AnalyticsManager.AudioAds.TrackAudioAdWatched(InitializeAudioAdAnalyticsInfo(new AudioAdAnalyticsInfo()));
            });
        }
        
        private void OnAudioAdUserClose()
        {
            UnityThreadExecutor.Execute(() =>
            {
                AnalyticsManager.AudioAds.TrackAudioAdClosed(InitializeAudioAdAnalyticsInfo(new AudioAdAnalyticsInfo()));
            });
        }

        private void OnAudioAdFailed(string errorCode)
        {
            UnityThreadExecutor.Execute(() =>
            {
                AudioAdFailedAnalyticsInfo eventParams = 
                    InitializeAudioAdAnalyticsInfo(new AudioAdFailedAnalyticsInfo());
                eventParams.errorCode = errorCode;
                AnalyticsManager.AudioAds.TrackAudioAdFailed(eventParams);
            });
        }
#endregion Events

        internal enum State
        {
            Disabled,
            ShowingAd,
            PositionConfigMissing,
            LoadingAd,
            TooEarly,
            Ready,
            Misconfigured
        }
        
    }
}