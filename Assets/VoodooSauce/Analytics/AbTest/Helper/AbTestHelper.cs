using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Voodoo.Sauce.Internal.Analytics
{
    public static class AbTestHelper
    {
        private const string PrefCohort = "VoodooSauce_Cohort";
        private const string CohortDefault = "Control";
        public const int ControlCohortCount = 1;
        public const string ABTestDefaultCountryCode = "us";

        internal static string GenerateNewRandomCohort(string[] runningAbTests, float usersPercentPerCohort)
        {
            if (runningAbTests == null || runningAbTests.Length == 0) {
                return null;
            }

            float randomValue = Random.value;
            return runningAbTests.Where((abTest, index) => randomValue < (index + 1) * usersPercentPerCohort).FirstOrDefault();
        }

        internal static string[] AddControlCohortsToAbTests(string[] runningAbTests)
        {
            var abTests = new string[runningAbTests.Length + ControlCohortCount];
            for (var i = 0; i < ControlCohortCount; i++) abTests[i] = CohortDefault + " " + (i + 1);
            for (var i = 0; i < runningAbTests.Length; i++) abTests[i + ControlCohortCount] = runningAbTests[i];
            return abTests;
        }

        internal static bool HasSavedPlayerCohort() => PlayerPrefs.HasKey(PrefCohort);

        internal static string GetSavedPlayerCohort() => PlayerPrefs.GetString(PrefCohort, null);

        public static void SavePlayerCohort(string cohort)
        {
            if (cohort == null) {
                if (PlayerPrefs.HasKey(PrefCohort)) {
                    PlayerPrefs.DeleteKey(PrefCohort);
                }
            } else {
                PlayerPrefs.SetString(PrefCohort, cohort);
            }
        }
    }
}