using Voodoo.Sauce.Internal.DebugScreen;

namespace Voodoo.Sauce.Tools.AccessButton
{
    public class DebugAccessButton : DebugToggleComponent
    {
        protected override bool DefaultButtonState() => AccessProcess.HasAccess();

        public override void SetEnabled(bool isEnabled)
        {
            AccessProcess.SetAccess(isEnabled);
            base.SetEnabled(isEnabled);
        }

        private void Awake()
        {
            AccessProcess.InstantiateAccessButton += UpdateEnabled;
            AccessProcess.DisposeAccessButton += UpdateEnabled;
        }

        private void OnDestroy()
        {
            AccessProcess.InstantiateAccessButton -= UpdateEnabled;
            AccessProcess.DisposeAccessButton -= UpdateEnabled;
        }
    }
}