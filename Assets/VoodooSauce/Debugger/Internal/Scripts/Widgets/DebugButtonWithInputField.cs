using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.DebugScreen;

namespace Voodoo.Sauce.Debugger
{
    public class DebugButtonWithInputField : Widget, IDebugRefreshable
    {
        [SerializeField] Button _button;
        [SerializeField] Text _buttonText;
        [SerializeField] InputField _inputField;
        [SerializeField] Text _inputFieldPlaceholderText;

        private Action _callback;
        private Action<string> _callbackWithText;
        private Action<string> _inputCallback;
        private Func<string> _refreshFunc;

        public bool Interactable {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
            _inputField.onEndEdit.RemoveListener(OnEndEdit);
        }

        private void OnButtonClick()
        {
            _callback?.Invoke();
            _callbackWithText?.Invoke(_inputField.text);
        }

        private void OnEndEdit(string value)
        {
            _inputCallback?.Invoke(value);
        }

        public void SetTitle(string title)
        {
            _buttonText.text = title;
        }

        public void SetColor(Color color)
        {
            _button.image.color = color;
        }

        public void SetButtonCallback(Action callback)
        {
            _callback = callback;
        }
        
        public void SetButtonCallback(Action<string> callback)
        {
            _callbackWithText = callback;
        }
        
        public void SetInputCallback(Action<string> callback)
        {
            _inputCallback = callback;
        }

        public void SetInputField(bool active, string placeholder = "")
        {
            if (!active) {
                _inputField.gameObject.SetActive(false);
                return;
            }

            _inputFieldPlaceholderText.text = placeholder;
        }

        public void SetRefreshFunc(Func<string> func)
        {
            _refreshFunc = func;
            Refresh();
        }

        public void SetEnable(bool enable) 
        {
            _button.interactable = enable;
        }

        public void Refresh()
        {
            SetCurrentText(_refreshFunc?.Invoke());
        }

        private void SetCurrentText(string text)
        {
            _inputField.text = text;
        }
    }
}
