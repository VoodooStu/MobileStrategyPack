namespace Voodoo.Sauce.Debugger
{
    public abstract class CustomDebugger
    {
        public abstract string GetTitle();
        public abstract int GetOrderIndex();
        
        public abstract void SetupScreen(Screen screen);
    }
}
