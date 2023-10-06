using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Voodoo.Sauce.Common.Utils;

#pragma warning disable 0649

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal abstract class FakeAd : MonoBehaviour
    {
        [SerializeField]
        private GameObject rootView;

        [FormerlySerializedAs("_viewButton"),SerializeField]
        private Button viewButton;

        public Action onClick;

        protected void Awake()
        {
            viewButton.onClick.AddListener(() => {
                onClick?.Invoke();
                if (PlatformUtils.UNITY_EDITOR) {
                    VoodooLog.LogDebug(Module.ADS, "FakeAds", "The ad has been clicked");
                }
            });
        }

        public void StartAd()
        {
            rootView.SetActive(true); 
        }

        public void StopAd()
        {
            rootView.SetActive(false); 
        }

    }
}