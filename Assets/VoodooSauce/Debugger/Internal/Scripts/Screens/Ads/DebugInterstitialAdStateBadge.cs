using UnityEngine;
using Voodoo.Sauce.Internal.Ads;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugInterstitialAdStateBadge : DebugAdStateBadge
    {
        protected override bool IsEnabled() => AdsDebugManager.IsInterstitialAdStateBadgeEnabled;

        protected override Color StateColor()
        {
            if (VoodooSauce.IsInterstitialAvailable())
                return AdLoadingState.Loaded.ToColor();
           return AdsManager.Interstitial.State.ToColor();
        }

        protected override void Awake()
        {
            AdsDebugManager.interstitialAdStateBadgeChanged += UpdateVisibility;
            base.Awake();
        }

        private void OnDestroy()
        {
            AdsDebugManager.interstitialAdStateBadgeChanged -= UpdateVisibility;
        }
    }
}