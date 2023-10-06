using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.CrashReport;
using Voodoo.Sauce.Debugger.Widgets;
using Voodoo.Sauce.Internal.DebugScreen;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class CrashReporterDebugScreen : Screen
    {
#region Constants

        private const string CRASH_REPORTER_CHOSEN_MESSAGE = "The crash reporter is set to {0}, restart the application to apply the modifications.";
        private const string CRASH_REPORTER_UNAVAILABILITY_MESSAGE = "The crash reporter is not available in the Unity Editor.";
        private const string CRASH_REPORTER_UPDATE_MESSAGE = "Changing the crash reporter will affect error reporting.";

#endregion

#region Properties

        [SerializeField]
        private DebugPopup debugPopup;

        private CrashReportCore.CrashReporter _crashReporter;
        private readonly Dictionary<CrashReportCore.CrashReporter, Widget> _crashReportersToggle =
            new Dictionary<CrashReportCore.CrashReporter, Widget>();
        
#endregion
        
        private void Start()
        {
            _crashReporter = VoodooSauceCore.GetCrashReport().GetCrashReporter();
            
            foreach (CrashReportCore.CrashReporter value in Enum.GetValues(typeof(CrashReportCore.CrashReporter))) {
                if (value == CrashReportCore.CrashReporter.None) {
                    continue;
                }

                float percentage = VoodooSauceCore.GetCrashReport().GetUserPercentage(value);
                if (percentage == 0) {
                    _crashReportersToggle.Add(value, Label($"{value.ToString()} (disabled)"));
                } else {
                    _crashReportersToggle.Add(value, Toggle($"{value.ToString()} ({percentage}%)", value == _crashReporter, isOn =>  CrashReporterSwitch(isOn, value)));
                }
            }

            Label(PlatformUtils.UNITY_EDITOR ? CRASH_REPORTER_UNAVAILABILITY_MESSAGE : CRASH_REPORTER_UPDATE_MESSAGE);
        }

        private void CrashReporterSwitch(bool isOn, CrashReportCore.CrashReporter crashReporter)
        {
            DebugToggleButton crashReporterButton = null;
            if (_crashReportersToggle[crashReporter] is DebugToggleButton) {
                crashReporterButton = (DebugToggleButton)_crashReportersToggle[crashReporter];
            }
            
            if (PlatformUtils.UNITY_EDITOR) {
                // ReSharper disable once Unity.NoNullPropagation
                crashReporterButton?.SetValue(false);
                DisplayPopup(CRASH_REPORTER_UNAVAILABILITY_MESSAGE);
                return;
            }
            
            if (crashReporter == _crashReporter && isOn == false) {
                // ReSharper disable once Unity.NoNullPropagation
                crashReporterButton?.SetValue(true);
                return;
            }
            
            if (crashReporter == _crashReporter || isOn == false) {
                return;
            }

            CrashReportCore.CrashReporter former = _crashReporter;
            _crashReporter = crashReporter;

            if (_crashReportersToggle[former] is DebugToggleButton) {
                ((DebugToggleButton)_crashReportersToggle[former]).SetValue(false);
            }
            // ReSharper disable once Unity.NoNullPropagation
            crashReporterButton?.SetValue(true);
            
            VoodooSauceCore.GetCrashReport().ForceCrashReporter(crashReporter);
            
            string message = string.Format(CRASH_REPORTER_CHOSEN_MESSAGE, crashReporter.ToString());
            DisplayPopup(message);
        }

        private void DisplayPopup(string message)
        {
            debugPopup.Initialize(message, () => { debugPopup.gameObject.SetActive(false); });
            debugPopup.gameObject.SetActive(true);
        }
    }
}