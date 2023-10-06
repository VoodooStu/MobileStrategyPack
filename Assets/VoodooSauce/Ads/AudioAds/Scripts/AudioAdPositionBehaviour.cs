using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Common.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [RequireComponent(typeof(RectTransform))]
    public class AudioAdPositionBehaviour: MonoBehaviour, IAudioAdPositionBehaviour
    {
        private const string TAG = "AudioAdPositionBehaviour";

        public Canvas Canvas => _canvas;
        public RectTransform RectTransform => _rectTransform;
        public bool IsActiveAndEnabled {
            get {
                // when the user opens the the scene containing the prefab and then opens another one, the prefab is destroyed
                // hence, we need to secure the access to the gameObject to avoid exceptions
                try {
                    return isActiveAndEnabled;                    
                } catch (Exception e) {
                    VoodooLog.LogWarning(Module.ADS, TAG, $"Unhandled Exception when accessing the AudioAdPositionBehaviour status: {e.Message}");
                    return false;
                }
            }
        }

        private Canvas _canvas;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            if (!PlatformUtils.UNITY_EDITOR)
            {
                // No visual in devices.
                GetComponent<Image>().enabled = false;
                transform.GetChild(0).gameObject.SetActive(false);
            }

            _canvas = GetComponentInParent<Canvas>();
            if (Canvas == null)
            {
                VoodooLog.LogError(Module.ADS, TAG, "The AudioAdPosition prefab must be placed inside a UI Canvas.");
            }

            _rectTransform = GetComponent<RectTransform>();

            AudioAdsManager.Instance.LinkAudioAdPositionToPrefab(this);
        }
    }
}