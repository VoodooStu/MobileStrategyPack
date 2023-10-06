using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Extension;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    // ReSharper disable once InconsistentNaming
    public class LegacyABTestsDebugScreen : Screen, IConditionalScreen
    {
        private const string NO_COHORT = "NO_COHORT";

        [SerializeField]
        private Text currentCohortText;

        [SerializeField]
        private Text restartGameText;

        [SerializeField]
        private Text cohortSizeText;

        public bool CanDisplay => !VoodooSettings.Load().UseRemoteConfig;

        private void Awake()
        {
            restartGameText.enabled = false;

            VoodooSettings settings = VoodooSettings.Load();

            cohortSizeText.text = $"{settings.GeUsersPercentPerCohort() * 100}%";
            SetUpCohortButtons(settings);
        }

        private void OnEnable()
        {
            currentCohortText.text = AbTestManager.GetPlayerCohort();

            if (string.IsNullOrEmpty(AbTestManager.GetPlayerCohort())) {
                currentCohortText.text = NO_COHORT;
            }
        }

        private void SetUpCohortButtons(VoodooSettings settings)
        {
            CreateCohortButton(NO_COHORT);

            foreach (string abTestName in settings.GetRunningABTests()) {
                CreateCohortButton(abTestName);
            }
        }

        private void CreateCohortButton(string abTestName)
        {
            var cohortButton = WidgetUtility.InstanceOf<CohortButton>(Parent);
            cohortButton.SetOnClickListener(() => SetAbTest(abTestName));
            cohortButton.SetTitleText(abTestName);
        }

        private void SetAbTest(string abTestName)
        {
            AbTestManager.SetPlayerCohort(abTestName.Equals(NO_COHORT) ? null : abTestName);

            currentCohortText.text = abTestName;
            restartGameText.enabled = true;
        }
    }
}