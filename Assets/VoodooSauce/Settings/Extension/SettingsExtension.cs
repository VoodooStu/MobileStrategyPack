using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Internal.Extension
{
    public static class SettingsExtension
    {
        public static string[] GetRunningABTests(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.RunningAndroidABTests;
#elif UNITY_IOS
            return settings.RunningIosABTests;
#else
            return new string[0];
#endif
        }

        public static bool IsCustomABTestsCountryCodesEnabled(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.EnableCustomAndroidABTestsCountryCodes;
#elif UNITY_IOS
            return settings.EnableCustomIosABTestsCountryCodes;
#else
            return false;
#endif
        }

        public static string[] GetCustomABTestsCountryCodes(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.CustomAndroidABTestsCountryCodes;
#elif UNITY_IOS
            return settings.CustomIosABTestsCountryCodes;
#else
            return new string[0];
#endif
        }

        public static DebugForcedCohort GetDebugForcedCohort(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.DebugAndroidForcedCohort;
#elif UNITY_IOS
            return settings.DebugForcedCohort;
#else
            return new DebugForcedCohort();
#endif
        }

        public static float GeUsersPercentPerCohort(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.AndroidUsersPercentPerCohort;
#elif UNITY_IOS
            return settings.MixpanelUsersPercentPerCohort;
#else
            return 0f;
#endif
        }

        public static float GeMaxPercentOfTotalCohorts(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.MaxPercentOfTotalAndroidCohorts;
#elif UNITY_IOS
            return settings.MaxPercentOfTotalIosCohorts;
#else
            return 0f;
#endif
        }

        public static bool UseMixpanel(this VoodooSettings settings)
        {
#if UNITY_ANDROID
            return settings.UseMixpanelAndroid;
#elif UNITY_IOS
            return settings.UseMixpanelIos;
#else
            return false;
#endif
        }
    }
}