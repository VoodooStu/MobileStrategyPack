using System;
using System.Collections.Generic;
using System.Reflection;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Ads;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooSettingsDebugScreen : Screen
    {
        private readonly HashSet<string> _ignoreKeySet = new HashSet<string> {
            "NAME",
            "AccessTokenID",
            "AccessBundleID",
            "LastUpdateDate",
            "EnableInEditorUnityAds",
            "EnableInEditorRVAds",
            "EnableInEditorFSAds",
            "MaxPercentOfTotalIosCohorts", // Ignore AB Test values - shown in ABTests
            "MixpanelUsersPercentPerCohort",
            "RunningIosABTests",
            "CustomIosABTestsCountryCodes",
            "DebugForcedCohort",
            "EnableAndroidABTests",
            "EnableCustomAndroidABTestsCountryCodes",
            "MaxPercentOfTotalAndroidCohorts",
            "AndroidUsersPercentPerCohort",
            "RunningAndroidABTests",
            "CustomAndroidABTestsCountryCodes",
            "DebugAndroidForcedCohort",
            "NoAdsBundleId", // Ignore IAP - shown in GeneralInfo
            "Products",
            "StudioLogoForSplashScreen",
            "GdprPrimaryColor",
            "VoodooLogLevelInEditor",
            "CrossPromo",
            "EditorIdfa",
            "IgnoreTheGoogleKeyError",
        };

        private void Awake()
        {
            DisplayVoodooSettingKeys();
        }

        private void DisplayVoodooSettingKeys()
        {
            Type voodooSettingsType = typeof(VoodooSettings);
            var voodooSettings = VoodooSettings.Load();

            VoodooLog.LogDebug(Internal.Module.ADS, "VoodooSauceSettingsKeys",
                $"Are VoodooSettings null: {voodooSettings == null}");

            FieldInfo[] voodooAutoSettingFields = voodooSettingsType.GetFields();

            VoodooLog.LogDebug(Internal.Module.ADS, "VoodooSauceSettingsKeys",
                $"fields count: {voodooAutoSettingFields.Length}");

            VoodooLog.LogDebug(Internal.Module.ADS, "VoodooSauceSettingsKeys",
                $"MaxAdsIosAdsKeys: {voodooSettingsType.GetField("MaxAdsIosAdsKeys").GetValue(voodooSettings)}");

            VoodooLog.LogDebug(Internal.Module.ADS, "VoodooSauceSettingsKeys",
                $"MaxAdsAndroidAdsKeys: {voodooSettingsType.GetField("MaxAdsAndroidAdsKeys").GetValue(voodooSettings)}");

            List<FieldInfo> sortedFields = new List<FieldInfo>(voodooAutoSettingFields);
            sortedFields.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            foreach (FieldInfo fieldInfo in sortedFields)
            {
                string fieldName = fieldInfo.Name;

                if (_ignoreKeySet.Contains(fieldName)) continue;

                if (VoodooSettings.IsAdUnitFieldName(fieldName))
                {
                    var adUnits = (AdsKeys)voodooSettingsType.GetField(fieldName).GetValue(voodooSettings);
                    Label(fieldName, "");
                    CopyToClipboard("-Banner", adUnits.maxAdsAdUnits.bannerAdUnit);
                    CopyToClipboard("-Interstitial", adUnits.maxAdsAdUnits.interstitialAdUnit);
                    CopyToClipboard("-RewardedVideo", adUnits.maxAdsAdUnits.rewardedVideoAdUnit);
                    CopyToClipboard("-AppOpen", adUnits.maxAdsAdUnits.appOpenAdUnit);
                    CopyToClipboard("-SessionCountThreshold", adUnits.sessionCountThreshold.ToString());
                    CopyToClipboard("-SecondaryBanner", adUnits.maxAdsSecondaryAdUnits.bannerAdUnit);
                    CopyToClipboard("-SecondaryInterstitial", adUnits.maxAdsSecondaryAdUnits.interstitialAdUnit);
                    CopyToClipboard("-SecondaryRewardedVideo", adUnits.maxAdsSecondaryAdUnits.rewardedVideoAdUnit);
                    CopyToClipboard("-SecondaryAppOpen", adUnits.maxAdsSecondaryAdUnits.appOpenAdUnit);
                }
                else
                {
                    object remoteFieldValue = voodooSettingsType.GetField(fieldName).GetValue(voodooSettings);
                    CopyToClipboard(fieldName, remoteFieldValue?.ToString());
                }
            }
        }
    }
}