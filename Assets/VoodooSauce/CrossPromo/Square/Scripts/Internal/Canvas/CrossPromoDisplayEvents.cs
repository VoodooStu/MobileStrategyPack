using System;

namespace Voodoo.Sauce.Internal.CrossPromo.Canvas
{
    public static class CrossPromoDisplayEvents

    {
        public static event Action ShowEvent;
        public static event Action HideEvent;

        public static void TriggerShow()
        {
            ShowEvent?.Invoke();
        }

        public static void TriggerHide()
        {
            HideEvent?.Invoke();
        }
    }
}