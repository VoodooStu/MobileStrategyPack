using System.Collections.Generic;

namespace Voodoo.Sauce.Debugger
{
    public class HomeDebugScreen : Screen
    {
        private List<Screen> _screens = new List<Screen>();

        public void Initialize(List<Screen> screens)
        {
            _screens = screens;
        }

        public void OnEnable() => Refresh();
        
        public void Refresh()
        {
            ClearDisplay();

            foreach (var screen in _screens)
            {
                if (screen is IConditionalScreen cs &&
                    cs.CanDisplay == false)
                {
                    continue;
                }

                MenuItem(screen.title, screen.image, screen.imageColor, () => Debugger.Show(screen), screen.Counter());
            }
        }
    }
}