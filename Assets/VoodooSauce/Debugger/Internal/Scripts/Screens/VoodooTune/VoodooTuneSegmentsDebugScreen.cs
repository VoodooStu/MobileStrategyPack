using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Tune.Core;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneSegmentsDebugScreen : Screen
    {
        private void Start()
        {
            CreateSegmentsSection();
        }

        private void CreateSegmentsSection()
        {
            IEnumerable<Segment> segments = DebugVTManager.GetDebugSegments();
            DebugVTConfiguration currentConfiguration = DebugVTManager.CurrentDebugConfiguration;

            Button("None", OnNoneSegmentButtonClicked);

            foreach (Segment segment in segments) {
                Segment tempSegment = segment;
                DebugCheckboxButton btn = CheckboxButton(segment.Name, value => {
                    SelectSegment(value, tempSegment);
                });
                btn.SetValue(currentConfiguration.SelectedSegments.Exists(x => x.Id == segment.Id));
            }
        }

        private void OnNoneSegmentButtonClicked()
        {
            DebugVTManager.ClearDebugSegments();

            foreach (var checkbox in GetAll<DebugCheckboxButton>()) {
                checkbox.SetValue(false);
            }
            
            CheckConfiguration();
        }

        private void SelectSegment(bool value, Segment segment)
        {
            if (value)
            {
                DebugVTManager.AddDebugSegment(segment);
            }
            else
            {
                if (DebugVTManager.RemoveDebugSegment(segment))
                {
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