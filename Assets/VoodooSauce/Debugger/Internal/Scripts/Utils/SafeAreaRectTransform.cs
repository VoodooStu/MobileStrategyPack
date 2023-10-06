using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.DebugScreen
{
    /// <summary>
    /// This Unity component can be attached to any U.I. GameObject (with a RectTransform).
    /// This script will scale it to correspond to the device's safe area.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRectTransform : MonoBehaviour
    {
        [SerializeField] private bool ignoreNotch;
        
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var anchorMin = Screen.safeArea.position;
            var anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            if (ignoreNotch)
            {
                anchorMax.y += Screen.height - Screen.safeArea.max.y;
            }
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}