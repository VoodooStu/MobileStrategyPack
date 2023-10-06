using System;
using UnityEngine;
using Voodoo.Sauce.Tools.AccessButton;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public static class AdsDebugManager
    {
        private const string SHOW_INTERSTITIAL_AD_BADGE_KEY = "Voodoo_DebugFSBadge";
        private const string SHOW_REWARDED_VIDEO_AD_BADGE_KEY = "Voodoo_DebugRVBadge";
        private const int DEFAULT_ENABLED_VALUE = 1; // 1 if the ad badges must be enabled by default, else 0.

        private static bool _initialized = false;

        public static Action interstitialAdStateBadgeChanged;
        public static Action rewardedVideoAdStateBadgeChanged;

        private static bool _isInterstitialAdStateBadgeEnabled;
        public static bool IsInterstitialAdStateBadgeEnabled {
            get {
                Initialize();
                return _isInterstitialAdStateBadgeEnabled;
            }
            set {
                _isInterstitialAdStateBadgeEnabled = value;
                PlayerPrefs.SetInt(SHOW_INTERSTITIAL_AD_BADGE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                interstitialAdStateBadgeChanged?.Invoke();
            }
        }

        private static bool _isRewardedVideoAdStateBadgeEnabled;
        public static bool IsRewardedVideoAdStateBadgeEnabled {
            get {
                Initialize();
                return _isRewardedVideoAdStateBadgeEnabled;
            }
            set {
                _isRewardedVideoAdStateBadgeEnabled = value;
                PlayerPrefs.SetInt(SHOW_REWARDED_VIDEO_AD_BADGE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                rewardedVideoAdStateBadgeChanged?.Invoke();
            }
        }

        private static void Initialize()
        {
            if (_initialized) {
                return;
            }
            
            _isInterstitialAdStateBadgeEnabled = PlayerPrefs.GetInt(SHOW_INTERSTITIAL_AD_BADGE_KEY, DEFAULT_ENABLED_VALUE) == 1;
            _isRewardedVideoAdStateBadgeEnabled = PlayerPrefs.GetInt(SHOW_REWARDED_VIDEO_AD_BADGE_KEY, DEFAULT_ENABLED_VALUE) == 1;

            AccessProcess.InstantiateAccessButton += () => {
                IsInterstitialAdStateBadgeEnabled = true;
                IsRewardedVideoAdStateBadgeEnabled = true;
            };

            AccessProcess.DisposeAccessButton += () => {
                IsInterstitialAdStateBadgeEnabled = false;
                IsRewardedVideoAdStateBadgeEnabled = false;
            };
            
            _initialized = true;
        }
    } 
}