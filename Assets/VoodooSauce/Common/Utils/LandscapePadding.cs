using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Common.Utils
{
    public class LandscapePadding : MonoBehaviour
    {
        public LandscapePaddingData preset;
        [Tooltip("If preset is filled, this value will not be used.")]
        public RectOffset landscapePadding;

        private RectOffset _defaultPadding;
        private bool _isLandscape;
        private LayoutGroup _layoutGroup;

        private void Awake()
        {
            _layoutGroup = GetComponent<LayoutGroup>();
            _defaultPadding = _layoutGroup.padding;

            _isLandscape = IsLandscape();
            if (_isLandscape) {
                AddPadding();
            }
        }

        private void Update()
        {
            bool newValue = IsLandscape();
            if (_isLandscape != newValue) {
                _isLandscape = newValue;
            
                if (_isLandscape) {
                    AddPadding();
                } else {
                    RemovePadding();
                }
            }
        }

        private bool IsLandscape() => Screen.width > Screen.height;

        private void AddPadding()
        {
            if (_layoutGroup) {
                if (preset) {
                    _layoutGroup.padding = preset.landscapePadding;
                } else {
                    _layoutGroup.padding = landscapePadding;
                }
            }
        }
    
        private void RemovePadding()
        {
            if (_layoutGroup) {
                _layoutGroup.padding = _defaultPadding;
            }
        }
    }
}
