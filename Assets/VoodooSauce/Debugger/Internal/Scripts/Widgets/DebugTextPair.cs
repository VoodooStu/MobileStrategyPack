using System;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.DebugScreen;
using Voodoo.Sauce.Internal.Extension;

#pragma warning disable 0649

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger.Widgets
{
    public class DebugTextPair : Widget, IDebugRefreshable
    {
        [SerializeField]
        private Text _key;
        [SerializeField]
        private Text _value;
        [SerializeField]
        private Button _copyButton;

        private string _valueToCopy = "";

        private Func<string> _refreshCallback;
        private Func<string> _refreshCopyCallback;

        private void Start()
        {
            if (_copyButton != null) {
                _copyButton.onClick.AddListener(OnCopyButtonClicked);
            }
        }

        private void OnCopyButtonClicked()
        {
            _valueToCopy.CopyToClipboard();
        }

        public void SetValueToCopy(string value)
        {
            _valueToCopy = value;
        }

        public void SetLabelBestFitTxt(bool bestFit)
        {
            _key.resizeTextForBestFit = bestFit;
        }
        
        public void SetLabel(string label)
        {
            if (!string.IsNullOrEmpty(label)) {
                _key.gameObject.SetActive(true);
                _key.text = label;
            }
            else
            {
                _key.gameObject.SetActive(false);
            }
        }

        public void SetValue(string value)
        {
            if (_value != null) {
                _value.gameObject.SetActive(true);
                _value.text = value;
            }
        }

        public void SetStyle(Color color, FontStyle style)
        {
            if (_key != null) {
                _key.color = color;
                _key.fontStyle = style;
            }

            if (_value != null) {
                _value.color = color;
                _value.fontStyle = style;
            }
        }

        public void ShowCopyButton(bool value)
        {
            if (_copyButton != null) {
                _copyButton.gameObject.SetActive(value);
            }
        }

        public void SetRefreshFunc(Func<string> callback)
        {
            _refreshCallback = callback;
        }

        public void SetRefreshToCopyFunc(Func<string> callback)
        {
            _refreshCopyCallback = callback;
        }

        public void Refresh()
        {
            if (_value) {
                _value.text = _refreshCallback?.Invoke();
            }

            _valueToCopy = _refreshCopyCallback?.Invoke();
        }
    }
}