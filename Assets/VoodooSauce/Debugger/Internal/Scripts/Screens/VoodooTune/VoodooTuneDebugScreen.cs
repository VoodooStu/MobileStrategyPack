using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Debugger
{
    public class VoodooTuneDebugScreen : Screen, IConditionalScreen
    {
        public bool CanDisplay => VoodooSettings.Load().UseRemoteConfig;
    }
}