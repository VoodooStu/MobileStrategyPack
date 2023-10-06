using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    [Serializable]
    public class BoolEvent : UnityEvent<bool>
    {
    }

    public class DebugToggleButton : Widget
    {
        [SerializeField] Toggle _toggle;

        [SerializeField] Text _label;
        [SerializeField] Image _image;
        [SerializeField] Sprite _toggleOn;
        [SerializeField] Sprite _toggleOff;

        public void Initialize(string title, bool value, Action<bool> onValueChanged) 
        {
            SetTitle(title);
            SetValue(value);
            ChangeUI(value);
            SetCallback(onValueChanged);
        }

        public void SetTitle(string title) => _label.text = title;

        public void SetValue(bool isOn) => _toggle.isOn = isOn;
        
        private void ChangeUI(bool isOn) => _image.sprite = isOn ? _toggleOn : _toggleOff;
            
        public void SetCallback(Action<bool> callback)
        {
            _toggle.onValueChanged.RemoveAllListeners();

            _toggle.onValueChanged.AddListener(ChangeUI);
            if (callback != null) {
                _toggle.onValueChanged.AddListener(value => callback(value));
            }
        }
    }
}
