using GameAnalyticsSDK;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Analytics.Editor
{
    public class GameAnalyticsPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            UpdateGameAnalyticsSettings(Resources.Load<VoodooSettings>(VoodooSettings.NAME), report.summary.platform);
        }

        private static void UpdateGameAnalyticsSettings(VoodooSettings settings, BuildTarget target)
        {
            if (target == BuildTarget.iOS) {
                UpdateGameAnalyticsPlatformSettings(settings.GameAnalyticsIosGameKey, settings.GameAnalyticsIosSecretKey, RuntimePlatform.IPhonePlayer);
            } else if (target == BuildTarget.Android) {
                UpdateGameAnalyticsPlatformSettings(settings.GameAnalyticsAndroidGameKey, settings.GameAnalyticsAndroidSecretKey, RuntimePlatform.Android);
            }
        }

        private static void UpdateGameAnalyticsPlatformSettings(string gameKey, string secretKey, RuntimePlatform platform)
        {
            if (string.IsNullOrEmpty(gameKey) || string.IsNullOrEmpty(secretKey)) {
                return;
            }

            if (!GameAnalytics.SettingsGA.Platforms.Contains(platform)) {
                GameAnalytics.SettingsGA.AddPlatform(platform);
            }

            int platformIndex = GameAnalytics.SettingsGA.Platforms.IndexOf(platform);
            GameAnalytics.SettingsGA.UpdateGameKey(platformIndex, gameKey);
            GameAnalytics.SettingsGA.UpdateSecretKey(platformIndex, secretKey);
            GameAnalytics.SettingsGA.Build[platformIndex] = Application.version;
            GameAnalytics.SettingsGA.InfoLogBuild = false;
            GameAnalytics.SettingsGA.InfoLogEditor = false;
            GameAnalytics.SettingsGA.SubmitFpsAverage = true;
            GameAnalytics.SettingsGA.SubmitFpsCritical = true;
        }
    }
}