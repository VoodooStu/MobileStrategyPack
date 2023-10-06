using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Analytics
{
    public class ABTestIntegrationCheck : IIntegrationCheck
    {
        private const string SETTINGS_AB_TESTING_REMOTELY_DEACTIVATED_ERROR =
            "You have added A/B Tests in VoodooSettings 'Running AB Tests' section but VoodooAnalytics is deactivated via VoodooKitchen. Please contact Voodoo if you believe this is a mistake.";

        private const string SETTINGS_LEGACY_AB_TESTING_IGNORED_WARNING =
            "A/B Tests set in VoodooSettings 'Running AB Tests' section will be ignored since RemoteConfig is enabled via VoodooKitchen. Please contact Voodoo if you believe this is a mistake or remove your running A/B tests";

        private const string SETTINGS_VOODOO_ANALYTICS_AB_TESTING_DEACTIVATED_ERROR =
            "VoodooAnalytics must be activated via VoodooKitchen for RemoteConfig AB Tests to work. Please contact Voodoo if you believe this is a mistake.";

        private const string SETTINGS_DEACTIVATE_MIXPANEL_WARNING =
            "Mixpanel is expensive and should only be used on a limited volume of users. We encourage you to deactivate Mixpanel in VoodooKitchen.";

        private const string SETTINGS_AB_TESTS_CUSTOM_COUNTRIES_REMOTELY_DEACTIVATED_ERROR =
            "You have added countries under 'Custom AB Tests Country Codes' section but A/B testing is deactivated via VoodooKitchen for countries other than the 'US'."
            + " Please contact Voodoo if you believe this is a mistake";

        private const string SETTINGS_AB_TESTS_EMPTY_CUSTOM_COUNTRIES_ERROR =
            "You have added A/B Tests under 'Running AB Tests' section but the 'Custom AB Tests Country Codes' is empty.";

        private const string SETTINGS_AB_TESTS_WRONG_COUNTRY_CODE_ERROR =
            "You have placed an invalid country code {0} in your 'Custom AB Tests Country Codes'.";

        private const string SETTINGS_AB_TESTS_TOTAL_PERCENT_EXCEEDS_MAX_ERROR =
            "The total percentage of A/B test users is {0} * {1} = {2}. This exceeds the max value of {3}.";

        private const string SETTINGS_AB_TESTS_NO_NAME_PROVIDED =
            "You are currently using Legacy A.B Tests. Please enter the name of the current A/B test under Legacy A/B Tests -> LegacyAbTestName";
        
        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();
            string[] runningABTests = settings.GetRunningABTests();
            bool useMixpanel = settings.UseMixpanel();
            bool useVoodooTune = settings.UseRemoteConfig;
            bool useVoodooAnalytics = settings.UseVoodooAnalytics;
            bool hasRunningABTests = runningABTests != null && runningABTests.Length > 0;

            string legacyAbTestName = settings.LegacyAbTestName;

            // Case: Remote A/B testing (VoodooTune is ON)
            if (useVoodooTune) {
                if (hasRunningABTests) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, SETTINGS_LEGACY_AB_TESTING_IGNORED_WARNING));
                }

                if (!useVoodooAnalytics) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_VOODOO_ANALYTICS_AB_TESTING_DEACTIVATED_ERROR));
                }

                return list;
            }

            // Case: Legacy A/B testing ( VoodooTune is OFF)
            if (hasRunningABTests) {
                if (String.IsNullOrWhiteSpace(legacyAbTestName)) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTS_NO_NAME_PROVIDED)); 
                }   
            } else {
                return list;
            }

            // VoodooAnalytics is not activated for A|B testing
            if (!useVoodooAnalytics) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTING_REMOTELY_DEACTIVATED_ERROR));
                return list;
            }
            
            if (useMixpanel) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, SETTINGS_DEACTIVATE_MIXPANEL_WARNING));
            }

            bool enableCustomABTestsCountryCodes = settings.IsCustomABTestsCountryCodesEnabled();
            string[] customABTestsCountryCodes = settings.GetCustomABTestsCountryCodes();
            float maxTotalCohortsPercent = settings.GeMaxPercentOfTotalCohorts();
            float percentPerCohort = settings.GeUsersPercentPerCohort();

            // custom A\B tests countries are filled but A\B testing on custom countries is deactivated
            if (!enableCustomABTestsCountryCodes && customABTestsCountryCodes != null
                && customABTestsCountryCodes.Any(code => code.ToLower() != AbTestHelper.ABTestDefaultCountryCode)) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTS_CUSTOM_COUNTRIES_REMOTELY_DEACTIVATED_ERROR));
                return list;
            }

            // Check A\B tests country codes
            if (enableCustomABTestsCountryCodes) {
                if (customABTestsCountryCodes == null || customABTestsCountryCodes.Length == 0) {
                    list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTS_EMPTY_CUSTOM_COUNTRIES_ERROR));
                    return list;
                }

                foreach (string code in customABTestsCountryCodes) {
                    try {
                        new RegionInfo(code);
                    } catch (Exception) {
                        list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTS_WRONG_COUNTRY_CODE_ERROR, new[] {code}));
                    }
                }
            }

            // Check A\B tests cohorts percent
            int nbRunningAbTests = runningABTests.Length + AbTestHelper.ControlCohortCount;
            float totalPercent = percentPerCohort * nbRunningAbTests;
            if (totalPercent > maxTotalCohortsPercent) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_AB_TESTS_TOTAL_PERCENT_EXCEEDS_MAX_ERROR, new[] {
                    nbRunningAbTests.StringValue(), percentPerCohort.StringValue(), totalPercent.StringValue(), maxTotalCohortsPercent.StringValue()
                }));
            }

            return list;
        }
    }
}