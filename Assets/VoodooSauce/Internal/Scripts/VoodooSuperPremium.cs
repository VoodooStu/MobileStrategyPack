using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IAP;

namespace Voodoo.Sauce.Internal
{
    internal static class VoodooSuperPremium
    {
        private static bool _isSuperPremium;
        private static bool _isDebugBuild;
        internal static bool IsSuperPremium() => _isSuperPremium && _isDebugBuild;
        
        internal static void Initialize(VoodooSettings settings)
        {
            _isSuperPremium = settings.EnableSuperPremiumMode;
            _isDebugBuild = Debug.isDebugBuild;

            if (!IsSuperPremium()) return;
            
            //If super premium is true, set VoodooPremium to true as well, to prevent CP, FS and Banner to be shown
            if (!VoodooPremium.IsPremium())
                VoodooPremium.SetEnabledPremium();
        }
    }
}