using System.Collections.Generic;
using Voodoo.Sauce.Internal.VoodooTune;
using Voodoo.Tune.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneSandboxDebugScreen : Screen
    {
        private void Start()
        {
            DebugVTManager.NewDebugDraftConfiguration();
            IEnumerable<Sandbox> sandboxList = DebugVTManager.GetDebugSandboxes();

            ClearDisplay();
            Button("None", UnselectSandbox);
            
            Label("Select a Sandbox:");
            CreateRadioGroup();
            foreach (Sandbox sandbox in sandboxList) {
                var tempSandbox = sandbox;
                DebugRadioButton toggle = RadioButton(sandbox.Name, isOn => {
                    OnToggleChanged(isOn, tempSandbox);
                });
                
                bool defaultValue = DebugVTManager.CurrentDebugConfiguration.SelectedSandbox?.Id == sandbox.Id; 
                toggle.SetValue(defaultValue);
            }
            CloseRadioGroup();
        }

        private void UnselectSandbox()
        {
            DebugVTManager.ClearSandboxes();
            ResetRadioGroups(false);
        }

        private void OnToggleChanged(bool isOn, Sandbox sandbox)
        {
            if (isOn == false)
            {
                return;
            }
            
            DebugVTManager.SelectDebugSandbox(sandbox);
        }
    }
}