using UnityEngine;
using Voodoo.Sauce.Internal.Ads;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugRewardedVideoAdStateBadge : DebugAdStateBadge
    {
        protected override bool IsEnabled() => AdsDebugManager.IsRewardedVideoAdStateBadgeEnabled;

        protected override Color StateColor()
        {
            if (VoodooSauce.IsRewardedVideoAvailable())
                return AdLoadingState.Loaded.ToColor();
            return AdsManager.RewardedVideo.State.ToColor();
        }

        protected override void Awake()
        {
            AdsDebugManager.rewardedVideoAdStateBadgeChanged += UpdateVisibility;
            base.Awake();
        }

        private void OnDestroy()
        {
            AdsDebugManager.rewardedVideoAdStateBadgeChanged -= UpdateVisibility;
        }
    }
}