using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Internal.Analytics
{
    public class DebugForcedCohort
    {
        public int index;
        private VoodooSettings _settings;

        public DebugForcedCohort() => index = 0;

        public string GetCohort()
        {
            if (_settings == null) {
                _settings = VoodooSettings.Load();
            }

            string[] options = _settings.GetRunningABTests();
            return index >= 3 ? options[index - 3] : "Control";
        }

        public bool HasForcedNoCohort() => index == 1;

        private bool IsNotEmpty() => index > 0;

        public bool IsDebugCohort() => (Debug.isDebugBuild || Application.isEditor) && IsNotEmpty();
    }
}