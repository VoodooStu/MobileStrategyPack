namespace Voodoo.Sauce.Debugger
{
    public class CustomDebuggerScreen : Screen
    {
        private CustomDebugger _customDebugger;
        
        public void Initialize(CustomDebugger customDebugger)
        {
            _customDebugger = customDebugger;
            title = _customDebugger.GetTitle();
            orderIndex = _customDebugger.GetOrderIndex();
            _customDebugger.SetupScreen(this);
        }
    }
}