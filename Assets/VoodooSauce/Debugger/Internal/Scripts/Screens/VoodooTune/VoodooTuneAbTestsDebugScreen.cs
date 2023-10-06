using System.Collections.Generic;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Tune.Core;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneAbTestsDebugScreen : Screen
    {
        private void Start()
        {
            CreateAbTestsSections();
        }

        private void CreateAbTestsSections()
        {
            IEnumerable<ABTest> abTests = DebugVTManager.GetDebugAbTests();
            DebugVTConfiguration currentConfiguration = DebugVTManager.CurrentDebugConfiguration;

            Button("None", OnNoneButtonClicked);

            foreach (ABTest abTest in abTests) {
                Label(abTest.Name);
                CreateRadioGroup();
                
                foreach (Cohort cohort in abTest.Cohorts) {
                    ABTest tempAbTest = abTest;
                    Cohort tempCohort = cohort;
                    
                    DebugRadioButton radioBtn = RadioButton(cohort.Name, value => {
                        SelectCohort(value, tempAbTest, tempCohort);
                    });
                    radioBtn.SetValue(currentConfiguration.SelectedCohorts.Exists(x => x.Id == cohort.Id));
                }

                CloseRadioGroup();
            }
        }

        private void OnNoneButtonClicked()
        {
            CheckConfiguration(true);
        }

        private void SelectCohort(bool value, ABTest abTest, Cohort cohort)
        {
            if (value) {
                DebugVTManager.AddCohort(cohort, abTest);
            } else {
                if (DebugVTManager.RemoveCohort(cohort, abTest)) {
                    CheckConfiguration();
                }
            }
        }

        // If there is no cohort and segment selected, the debug configuration must be reset to be clean
        // and avoid some bugs on the VTConfigUI screen and to save in the player prefs.
        // So please call this method every time a cohort or a segment is removed. 
        private void CheckConfiguration(bool force = false)
        {
            DebugVTConfiguration configuration = DebugVTManager.DraftDebugConfiguration;
            bool mustBeReset = force || (configuration != null && configuration.HasNoneSelected);

            if (mustBeReset == false) {
                return;
            }
            
            DebugVTManager.ResetDebugDraftConfiguration();
            ResetRadioGroups(false);
        }
    }
}