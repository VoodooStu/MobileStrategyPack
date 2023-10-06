using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Ads.FakeMediation;
using Voodoo.Sauce.Internal.CrossPromo.Mercury;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Tools.AccessButton;
using Voodoo.Sauce.Tools.PerformanceDisplay;

namespace Voodoo.Sauce.Debugger
{
    public class OptionsDebugScreen : Screen
    {
        private void OnEnable()
        {
            ClearDisplay();
            
            Toggle("Enable Access Button", AccessProcess.HasAccess(), ToggleAccessButton);
            Toggle("Enable Performance Display", PerformanceDisplayManager.IsEnabled, TogglePerformanceDisplay);
            Toggle("Enable Interstitial Ad Badge", AdsDebugManager.IsInterstitialAdStateBadgeEnabled, ToggleInterstitialBadge);
            Toggle("Enable Rewarded Video Ad Badge", AdsDebugManager.IsRewardedVideoAdStateBadgeEnabled, ToggleRewardedBadge);
            
            if (PlatformUtils.UNITY_EDITOR || !Debug.isDebugBuild) {
                Label(GetFakeAdsMessage());
            } else {
                Toggle("Enable Fake Ads", AdsManager.fakeAdsManager.IsEnabled(), ToggleFakeAds);
            }
            
            Toggle("Enable Cross Promo/Backup FS Test mode", MercuryTestModeManager.Instance.IsTestModeEnabled(), ToggleMercuryTestMode);
            Button("Delete PlayerPrefs", DeletePlayerPrefs);
        }

        private string GetFakeAdsMessage()
        {
            var message = "";
            
            if (PlatformUtils.UNITY_EDITOR) {
                message = "The fake ads can not be disabled in the in-editor mode.";
            } else if (!Debug.isDebugBuild) {
                message = "The fake ads can be enabled on development mode only.";
            }

            return message;
        }

        private void ToggleAccessButton(bool value)
        {
            AccessProcess.SetAccess(value);
        }

        private void TogglePerformanceDisplay(bool value)
        {
            PerformanceDisplayManager.IsEnabled = value;
        }

        private void ToggleInterstitialBadge(bool value)
        {
            AdsDebugManager.IsInterstitialAdStateBadgeEnabled = value;
        }

        private void ToggleRewardedBadge(bool value)
        {
            AdsDebugManager.IsRewardedVideoAdStateBadgeEnabled = value;
        }

        private void ToggleFakeAds(bool value)
        {
            IFakeAdsManager fakeAdsManager = AdsManager.fakeAdsManager;
            
            bool previousState = fakeAdsManager.IsEnabled();
            fakeAdsManager.SetEnabled(value);
            bool currentState = fakeAdsManager.IsEnabled();
            
            if (previousState != currentState) {
                Debugger.DisplayPopup(new DebuggerPopupConfig {
                    message = "To enable or disable the fake ads the application must be restarted.",
                    confirmCallback = Application.Quit,
                    confirmText = "Restart",
                    cancelText = "Later"
                });
            }
        }

        private void ToggleMercuryTestMode(bool value)
        {
            MercuryTestModeManager.Instance.SetTestMode(value);
        }

        private void DeletePlayerPrefs()
        {
            Debugger.DisplayPopup(new DebuggerPopupConfig {
                message = "Are you sure you want to delete local PlayerPrefs? This action cannot be undone.",
                confirmCallback = PlayerPrefs.DeleteAll,
            });
        }
    }
}