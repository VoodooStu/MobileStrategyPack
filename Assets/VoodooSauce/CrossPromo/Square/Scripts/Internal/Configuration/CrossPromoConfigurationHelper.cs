using System;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Internal.VoodooTune;

namespace Voodoo.Sauce.Internal.CrossPromo.Configuration
{
    internal class CrossPromoConfigurationHelper
    {
        private CrossPromoConfiguration _configuration;

        private int _impressionsCount;

        private int _lastSessionCountWhenShown = -1;
        private int LastSessionCountWhenShown {
            get {
                if (_lastSessionCountWhenShown == -1)
                    _lastSessionCountWhenShown = PlayerPrefs.GetInt(K_PREFS_LAST_SESSION_SHOWN, 0);
                return _lastSessionCountWhenShown;
            }
            set {
                _lastSessionCountWhenShown = value;
                PlayerPrefs.SetInt(K_PREFS_LAST_SESSION_SHOWN, _lastSessionCountWhenShown);
                PlayerPrefs.Save();
            }
        }

        private int _lastGamePlayedWhenShown;
        private int _gamesPlayed;
        private bool _crossPromoEnabled;
        private int _currentSessionCooldown;

        private const string K_PREFS_LAST_SESSION_SHOWN = "CrossPromo_LastSessionShown";

        private int SessionCount => AnalyticsSessionManager.Instance().SessionInfo.count;
        private int GameCount => AnalyticsStorageHelper.Instance.GetGameCount();

        internal enum ShouldBeShownReason
        {
            CanBeShown,
            SessionTooEarly,
            SessionCooldown,
            GameTooEarly,
            GameCooldown,
            TooManyImpressions,
            NotEnabled
        }

        internal void Initialize(CrossPromoConfiguration configuration, bool crossPromoEnabled)
        {
            _configuration = configuration ?? new CrossPromoConfiguration();
            _crossPromoEnabled = crossPromoEnabled;

            _impressionsCount = 0;
            _gamesPlayed = 0;
            _lastGamePlayedWhenShown = 0;

            _currentSessionCooldown = SessionCount - LastSessionCountWhenShown;

            if (ShouldBeEnabled()) {
                AnalyticsManager.OnGamePlayed += OnGamePlayed;
                CrossPromoEvents.OnShown += OnShown;
                CrossPromoEvents.OnClick += OnClick;
            }
        }

        internal bool ShouldBeEnabled() => _crossPromoEnabled && _configuration.enabled;

        internal ShouldBeShownReason GetShouldBeShownReason() =>
            GetShouldBeShownReason(SessionCount, _currentSessionCooldown, _gamesPlayed, _lastGamePlayedWhenShown, _impressionsCount,
                LastSessionCountWhenShown, _lastGamePlayedWhenShown);

        internal ShouldBeShownReason GetShouldBeShownReason(int sessionsCount,
                                                            int sessionsCooldown,
                                                            int gamesPlayed,
                                                            int lastGamesPlayed,
                                                            int impressionsCount,
                                                            int lastSessionCountWhenShown = 0,
                                                            int lastGamePlayedWhenShown = 0)
        {
            if (!ShouldBeEnabled())
                return ShouldBeShownReason.NotEnabled;

            if (_configuration.impressionsCountBeforeHiding > 0 && impressionsCount > _configuration.impressionsCountBeforeHiding)
                return ShouldBeShownReason.TooManyImpressions;

            if (lastSessionCountWhenShown == 0 && sessionsCount < _configuration.sessionsCountBeforeShowing)
                return ShouldBeShownReason.SessionTooEarly;

            if (sessionsCooldown < _configuration.sessionsCountBetweenShowing)
                return ShouldBeShownReason.SessionCooldown;

            if (lastGamePlayedWhenShown == 0 && gamesPlayed < _configuration.gamesCountBeforeShowing)
                return ShouldBeShownReason.GameTooEarly;

            int gameCooldown = Mathf.Abs(gamesPlayed - lastGamesPlayed);
            if (gameCooldown < _configuration.gamesCountBetweenShowing)
                return ShouldBeShownReason.GameCooldown;

            return ShouldBeShownReason.CanBeShown;
        }

        internal bool ShouldBeShown() => GetShouldBeShownReason() == ShouldBeShownReason.CanBeShown;

        internal string GetGamesToShow()
        {
            if (!ShouldBeEnabled())
                return "";

            return _configuration.gamesToShow.Replace(" ", "");
        }

        private void OnClick(AssetModel obj)
        {
            _impressionsCount = 0;
        }

        private void OnShown(AssetModel obj)
        {
            LastSessionCountWhenShown = SessionCount;
            _lastGamePlayedWhenShown = _gamesPlayed;
            _impressionsCount++;
        }

        private void OnGamePlayed(int level, bool victory)
        {
            _gamesPlayed++;
        }

        internal string GetCurrentStateDebugText()
        {
            int sessionCount = SessionCount;
            int gameCount = GameCount;
            int imps = _impressionsCount;

            int gameCooldown = _gamesPlayed - _lastGamePlayedWhenShown;

            string text = $"session: {sessionCount} - games: {gameCount} - imps: {imps}" + Environment.NewLine;
            text += $"sessionCooldown: {_currentSessionCooldown} - gameCooldown: {gameCooldown}" + Environment.NewLine;
            text += $"enabled: {ShouldBeEnabled()} - waterfallId: {GetWaterfallId()}" + Environment.NewLine;
            text += $"Shown Reason: {GetShouldBeShownReason()}" + Environment.NewLine;

            return text;
        }

        internal string GetCurrentConfigurationDebugText() => _configuration?.ToString() ?? "";

        internal string GetWaterfallId()
        {
            if (!string.IsNullOrEmpty(_configuration?.waterfallId))
                return _configuration.waterfallId;

            string cohortUuid = VoodooTuneManager.GetMainAbTestCohortUuid();
            if (!string.IsNullOrEmpty(cohortUuid))
                return VoodooTuneManager.GetMainAbTestCohortUuid();

            return "no_waterfall";
        }
    }
}