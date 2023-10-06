using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    public class DebugCheckboxButton : Widget
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Text label;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color checkedColor;

        private Action<bool> _callback;

        public bool Toggled => toggle.isOn;

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(SetValue);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(SetValue);
        }

        public void SetCallback(Action<bool> callback)
        {
            _callback = callback;
        }

        public void SetValue(bool value) => SetValueAndCallCallback(value, true);

        public void SetValueWithoutCallingCallback(bool value) => SetValueAndCallCallback(value, false);

        internal void SetValueAndCallCallback(bool value, bool callCallback)
        {
            toggle.isOn = value;
            backgroundImage.color = value ? checkedColor : defaultColor;

            if (callCallback) {
                _callback?.Invoke(value);
            }
        }

        public void SetLabel(string text)
        {
            label.text = text;
        }
    }
}