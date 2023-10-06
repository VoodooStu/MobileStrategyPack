using System;
using System.Globalization;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public class AdDisplayConditions
    {
#region Enums

        internal enum AdConditionState
        {
            FirstInterstitialReadyToShow,
            FirstInterstitialTooEarly,
            InterstitialTooEarlyAndNotEnoughGamesPlayed,
            EnoughGamesWerePlayed,
            RewardedVideoWasPlayedRecently,
            InterstitialReadyToShow,
            InterstitialWasPlayedRecently,
            AppOpenWasPlayedRecently,
            AppOpenReadyToShow,
            AppOpenTooEarly,
            Disabled
        }

#endregion

#region Constants
        
        private const string TAG = "AdDisplayConditions";

        private const string PREFS_FIRST_APP_LAUNCH = "VoodooSauce.Interstitial.FirstAppLaunch";

        private const int DEFAULT_DELAY_IN_SECONDS_BEFORE_FIRST_INTERSTITIAL_AD = 30;
        private const int DEFAULT_DELAY_IN_SECONDS_BEFORE_SESSION_FIRST_INTERSTITIAL_AD = -1;
        private const int DEFAULT_DELAY_IN_SECONDS_BETWEEN_INTERSTITIAL_ADS = 30;
        private const int DEFAULT_MAX_GAMES_BETWEEN_INTERSTITIAL_ADS = 3;
        private const int DEFAULT_DELAY_IN_SECONDS_BETWEEN_REWARDED_VIDEO_AND_INTERSTITIAL = 5;

        private const float DEFAULT_DELAY_IN_SECONDS_IN_BACKGROUND_BEFORE_APP_OPEN_AD = 60;
        private const float DEFAULT_DELAY_IN_SECONDS_BETWEEN_APP_OPEN_ADS = 60;
        private const float DEFAULT_DELAY_IN_SECONDS_BETWEEN_INTERSTITIAL_AND_APP_OPEN_AD = 0;
        private const float DEFAULT_DELAY_IN_SECONDS_BETWEEN_REWARDED_VIDEO_AND_APP_OPEN_AD = 5;
        private const float DEFAULT_DELAY_IN_SECONDS_BETWEEN_APP_OPEN_AD_AND_INTERSTITIAL = 0;

#endregion

#region Properties

        private readonly int _delayInSecondsBeforeFirstInterstitialAd;
        private readonly int _delayInSecondsBeforeSessionFirstInterstitial;
        private readonly int _delayInSecondsBetweenInterstitialAds;
        private readonly int _maxGamesPlayedBetweenInterstitials;
        private readonly int _delayInSecondsBetweenRewardedVideoAndInterstitial;

        private readonly float _delayInSecondsInBackgroundBeforeAppOpenAd;
        private readonly float _delayInSecondsBetweenAppOpenAds;
        private readonly float _delayInSecondsBetweenInterstitialAndAppOpenAd;
        private readonly float _delayInSecondsBetweenRewardedVideoAndAppOpenAd;
        private readonly float _delayInSecondsBetweenAppOpenAdAndInterstitial;

        private bool _firstEverInterstitialDisplayed;
        private bool _firstSessionInterstitialDisplayed;
        private int _gamesPlayedSinceLastInterstitial;
        private float? _lastInterstitialTime;

        private readonly bool _rewardedVideosDelayFullscreen;
        private float? _lastRewardedVideoTime;

        private bool _appOpenAdEnabled;
        private float? _lastAppOpenTime;

        private float _amountOfTimeInBackground;
        private float? _backgroundStartTime;

#endregion

#region Constructors

        public AdDisplayConditions(float delayInSecondsInBackgroundBeforeAppOpenAd,
                                   float delayInSecondsBetweenAppOpenAds,
                                   float delayInSecondsBetweenInterstitialAndAppOpenAd,
                                   float delayInSecondsBetweenRewardedVideoAndAppOpenAd,
                                   float delayInSecondsBetweenAppOpenAdAndInterstitial) :
            this(
                DEFAULT_DELAY_IN_SECONDS_BEFORE_FIRST_INTERSTITIAL_AD,
                DEFAULT_DELAY_IN_SECONDS_BEFORE_SESSION_FIRST_INTERSTITIAL_AD,
                DEFAULT_DELAY_IN_SECONDS_BETWEEN_INTERSTITIAL_ADS,
                DEFAULT_MAX_GAMES_BETWEEN_INTERSTITIAL_ADS,
                DEFAULT_DELAY_IN_SECONDS_BETWEEN_REWARDED_VIDEO_AND_INTERSTITIAL,
                delayInSecondsInBackgroundBeforeAppOpenAd,
                delayInSecondsBetweenAppOpenAds,
                delayInSecondsBetweenInterstitialAndAppOpenAd,
                delayInSecondsBetweenRewardedVideoAndAppOpenAd,
                delayInSecondsBetweenAppOpenAdAndInterstitial
            ) { }

        public AdDisplayConditions(AdDisplayConditions currentAdDisplayConditions,
                                   int delayInSecondsBeforeFirstInterstitialAd,
                                   int delayInSecondsBeforeSessionFirstInterstitial,
                                   int delayInSecondsBetweenInterstitialAds,
                                   int maxGamesBetweenInterstitialAds,
                                   int delayInSecondsBetweenRewardedVideoAndInterstitial,
                                   float delayInSecondsBetweenAppOpenAdAndInterstitial) :
            this(
                delayInSecondsBeforeFirstInterstitialAd,
                delayInSecondsBeforeSessionFirstInterstitial,
                delayInSecondsBetweenInterstitialAds,
                maxGamesBetweenInterstitialAds,
                delayInSecondsBetweenRewardedVideoAndInterstitial,
                currentAdDisplayConditions?._delayInSecondsInBackgroundBeforeAppOpenAd ?? -1,
                currentAdDisplayConditions?._delayInSecondsBetweenAppOpenAds ?? -1,
                currentAdDisplayConditions?._delayInSecondsBetweenInterstitialAndAppOpenAd ?? -1,
                currentAdDisplayConditions?._delayInSecondsBetweenRewardedVideoAndAppOpenAd ?? -1,
                delayInSecondsBetweenAppOpenAdAndInterstitial,
                currentAdDisplayConditions?._appOpenAdEnabled ?? false
            ) { }

        public AdDisplayConditions(int delayInSecondsBeforeFirstInterstitialAdAd,
            int delayInSecondsBeforeSessionFirstInterstitial,
            int delayInSecondsBetweenInterstitialAds,
            int maxGamesBetweenInterstitialAds, int delayInSecondsBetweenRewardedVideoAndInterstitial,
            float delayInSecondsInBackgroundBeforeAppOpenAd, float delayInSecondsBetweenAppOpenAds,
            float delayInSecondsBetweenInterstitialAndAppOpenAd, float delayInSecondsBetweenRewardedVideoAndAppOpenAd,
            float delayInSecondsBetweenAppOpenAdAndInterstitial,
            bool appOpenAdEnabled = false)
        {
            _delayInSecondsBeforeFirstInterstitialAd = delayInSecondsBeforeFirstInterstitialAdAd;
            _delayInSecondsBeforeSessionFirstInterstitial = delayInSecondsBeforeSessionFirstInterstitial;
            _delayInSecondsBetweenInterstitialAds = delayInSecondsBetweenInterstitialAds;
            _maxGamesPlayedBetweenInterstitials = maxGamesBetweenInterstitialAds;

            _delayInSecondsBetweenRewardedVideoAndInterstitial = delayInSecondsBetweenRewardedVideoAndInterstitial;
            if (_delayInSecondsBetweenRewardedVideoAndInterstitial == -1)
            {
                _delayInSecondsBetweenRewardedVideoAndInterstitial =
                    DEFAULT_DELAY_IN_SECONDS_BETWEEN_REWARDED_VIDEO_AND_INTERSTITIAL;
            }

            _delayInSecondsInBackgroundBeforeAppOpenAd = delayInSecondsInBackgroundBeforeAppOpenAd;
            if (_delayInSecondsInBackgroundBeforeAppOpenAd < 0)
            {
                _delayInSecondsInBackgroundBeforeAppOpenAd =
                    DEFAULT_DELAY_IN_SECONDS_IN_BACKGROUND_BEFORE_APP_OPEN_AD;
            }

            _delayInSecondsBetweenAppOpenAds = delayInSecondsBetweenAppOpenAds;
            if (_delayInSecondsBetweenAppOpenAds < 0)
            {
                _delayInSecondsBetweenAppOpenAds =
                    DEFAULT_DELAY_IN_SECONDS_BETWEEN_APP_OPEN_ADS;
            }

            _delayInSecondsBetweenInterstitialAndAppOpenAd = delayInSecondsBetweenInterstitialAndAppOpenAd;
            if (_delayInSecondsBetweenInterstitialAndAppOpenAd < 0)
            {
                _delayInSecondsBetweenInterstitialAndAppOpenAd =
                    DEFAULT_DELAY_IN_SECONDS_BETWEEN_INTERSTITIAL_AND_APP_OPEN_AD;
            }

            _delayInSecondsBetweenRewardedVideoAndAppOpenAd = delayInSecondsBetweenRewardedVideoAndAppOpenAd;
            if (_delayInSecondsBetweenRewardedVideoAndAppOpenAd < 0)
            {
                _delayInSecondsBetweenRewardedVideoAndAppOpenAd =
                    DEFAULT_DELAY_IN_SECONDS_BETWEEN_REWARDED_VIDEO_AND_APP_OPEN_AD;
            }

            _delayInSecondsBetweenAppOpenAdAndInterstitial = delayInSecondsBetweenAppOpenAdAndInterstitial;
            if (_delayInSecondsBetweenAppOpenAdAndInterstitial < 0)
            {
                _delayInSecondsBetweenAppOpenAdAndInterstitial =
                    DEFAULT_DELAY_IN_SECONDS_BETWEEN_APP_OPEN_AD_AND_INTERSTITIAL;
            }

            _firstEverInterstitialDisplayed = PlayerPrefs.HasKey(PREFS_FIRST_APP_LAUNCH);
            _gamesPlayedSinceLastInterstitial = 0;
            _lastInterstitialTime = null; // Do not initialize with Time.unscaledTime because no interstitial was displayed yet!
            _lastRewardedVideoTime = null; // Do not initialize with Time.unscaledTime because no rewarded video was displayed yet!
            _lastAppOpenTime = null; // Do not initialize with Time.unscaledTime because no appOpen was displayed yet!
            _amountOfTimeInBackground = 0;
            _backgroundStartTime = null;
            _rewardedVideosDelayFullscreen = true;
            _appOpenAdEnabled = appOpenAdEnabled;

            VoodooLog.LogDebug(Module.ADS, TAG, $"{InterstitialConditionsToString()} (first interstitial: {_firstEverInterstitialDisplayed})");
            VoodooLog.LogDebug(Module.ADS, TAG, AppOpenConditionsToString());
        }

#endregion

#region Methods

        public void IncrementGamesPlayed()
        {
            _gamesPlayedSinceLastInterstitial++;
        }

        public void OnApplicationPause(bool pauseStatus, bool isAdShowing)
        {
            // Only the "real" background mode is taken in account.
            // When a full screen ad is showing, the application is paused too
            // but we don't want this time to be taken in account.
            if (isAdShowing) {
                return;
            }
            
            if (pauseStatus) {
                _backgroundStartTime = Time.realtimeSinceStartup;
                VoodooLog.LogDebug(Module.ADS, TAG, "Start pause timer");
            } else if (_backgroundStartTime != null) {
                _amountOfTimeInBackground = (float)(Time.realtimeSinceStartup - _backgroundStartTime);
                _backgroundStartTime = null;
                VoodooLog.LogDebug(Module.ADS, TAG, $"Stop pause timer: total {_amountOfTimeInBackground.ToString("0.##", CultureInfo.CreateSpecificCulture("en-US"))} s.");
            }
        }

        public void ResetBackgroundTimer()
        {
            _amountOfTimeInBackground = 0;
            VoodooLog.LogDebug(Module.ADS, TAG, "Reset pause timer");
        }
        
#endregion

#region Rewarded Video

        public void RewardedVideoDisplayed(bool completed)
        {
            if (!completed || !_rewardedVideosDelayFullscreen) return;
            
            VoodooLog.LogDebug(Module.ADS, TAG,
                $"Rewarded Videos Delay Fullscreen is enabled. Delaying the next interstitial by {_delayInSecondsBetweenRewardedVideoAndInterstitial}...");
            _lastRewardedVideoTime = Time.unscaledTime;
        }

#endregion

#region Interstitial
        
        public string InterstitialConditionsToString() =>
            $"Interstitial conditions set to: {_delayInSecondsBeforeFirstInterstitialAd}/{_delayInSecondsBeforeSessionFirstInterstitial}/{_delayInSecondsBetweenInterstitialAds}/{_maxGamesPlayedBetweenInterstitials}";

        public void InterstitialDisplayed()
        {
            if (!_firstEverInterstitialDisplayed) {
                _firstEverInterstitialDisplayed = true;
                PlayerPrefs.SetInt(PREFS_FIRST_APP_LAUNCH, 0);
            }

            _firstSessionInterstitialDisplayed = true;
            _gamesPlayedSinceLastInterstitial = 0;
            _lastInterstitialTime = Time.unscaledTime;
        }

        public bool AreInterstitialConditionsMet()
        {
            AdConditionState state = GetCurrentInterstitialConditionState();
            
            VoodooLog.LogDebug(Module.ADS, TAG, $"Are interstitial conditions met? {state.ToString()} - {GetInterstitialConditionTimeString()}");

            return state == AdConditionState.EnoughGamesWerePlayed ||
                state == AdConditionState.FirstInterstitialReadyToShow ||
                state == AdConditionState.InterstitialReadyToShow;
        }

        internal AdConditionState GetCurrentInterstitialConditionState()
        {
            // Time since last rewarded video check
            if (_rewardedVideosDelayFullscreen && _lastRewardedVideoTime != null &&
                Time.unscaledTime - _lastRewardedVideoTime < _delayInSecondsBetweenRewardedVideoAndInterstitial) {
                return AdConditionState.RewardedVideoWasPlayedRecently;
            }

            // Time since last app open
            if (_appOpenAdEnabled && _delayInSecondsBetweenAppOpenAdAndInterstitial > 0 && _lastAppOpenTime != null &&
                Time.unscaledTime - _lastAppOpenTime < _delayInSecondsBetweenAppOpenAdAndInterstitial) {
                return AdConditionState.AppOpenWasPlayedRecently;
            }

            // First-ever interstitial checks
            if (!_firstEverInterstitialDisplayed && _delayInSecondsBeforeFirstInterstitialAd >= 0)
            {
                return Time.unscaledTime < _delayInSecondsBeforeFirstInterstitialAd
                    ? AdConditionState.FirstInterstitialTooEarly
                    : AdConditionState.FirstInterstitialReadyToShow;
            }

            // If no interstitial ad hasn't been displayed in this session,
            // and _delayInSecondsBeforeSessionFirstInterstitial is enabled,
            // this delay is used.
            if (!_firstSessionInterstitialDisplayed && _delayInSecondsBeforeSessionFirstInterstitial >= 0)
            {
                return Time.unscaledTime < _delayInSecondsBeforeSessionFirstInterstitial
                    ? AdConditionState.FirstInterstitialTooEarly
                    : AdConditionState.FirstInterstitialReadyToShow;
            }

            // Timer check
            if (Time.unscaledTime - (_lastInterstitialTime ?? 0) >= _delayInSecondsBetweenInterstitialAds) {
                return !_firstSessionInterstitialDisplayed ? AdConditionState.FirstInterstitialReadyToShow: AdConditionState.InterstitialReadyToShow;
            }

            // No number of games check for the first FS.
            if (!_firstSessionInterstitialDisplayed)
            {
                return AdConditionState.FirstInterstitialTooEarly;
            }
            
            // Number of games played check
            return _gamesPlayedSinceLastInterstitial >= _maxGamesPlayedBetweenInterstitials
                ? AdConditionState.EnoughGamesWerePlayed
                : AdConditionState.InterstitialTooEarlyAndNotEnoughGamesPlayed;
        }

        public string GetInterstitialConditionTimeString()
        {
            AdConditionState state = GetCurrentInterstitialConditionState();
            
            float timeSinceGameStarted = Time.unscaledTime;
            var timeSinceLastInterstitial = (float)(_lastInterstitialTime != null ? Time.unscaledTime - _lastInterstitialTime : 0);
            var timeSinceLastRewarded = (float)(_lastRewardedVideoTime != null ? Time.unscaledTime - _lastRewardedVideoTime : 0);
            var timeSinceLastAppOpen = (float)(_lastAppOpenTime != null ? Time.unscaledTime - _lastAppOpenTime - _amountOfTimeInBackground : 0);
            int delayInterstitial = _delayInSecondsBetweenInterstitialAds;
            if (!_firstEverInterstitialDisplayed && _delayInSecondsBeforeFirstInterstitialAd >= 0)
            {
                delayInterstitial = _delayInSecondsBeforeFirstInterstitialAd;
            } else if (!_firstSessionInterstitialDisplayed && _delayInSecondsBeforeSessionFirstInterstitial >= 0)
            {
                delayInterstitial = _delayInSecondsBeforeSessionFirstInterstitial;
            }
            switch (state) {
                case AdConditionState.FirstInterstitialReadyToShow:
                    return $"{timeSinceGameStarted:0}s/{delayInterstitial : 0}s";

                case AdConditionState.FirstInterstitialTooEarly:
                    return $"{timeSinceGameStarted:0}s/{delayInterstitial : 0}s";

                case AdConditionState.InterstitialTooEarlyAndNotEnoughGamesPlayed:
                    return
                        $"{timeSinceLastInterstitial:0}s/{delayInterstitial : 0}s - {_gamesPlayedSinceLastInterstitial:0}/{_maxGamesPlayedBetweenInterstitials:0}";

                case AdConditionState.RewardedVideoWasPlayedRecently:
                    return $"{timeSinceLastRewarded:0}s/{_delayInSecondsBetweenRewardedVideoAndInterstitial:0}s";

                case AdConditionState.AppOpenWasPlayedRecently:
                    return $"{timeSinceLastAppOpen:0}s/{_delayInSecondsBetweenAppOpenAdAndInterstitial:0}s";

                case AdConditionState.InterstitialReadyToShow:
                    return $"{timeSinceLastInterstitial:0}s/{delayInterstitial:0}s";

                case AdConditionState.EnoughGamesWerePlayed:
                    return
                        $"{timeSinceLastInterstitial:0}s/{delayInterstitial:0}s - {_gamesPlayedSinceLastInterstitial:0}/{_maxGamesPlayedBetweenInterstitials:0}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public string GetInterstitialConditionStatusString() => GetCurrentInterstitialConditionState().ToString();
        
#endregion

#region AppOpen

        public string AppOpenConditionsToString() =>
            $"AppOpen conditions set to {_delayInSecondsInBackgroundBeforeAppOpenAd}/{_delayInSecondsBetweenAppOpenAds}/{_delayInSecondsBetweenInterstitialAndAppOpenAd}/{_delayInSecondsBetweenRewardedVideoAndAppOpenAd}/{_delayInSecondsBetweenAppOpenAdAndInterstitial}";

        public void AppOpenDisplayed()
        {
            _lastAppOpenTime = Time.unscaledTime;
        }

        internal void EnableAppOpenAd()
        {
            _appOpenAdEnabled = true;
        }
        
        internal void DisableAppOpenAd()
        {
            _appOpenAdEnabled = false;
        }

        public bool AreAppOpenConditionsMet()
        {
            AdConditionState state = GetCurrentAppOpenConditionState();
            
            VoodooLog.LogDebug(Module.ADS, TAG, $"Are app open conditions met? {state.ToString()} - {GetAppOpenConditionTimeString()}");

            return state == AdConditionState.AppOpenReadyToShow;
        }

        internal AdConditionState GetCurrentAppOpenConditionState()
        {
            // Enabled state
            if (!_appOpenAdEnabled) {
                return AdConditionState.Disabled;
            }
            
            // Time in background
            if (_delayInSecondsInBackgroundBeforeAppOpenAd > 0 &&
                _amountOfTimeInBackground < _delayInSecondsInBackgroundBeforeAppOpenAd) {
                return AdConditionState.AppOpenTooEarly;
            }

            // Time since last app open (the time in background isn’t counted)
            if (_delayInSecondsBetweenAppOpenAds > 0 && _lastAppOpenTime != null &&
                (Time.unscaledTime - _lastAppOpenTime - _amountOfTimeInBackground) < _delayInSecondsBetweenAppOpenAds) {
                return AdConditionState.AppOpenWasPlayedRecently;
            }

            // Time since last interstitial (the time in background isn’t counted)
            if (_delayInSecondsBetweenInterstitialAndAppOpenAd > 0 &&
                (Time.unscaledTime - _lastInterstitialTime - _amountOfTimeInBackground) < _delayInSecondsBetweenInterstitialAndAppOpenAd) {
                return AdConditionState.InterstitialWasPlayedRecently;
            }

            // Time since last rewarded video (the time in background isn’t counted)
            if (_delayInSecondsBetweenRewardedVideoAndAppOpenAd > 0 &&
                (Time.unscaledTime - _lastRewardedVideoTime - _amountOfTimeInBackground)  < _delayInSecondsBetweenRewardedVideoAndAppOpenAd) {
                return AdConditionState.RewardedVideoWasPlayedRecently;
            }

            return AdConditionState.AppOpenReadyToShow;
        }

        public string GetAppOpenConditionTimeString()
        {
            AdConditionState state = GetCurrentAppOpenConditionState();

            float delay;
            
            switch (state) {
                case AdConditionState.Disabled:
                    return "";
                
                case AdConditionState.AppOpenTooEarly:
                    return $"{_amountOfTimeInBackground:0}s/{_delayInSecondsInBackgroundBeforeAppOpenAd:0}s";

                case AdConditionState.AppOpenWasPlayedRecently:
                    delay = (float)(_lastAppOpenTime != null ? Time.unscaledTime - _lastAppOpenTime - _amountOfTimeInBackground : 0);
                    return $"{delay:0}s/{_delayInSecondsBetweenAppOpenAds:0}s";

                case AdConditionState.InterstitialWasPlayedRecently:
                    delay = (float)(_lastInterstitialTime != null ? Time.unscaledTime - _lastInterstitialTime - _amountOfTimeInBackground : 0);
                    return $"{delay:0}s/{_delayInSecondsBetweenInterstitialAndAppOpenAd:0}s";

                case AdConditionState.RewardedVideoWasPlayedRecently:
                    delay = (float)(_lastRewardedVideoTime != null ? Time.unscaledTime - _lastRewardedVideoTime - _amountOfTimeInBackground : 0);
                    return $"{delay:0}s/{_delayInSecondsBetweenRewardedVideoAndInterstitial:0}s";

                case AdConditionState.AppOpenReadyToShow:
                    return $"{_amountOfTimeInBackground:0}s/{_delayInSecondsInBackgroundBeforeAppOpenAd:0}s";
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public string GetAppOpenConditionStatusString() => GetCurrentAppOpenConditionState().ToString();

#endregion

#region Test functions

        // The following functions must be used only for the unit/integration tests.
        
        internal void IncrementBackgroundTime(float seconds)
        {
            _amountOfTimeInBackground += seconds;
        }

        internal void ResetLastAppOpenTime()
        {
            _lastAppOpenTime = null;
        }
        
        internal void SetLastAppOpenTime(float secondsAgo)
        {
            _lastAppOpenTime = Time.unscaledTime - secondsAgo;
        }

        internal void ResetLastInterstitialTime()
        {
            _lastInterstitialTime = null;
            _firstSessionInterstitialDisplayed = false;
        }
        
        internal void SetLastInterstitialTime(float secondsAgo)
        {
            _lastInterstitialTime = Time.unscaledTime - secondsAgo;
            _firstSessionInterstitialDisplayed = true;
        }

        internal void ResetLastRewardedVideoTime()
        {
            _lastRewardedVideoTime = null;
        }
        
        internal void SetLastRewardedVideoTime(float secondsAgo)
        {
            _lastRewardedVideoTime = Time.unscaledTime - secondsAgo;
        }
        
        internal void ResetGamesPlayed()
        {
            _gamesPlayedSinceLastInterstitial = 0;
        }
        
        internal void SetGamesPlayed(int gamesPlayed)
        {
            _gamesPlayedSinceLastInterstitial = gamesPlayed;
        }
        
        internal void SetFirstEverInterstitialDisplayed()
        {
            _firstEverInterstitialDisplayed = true;
            PlayerPrefs.SetInt(PREFS_FIRST_APP_LAUNCH, 0);
        }
        
        internal void ResetFirstEverInterstitialDisplayed()
        {
            _firstEverInterstitialDisplayed = false;
            PlayerPrefs.DeleteKey(PREFS_FIRST_APP_LAUNCH);
        }

#endregion
        
    }
}