using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AnalyticsStorageHelper : IAnalyticStorage
    {
        private const string GAME_COUNT_KEY = "VoodooSauce_GameCount";
        private const string SUCCESSFUL_GAME_COUNT_KEY = "VoodooSauce_SuccessfulGameCount";
        private const string CURRENT_LEVEL_KEY = "VoodooSauce_Level";
        private const string APP_LAUNCH_COUNT_KEY = "VoodooSauce_AppLaunchCount";
        private const string HIGHEST_SCORE_KEY = "VoodooSauce_HighScore";
        private const string FIRST_APP_LAUNCH_DATE_KEY = "VoodooSauce_FirstAppLaunchDate";
        //Incremented each time we call Show FS/RV/NativeAds
        private const string SHOW_INTERSTITIAL_COUNT_KEY = "FsCount";
        private const string SHOW_REWARDED_COUNT_KEY = "RvCount";
        private const string SHOW_APP_OPEN_COUNT_KEY = "AoCount";
        private const string SHOW_NATIVE_ADS_COUNT_KEY = "NativeAds_Ad_Count";
        private const string MREC_COUNT_KEY = "MRec_Ad_Count";
        private const string AUDIO_AD_COUNT_KEY = "Audio_Ad_Count";
        //Incremented once the FS/RV is watched 
        private const string INTERSTITIAL_WATCHED_COUNT_KEY = "VoodooSauce_FSShownCount";
        private const string REWARDED_WATCHED_COUNT_KEY = "VoodooSauce_RVShownCount";
        private const string APP_OPEN_WATCHED_COUNT_KEY = "VoodooSauce_AOShownCount";
        //Incremented once CP or BackupFS are shown
        private const string SHOW_CROSS_PROMO_COUNT_KEY = "CrossPromoCount";
        private const string SHOW_BACKUP_FS_COUNT_KEY = "BackupFSCount";
        private const string SHOW_BACKUP_RV_COUNT_KEY = "BackupRVCount";

        //these values are saved only in memory cache
        private const string GAME_ROUND_ID_KEY = "GameRoundId";
        private const string RV_USED_KEY = "RvUsed";

        private static IAnalyticStorage instance;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        internal static IAnalyticStorage Instance => instance ?? (instance = new AnalyticsStorageHelper());

        public void PreLoad()
        {
            //Preload these values to be called from a background thread.
            LoadCacheStorageIntValue(GAME_COUNT_KEY);
            LoadCacheStorageIntValue(SHOW_REWARDED_COUNT_KEY);
            LoadCacheStorageIntValue(SHOW_INTERSTITIAL_COUNT_KEY);
            LoadCacheStorageIntValue(SHOW_APP_OPEN_COUNT_KEY);
            LoadCacheStorageIntValue(MREC_COUNT_KEY);
            LoadCacheStorageIntValue(SHOW_NATIVE_ADS_COUNT_KEY);
            LoadCacheStorageStringValue(CURRENT_LEVEL_KEY);
        }

#if UNITY_EDITOR
        //This will only be used in IntegrationTest since currently, everything is still singleton & static
        internal static void Inject(IAnalyticStorage analyticStorage) => instance = analyticStorage;
#endif

#region CrossPromo analytics storage methods

        public int IncrementShowCrossPromoCount() => IncrementIntValue(SHOW_CROSS_PROMO_COUNT_KEY);
        public int GetShowCrossPromoCount() => GetIntValue(SHOW_CROSS_PROMO_COUNT_KEY);
        public void IncrementShowBackupFsCount() => IncrementIntValue(SHOW_BACKUP_FS_COUNT_KEY);
        public void IncrementShowBackupRvCount() => IncrementIntValue(SHOW_BACKUP_RV_COUNT_KEY);

        public int GetShowBackupFsCount() => GetIntValue(SHOW_BACKUP_FS_COUNT_KEY);
        public int GetShowBackupRvCount() => GetIntValue(SHOW_BACKUP_RV_COUNT_KEY);

#endregion

#region Game analytics storage methods

        public void IncrementGameCount() => IncrementIntValue(GAME_COUNT_KEY);
        public int GetGameCount() => GetIntValue(GAME_COUNT_KEY);
        public int IncrementSuccessfulGameCount() => IncrementIntValue(SUCCESSFUL_GAME_COUNT_KEY);
        public int GetSuccessfulGameCount() => GetIntValue(SUCCESSFUL_GAME_COUNT_KEY);

        public double GetWinRate()
        {
            var gameCount = (double)GetGameCount();
            var successfulGameCount = (double)GetSuccessfulGameCount();
            return Math.Min(gameCount > 0 ? successfulGameCount / gameCount : 0, 1);
        }

        public bool HasWinRate() => PlayerPrefs.HasKey(GAME_COUNT_KEY) && PlayerPrefs.HasKey(SUCCESSFUL_GAME_COUNT_KEY);
        public string GetCurrentLevel() => GetStringValue(CURRENT_LEVEL_KEY);

        public void UpdateCurrentLevel(string level)
        {
            UpdateStringValue(CURRENT_LEVEL_KEY, level);
        }

        public bool HasCurrentLevel() => !string.IsNullOrEmpty(GetCurrentLevel());
        public float GetGameHighestScore() => GetFloatValue(HIGHEST_SCORE_KEY);
        public bool HasGameHighestScore() => PlayerPrefs.HasKey(HIGHEST_SCORE_KEY);

        public bool UpdateGameHighestScore(float score)
        {
            if (score < GetGameHighestScore()) return false;
            UpdateFloatValue(HIGHEST_SCORE_KEY, score);
            return true;
        }

        public string GetGameRoundId() =>
            _cache.ContainsKey(GAME_ROUND_ID_KEY) ? (string)_cache[GAME_ROUND_ID_KEY] : null;

        public string CreateRoundId()
        {
            var value = Guid.NewGuid().ToString();
            _cache[GAME_ROUND_ID_KEY] = value;
            return value;
        }

#endregion

#region App analytics storage methods

        public int GetAppLaunchCount() => GetIntValue(APP_LAUNCH_COUNT_KEY, 1);
        public bool IsFirstAppLaunch() => GetAppLaunchCount() == 1;
        public void IncrementAppLaunchCount() => IncrementIntValue(APP_LAUNCH_COUNT_KEY);

        public DateTime? GetFirstAppLaunchDate()
        {
            int timeStamp = GetIntValue(FIRST_APP_LAUNCH_DATE_KEY);
            return timeStamp > 0 ? TimeUtils.TimeStampToDateTime(timeStamp) : null;
        }

        public void SaveFirstAppLaunchDate() => UpdateIntValue(FIRST_APP_LAUNCH_DATE_KEY, TimeUtils.NowAsTimeStamp());

#endregion

#region Ads analytics storage methods
        public int IncrementAudioAdCount() => IncrementIntValue(AUDIO_AD_COUNT_KEY);
        public int GetAudioAdCount() => GetIntValue(AUDIO_AD_COUNT_KEY);
        public int IncrementShowInterstitialCount() => IncrementIntValue(SHOW_INTERSTITIAL_COUNT_KEY);
        public int GetShowInterstitialCount() => GetIntValue(SHOW_INTERSTITIAL_COUNT_KEY);
        public int IncrementShowRewardedVideoCount() => IncrementIntValue(SHOW_REWARDED_COUNT_KEY);
        public int GetShowRewardedVideoCount() => GetIntValue(SHOW_REWARDED_COUNT_KEY);
        public int IncrementShowAppOpenCount() => IncrementIntValue(SHOW_APP_OPEN_COUNT_KEY);
        public int GetShowAppOpenCount() => GetIntValue(SHOW_APP_OPEN_COUNT_KEY);
        public int IncrementMrecCount() => IncrementIntValue(MREC_COUNT_KEY);
        public int GetMrecCount() => GetIntValue(MREC_COUNT_KEY);
        public int IncrementShowNativeAdsCount() => IncrementIntValue(SHOW_NATIVE_ADS_COUNT_KEY);
        public int GetShowNativeAdsCount() => GetIntValue(SHOW_NATIVE_ADS_COUNT_KEY);
        public int IncrementInterstitialWatchedCount() => IncrementIntValue(INTERSTITIAL_WATCHED_COUNT_KEY);
        public int GetInterstitialWatchedCount() => GetIntValue(INTERSTITIAL_WATCHED_COUNT_KEY);
        public int IncrementRewardedVideoWatchedCount() => IncrementIntValue(REWARDED_WATCHED_COUNT_KEY);
        public int GetRewardedVideoWatchedCount() => GetIntValue(REWARDED_WATCHED_COUNT_KEY);
        public int IncrementAppOpenWatchedCount() => IncrementIntValue(APP_OPEN_WATCHED_COUNT_KEY);
        public int GetAppOpenWatchedCount() => GetIntValue(APP_OPEN_WATCHED_COUNT_KEY);

        public void IncreaseRvUsed() => _cache[RV_USED_KEY] = GetRvUsed() + 1;
        public void ResetRvUsed() => _cache[RV_USED_KEY] = 0;

        public int GetRvUsed()
        {
            if (_cache.ContainsKey(RV_USED_KEY)) return (int)_cache[RV_USED_KEY];
            _cache[RV_USED_KEY] = 0;
            return 0;
        }

#endregion

#region Generic analytics storage methods

        private int IncrementIntValue(string key, int defaultValue = 0)
        {
            int value = PlayerPrefs.GetInt(key, defaultValue) + 1;
            _cache[key] = value;
            PlayerPrefs.SetInt(key, value);
            return value;
        }

        private void UpdateIntValue(string key, int value)
        {
            _cache[key] = value;
            PlayerPrefs.SetInt(key, value);
        }

        private int GetIntValue(string key)
        {
            if (!_cache.ContainsKey(key)) {
                return LoadCacheStorageIntValue(key);
            }

            return (int)_cache[key];
        }

        private int GetIntValue(string key, int @default)
        {
            if (!_cache.ContainsKey(key))
            {
                return LoadCacheStorageIntValue(key, @default);
            }

            return (int)_cache[key];
        }

        private int LoadCacheStorageIntValue(string key)
        {
            int storedValue = PlayerPrefs.GetInt(key);
            _cache[key] = storedValue;
            return storedValue;
        }

        private int LoadCacheStorageIntValue(string key, int @default)
        {
            int storedValue = PlayerPrefs.GetInt(key, @default);
            _cache[key] = storedValue;
            return storedValue;
        }

        private void UpdateFloatValue(string key, float value)
        {
            _cache[key] = value;
            PlayerPrefs.SetFloat(key, value);
        }

        private float GetFloatValue(string key)
        {
            //always false for preloaded values
            if (!_cache.ContainsKey(key)) {
                return LoadCacheStorageFloatValue(key);
            }

            return (float)_cache[key];
        }

        private float LoadCacheStorageFloatValue(string key)
        {
            float storedValue = PlayerPrefs.GetFloat(key);
            _cache[key] = storedValue;
            return storedValue;
        }

        private void UpdateStringValue(string key, string value)
        {
            _cache[key] = value;
            PlayerPrefs.SetString(key, value);
        }

        private string GetStringValue(string key)
        {
            //always false for preloaded values
            if (!_cache.ContainsKey(key)) {
                return LoadCacheStorageStringValue(key);
            }

            return (string)_cache[key];
        }

        private string LoadCacheStorageStringValue(string key)
        {
            string storedValue = PlayerPrefs.GetString(key);
            _cache[key] = storedValue;
            return storedValue;
        }

#endregion
    }
}