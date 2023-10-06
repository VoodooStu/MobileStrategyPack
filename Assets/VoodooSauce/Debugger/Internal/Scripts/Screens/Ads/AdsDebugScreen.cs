using System.Globalization;
using UnityEngine;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Ads;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts;
using Voodoo.Sauce.Internal.CrossPromo.BackupAds.Scripts.Models;
using Voodoo.Sauce.Internal.CrossPromo.Mercury.Models;

namespace Voodoo.Sauce.Debugger
{
    public class AdsDebugScreen : Screen
    {
        private DebugButtonWithInputField _showInterstitialButton;
        private DebugButtonWithInputField _showRewardedVideoButton;
        private DebugButtonWithInputField _showAppOpenButton;
        private DebugButtonWithInputField _triggerAndShowRewardedVideoButton;
        private DebugButtonWithInputField _showBackupInterstitialButton;
        private DebugButtonWithInputField _showBackupRewardedButton;
        
        private int _lastTime = 0;
        private const int TIME_REFRESH_INTERVAL = 5;

        private const string INTERSTITIAL_ADS_CONDITION = "(sec before first FS, sec before session's first FS, sec between FS, games between FS)";
        private const string APP_OPEN_ADS_CONDITION = "(sec in background, sec between AO, sec between FS and AO, sec between RV and AO, sec between AO and FS)";

        private void OnEnable()
        {
            ClearDisplay();
            ShowAdsInfo();

            if (!AdsManager.MediationAdapter.IsSdkInitialized())
            {
                ShowBackupInterstitialInfo();
                ShowInterstitialAdInfo();
                ShowRewardedVideoAdInfo();
                ShowAppOpenAdInfo();
            }
            else
            {
                ShowBannerAdInfo();
                ShowInterstitialAdInfo();
                ShowRewardedVideoAdInfo();
                ShowAppOpenAdInfo();
                ShowMrecAdInfo();
                ShowNativeAdInfo();
                ShowBackupInterstitialInfo();
            }

            ShowAudioAdInfo();
        }

        private void Update()
        {
            Refresh();
            RefreshWidgets();
        }

        private void Refresh()
        {
            if (!AdsManager.MediationAdapter.IsSdkInitialized())
            {
                UpdateButtonState(_showInterstitialButton, AdLoadingState.NotInitialized);
                UpdateButtonState(_showRewardedVideoButton, AdLoadingState.NotInitialized);
                UpdateButtonState(_showAppOpenButton, AdLoadingState.NotInitialized);
                UpdateButtonState(_triggerAndShowRewardedVideoButton, AdLoadingState.NotInitialized);
            }
            else
            {
                UpdateButtonState(_showInterstitialButton, AdsManager.Interstitial.State);
                UpdateButtonState(_showRewardedVideoButton, AdsManager.RewardedVideo.State);
                UpdateButtonState(_showAppOpenButton, AdsManager.AppOpen.State);
                UpdateButtonState(_triggerAndShowRewardedVideoButton, AdsManager.RewardedVideo.State);
            }

            UpdateButtonState(_showBackupInterstitialButton, BackupAdsManager.Instance.CanShowBackupInterstitial());
            UpdateButtonState(_showBackupRewardedButton, BackupAdsManager.Instance.CanShowBackupRewardedVideo());
        }

        private void ShowAdsInfo()
        {
            OpenFoldout("General info");
            if (!AdsManager.MediationAdapter.IsSdkInitialized())
            {
                CopyToClipboard("Mediation", "Ads Not Initialized");
                return;
            }

            Label("Mediation", AdsManager.MediationAdapter.GetMediationType().ToString());
            Label("Session count", AnalyticsStorageHelper.Instance.GetAppLaunchCount().ToString());
            if (AdsManager.MediationAdapter.GetMediationType() == MediationType.MaxAds)
            {
                CopyToClipboard("Banner Ad Unit ID", AdUnitsManager.adsKeys.BannerAdUnit);
                CopyToClipboard("Interstitial Ad Unit ID", AdUnitsManager.adsKeys.InterstitialAdUnit);
                CopyToClipboard("Rewarded Video Ad Unit ID", AdUnitsManager.adsKeys.RewardedVideoAdUnit);
                CopyToClipboard("AppOpen Ad Unit ID", AdUnitsManager.adsKeys.AppOpenAdUnit);
                CopyToClipboard("Mrec Ad Unit ID", AdUnitsManager.adsKeys.MrecAdUnit);
                CopyToClipboard("Native Ads Ad Unit ID", AdUnitsManager.adsKeys.NativeAdsAdUnit);
            }

            if (AdsManager.MediationAdapter.HasDebugger())
            {
                Button("Show Debugger", () => AdsManager.MediationAdapter.ShowDebugger());
            }

            CloseFoldout();
        }

        private void ShowBannerAdInfo()
        {
            OpenFoldout("Banner");
            Label("Loading time", () => FormatLoadingTime(AdsManager.Banner.LoadingTime));
            Label("Network", () => AdsManager.MediationAdapter.GetBannerInfo().AdNetworkName);
            Label("Native Banner Height (dp)", () => VoodooSauce.GetNativeBannerHeightInDp().ToString(CultureInfo.InvariantCulture));
            CloseFoldout();
        }

        private void ShowInterstitialAdInfo()
        {
            OpenFoldout("Interstitial");
            AddDebugUnityPauseWhenDisplayingAds();
            Label(AdsManager.GetInterstitialConditionSettings());
            Label(INTERSTITIAL_ADS_CONDITION).SetLabelBestFitTxt(true);
            Label(GetInterstitialConditionsInfo);
            Label("Loading time", () => FormatLoadingTime(AdsManager.Interstitial.LoadingTime));
            Label("Network", () => AdsManager.MediationAdapter.GetInterstitialInfo().AdNetworkName);
            Label("CPM", () => (AdsManager.MediationAdapter.GetInterstitialInfo().Revenue * 1000).ToString());
            _showInterstitialButton = Button("Show Interstitial",
                () => VoodooSauce.ShowInterstitial(
                    () => Debugger.DisplayPopup("The interstitial callback has been called."),
                    false,
                    "debugger_fs"));
            CloseFoldout();
        }

        private static string GetInterstitialConditionsInfo()
        {
            string state = AdsManager.Interstitial.AdDisplayConditions?.GetInterstitialConditionStatusString();
            string time = AdsManager.Interstitial.AdDisplayConditions?.GetInterstitialConditionTimeString();
            return $"{state}\n{time}";
        }

        private void ShowAppOpenAdInfo()
        {
            OpenFoldout("AppOpen");
            if (AdsManager.AppOpen.State == AdLoadingState.Disabled) {
                Label("State", () => AdsManager.AppOpen.State.ToString());
            } else {
                AddDebugUnityPauseWhenDisplayingAds();
                Label(AdsManager.GetAppOpenConditionSettings());
                Label(APP_OPEN_ADS_CONDITION).SetLabelBestFitTxt(true);
                Label(GetAppOpenConditionsInfo);
                Label("Loading time", () => FormatLoadingTime(AdsManager.AppOpen.LoadingTime));
                Label("Network", () => AdsManager.MediationAdapter.GetAppOpenInfo().AdNetworkName);
                Label("CPM", () => (AdsManager.MediationAdapter.GetAppOpenInfo().Revenue * 1000).ToString());
                _showAppOpenButton = Button("Show AppOpen", 
                    () => AdsManager.AppOpen.Show(
                        () => Debugger.DisplayPopup("The app open callback has been called.")));
            }
            CloseFoldout();
        }

        private static string GetAppOpenConditionsInfo()
        {
            string state = AdsManager.AppOpen.AdDisplayConditions?.GetAppOpenConditionStatusString();
            string time = AdsManager.AppOpen.AdDisplayConditions?.GetAppOpenConditionTimeString();
            return $"{state}\n{time}";
        }

        private void ShowRewardedVideoAdInfo()
        {
            OpenFoldout("Rewarded Video");
            AddDebugUnityPauseWhenDisplayingAds();
            Label("Loading time", () => FormatLoadingTime(AdsManager.RewardedVideo.LoadingTime));
            Label("Network", () => AdsManager.MediationAdapter.GetRewardedVideoInfo().AdNetworkName);
            Label("CPM", () => (AdsManager.MediationAdapter.GetRewardedVideoInfo().Revenue * 1000).ToString());
            if (AdsManager.EnableReplaceRewardedOnCpm)
            { 
                CopyToClipboard("Replace with Interstitial if CPM lower", "Enabled");
            }
            
            if (AdsManager.EnableReplaceRewardedIfNotLoaded)
            { 
                CopyToClipboard("Replace with Interstitial if RV not loaded", "Enabled");
            }

            var rvTag = "debugger_rv";
            _showRewardedVideoButton = Button("Show RewardedVideo",
                () => VoodooSauce.ShowRewardedVideo(ShowRewardedVideoCallbackPopup, rvTag));
            _triggerAndShowRewardedVideoButton = Button("Trigger & Show RewardedVideo", () =>
            {
                VoodooSauce.OnRewardedVideoButtonShown(rvTag);
                VoodooSauce.ShowRewardedVideo(ShowRewardedVideoCallbackPopup, rvTag);
            });
            CloseFoldout();
        }
        
        
        // This text is used to check if unity is paused or not during ads
        // the value must be the same after closing an ad
        private void AddDebugUnityPauseWhenDisplayingAds()
        {
            if (Application.identifier != VoodooConstants.TEST_APP_BUNDLE) return;
            Label("Debug Unity pause time", () => {
                if (Time.time > _lastTime + TIME_REFRESH_INTERVAL) {
                    _lastTime = (int) Time.time;
                }

                return _lastTime.ToString(CultureInfo.InvariantCulture);

            }, false);
        }


        private void ShowRewardedVideoCallbackPopup(bool reward)
        {
            Debugger.DisplayPopup("The rewarded video callback has been called with" + (reward? "": "out") + " the reward.");
        }

        private void ShowMrecAdInfo()
        {
            OpenFoldout("Mrec");
            Label("State", () => AdsManager.Mrec.State.ToString());
            if (AdsManager.Mrec.State != AdLoadingState.Disabled)
            {
                Label("Loading time", () => FormatLoadingTime(AdsManager.Mrec.LoadingTime));
                Label("Network", () => AdsManager.MediationAdapter.GetMrecInfo().AdNetworkName);
            }
            CloseFoldout();
        }

        private void ShowAudioAdInfo()
        {
            OpenFoldout("Audio ad");
            if (AudioAdsManager.Instance.IsEnabled)
            {
                Label("Ad network", AudioAdsManager.Instance.AdNetworkName);
                Label("Timer", () =>
                {
                    Vector2 audioAdTimer = AudioAdsManager.Instance.GetTimer();
                    return Mathf.Floor(audioAdTimer.x) + "s/" + audioAdTimer.y + "s";
                });
            }

            Label("State", () =>
            {
                return AudioAdsManager.Instance.GetState() switch
                {
                    AudioAdsManager.State.Ready => "An audio ad is ready.",
                    AudioAdsManager.State.Disabled => "The audio ad module is disabled.",
                    AudioAdsManager.State.Misconfigured => "The audio ad module is misconfigured.",
                    AudioAdsManager.State.TooEarly => "It's too early.",
                    AudioAdsManager.State.LoadingAd => "An audio ad is not loaded yet.",
                    AudioAdsManager.State.ShowingAd => "An audio ad is showing.",
                    AudioAdsManager.State.PositionConfigMissing => "Position configuration missing.",
                    _ => "Unknown state."
                };
            });
            if (AudioAdsManager.Instance.IsEnabled)
            {
                Button("Show/hide Audio Ad", AudioAdsManager.Instance.ShowOrHideAudioAd);
            }
        }

        private void ShowNativeAdInfo()
        {
            OpenFoldout("Native Ad");
            Label("State", () => AdsManager.NativeAds.State.ToString());
            if (AdsManager.NativeAds.State != AdLoadingState.Disabled)
            {
                Label("Loading time", () => FormatLoadingTime(AdsManager.NativeAds.LoadingTime));
                Label("Network", () => AdsManager.MediationAdapter.GetNativeAdsInfo().AdNetworkName);
            }
            CloseFoldout();
        }

        private void ShowBackupInterstitialInfo()
        {
            OpenFoldout("Backup Ads");

            var response = GetBackupAdsApiResponse();
            var value = "Copy JSON";
            if (string.IsNullOrEmpty(response))
            {
                value = "No API response available";
            }

            CopyToClipboard("API Response", value, GetBackupAdsApiResponse);
            CopyToClipboard("Videos", GetBackupAdsAssets);
            Label("State", () => BackupAdsManager.Instance.IsBackupAdAvailable().ToString());
            Label("Last Watched", GetLastDisplayedBackupInterstitial);

            _showBackupInterstitialButton = Button("Show Backup Interstitial", () => BackupAdsManager.Instance.ShowCrossPromoInterstitial(null));
            _showBackupRewardedButton = Button("Show Backup Rewarded Video", () => BackupAdsManager.Instance.ShowCrossPromoRewardedVideo(null));
            
            CloseFoldout();
        }

        private static void UpdateButtonState(DebugButtonWithInputField button, AdLoadingState state)
        {
            if (!button) {
                return;
            }
            
            button.SetEnable(state != AdLoadingState.Disabled);
            button.SetColor(state.ToColor());
        }
        
        private static void UpdateButtonState(DebugButtonWithInputField button, bool enable)
        {
            if (!button) {
                return;
            }
            
            button.SetEnable(enable);
            button.SetColor(enable ? Color.green : Color.red);
        }

        private static string FormatLoadingTime(AdTimer loadingTime) => loadingTime.TotalSeconds.ToString("0.##") + "s";

        private static string GetLastDisplayedBackupInterstitial()
        {
            BackupAdsInfo lastDisplayedAds = BackupAdsManager.Instance.lastDisplayedAdInfo;
            return lastDisplayedAds != null ? lastDisplayedAds.gameName : "No video was watched";
        }

        private static string GetBackupAdsAssets()
        {
            MercuryWaterfall waterfall = BackupAdsManager.Instance.mercuryWaterfall;
            if (waterfall == null)
            {
                return "Waterfall is null";
            }

            var assets = waterfall.GetPromotedAssets();
            if (string.IsNullOrEmpty(assets))
            {
                return "No assets founds";
            }

            return assets;
        }

        private string GetBackupAdsApiResponse()
        {
            MercuryWaterfall waterfall = BackupAdsManager.Instance.mercuryWaterfall;
            return waterfall != null ? waterfall.ToJson() : "";
        }
    }
}