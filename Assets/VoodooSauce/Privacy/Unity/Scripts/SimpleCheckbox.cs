using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Image))]
    public class SimpleCheckbox : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Sprite _uncheckedSprite;
        [SerializeField]
        private Sprite _checkedSprite;
        
        private bool _isLocked;
        private bool _checked;
        private Image _image;
        private Toggle.ToggleEvent _onToggle;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _onToggle = new Toggle.ToggleEvent();
        }

        public void Initialize(bool isChecked, bool isLocked, UnityAction<bool> toggleEvent = null)
        {
            _isLocked = isLocked;
            if (_isLocked) {
                SetChecked(false);
                _image.color = new Color(1, 1, 1, .5f);
            } else {
                SetChecked(isChecked);
                _onToggle.RemoveAllListeners();
                if (toggleEvent != null) _onToggle.AddListener(toggleEvent);
            }
        }
 

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isLocked) SetChecked(!_checked);
        }

        public bool IsChecked() => _checked;

        private void SetChecked(bool value)
        {
            _checked = value;
            _image.sprite = _checked ? _checkedSprite : _uncheckedSprite;
            if (_onToggle != null) _onToggle.Invoke(_checked);
        }
    }
}