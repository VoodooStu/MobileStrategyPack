using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal class FakeRewardedVideoAd : FakeClosableAd
    {
        [FormerlySerializedAs("_closeWithRewardButton"),SerializeField]
        private Button closeWithRewardButton;
        
        [FormerlySerializedAs("_closeWithoutRewardButton"),SerializeField]
        private Button closeWithoutRewardButton;

        public Action onCloseWithReward;
        public Action onCloseWithoutReward;
        
        protected new void Awake()
        {
            base.Awake();
            
            if (closeWithRewardButton) {
                closeWithRewardButton.onClick.AddListener(() => {
                    onCloseWithReward?.Invoke();
                    StopAd();
                });
            }
            
            if (closeWithoutRewardButton) {
                closeWithoutRewardButton.onClick.AddListener(() => {
                    onCloseWithoutReward?.Invoke();
                    StopAd();
                });
            }
        }
    }
}