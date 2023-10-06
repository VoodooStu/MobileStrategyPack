using System;
using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Sauce.Debugger
{
    public class DebugRadioGroup
    {
        private readonly List<DebugRadioButton> _buttons = new List<DebugRadioButton>();

        public void Add(DebugRadioButton button)
        {
            _buttons.Add(button);
            button.SetGroupCallback(OnRadioSelected);
        }

        private void OnRadioSelected(bool value, DebugRadioButton button)
        {
            button.CallCallbackFromGroup(value);
            if (value) {
                UnselectOthers(button);
            }
        }

        private void UnselectOthers(DebugRadioButton button)
        {
            foreach (DebugRadioButton btn in _buttons.Where(btn => btn != button)) {
                btn.SetValue(false);
            }
        }

        public void ResetToggles(bool callCallback = true)
        {
            foreach (DebugRadioButton btn in _buttons.Where(btn => btn.Toggled)) {
                btn.SetValueAndCallCallback(false, callCallback);
            }
        }
    }
}