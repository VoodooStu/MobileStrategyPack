using UnityEngine;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.CrossPromo;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.IAP
{
    internal static class VoodooPremium
    {
        private const string TAG = "VoodooPremium";
        private const string PREFS_PREMIUM = "VoodooSauce.Premium";
        private const string PREMIUM_PERIOD = "VoodooSauce.PremiumPeriod";

        public static void SetEnabledPremium(bool enabled = true)
        {
            if (enabled == IsIAPPremium())
            {
                if (!enabled)
                {
                    VoodooLog.LogError(Module.IAP, TAG,
                        "User is already premium, you should not be calling this method more than once, when the " +
                        "user bought your NO ADS in-app product");
                }

                return;
            }

            if (enabled)
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User is now PREMIUM");
            }
            else
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User isn't PREMIUM.");
            }

            PlayerPrefs.SetInt(PREFS_PREMIUM, enabled ? 1 : 0);
            SetEnabledNotRewardedAds(!enabled);
        }

        public static void SetPremiumPeriod(bool isPremiumPeriodActive)
        {
            if (isPremiumPeriodActive)
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User is now in PREMIUM PERIOD");
                PlayerPrefs.SetInt(PREMIUM_PERIOD, 1);
                SetEnabledNotRewardedAds(false);
            }
            else
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User is now in NOT in PREMIUM PERIOD");
                if (HasPremiumPeriod()) // if previously the free period was active
                {
                    PlayerPrefs.SetInt(PREMIUM_PERIOD, 0);
                    if (!IsPremium()) // and not a premium user (NoAds IAP)
                    {
                        SetEnabledNotRewardedAds(true);
                    }
                }
            }
        }

        private static void SetEnabledNotRewardedAds(bool enabled)
        {
            if (!enabled)
            {
                AdsManager.Banner.Hide();
                AdsManager.Banner.Disable();
                AdsManager.Interstitial.Disable();
                AdsManager.RewardedInterstitialVideo.Disable();
                AdsManager.AppOpen.Disable();
                AdsManager.Mrec.Disable();
                AdsManager.NativeAds.Disable();
                VoodooCrossPromo.Hide();
            }
            else 
            {
                AdsManager.Banner.Enable();
                AdsManager.Interstitial.Enable();
                AdsManager.RewardedInterstitialVideo.Enable();
                AdsManager.AppOpen.Enable();
                AdsManager.Mrec.Enable();
                AdsManager.NativeAds.Enable();
            }
        }
        
        

        public static void SetFreePeriod(bool isFreePeriodActive)
        {
            if (isFreePeriodActive)
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User is now in PREMIUM PERIOD");
                PlayerPrefs.SetInt(PREMIUM_PERIOD, 1);
                SetEnabledNotRewardedAds(false);
            }
            else
            {
                VoodooLog.LogDebug(Module.IAP, TAG, "User is now in NOT in PREMIUM PERIOD");
                if (PlayerPrefs.GetInt(PREMIUM_PERIOD, 0) == 1) // if previously the free period was active
                {
                    PlayerPrefs.SetInt(PREMIUM_PERIOD, 0);
                    if (!IsPremium()) // and not a premium user (NoAds IAP)
                    {
                        SetEnabledNotRewardedAds(false);
                    }
                }
            }
        }

        public static bool IsPremium() => IsIAPPremium() || HasPremiumPeriod();

        public static bool IsIAPPremium() => PlayerPrefs.GetInt(PREFS_PREMIUM, 0) == 1;
        public static bool HasPremiumPeriod() => PlayerPrefs.GetInt(PREMIUM_PERIOD, 0) == 1;
    }
}
