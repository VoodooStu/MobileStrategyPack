using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger
{
    public class DebuggerPopupScreen : MonoBehaviour
    {
        private const string PREFIX_IDFV = "Idfv: ";
        private const string PREFIX_IDFA = "Idfa: ";
        
        [SerializeField] private Text title;
        
        [SerializeField] private Text message;
        [SerializeField] private IdSectionDebugger idfvSectionDebugger;
        [SerializeField] private IdSectionDebugger idfaSectionDebugger;
        
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;

        private DebuggerPopupConfig _currentConfig;
        private Action _closeCallback;

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
        }

        public void Show(DebuggerPopupConfig config, Action closeCallback = null)
        {
            Open();
            _closeCallback = closeCallback;
            _currentConfig = config;
            
            title.text = _currentConfig.title;
            message.text = _currentConfig.message;

            confirmButton.gameObject.SetActive(_currentConfig.showConfirmButton);
            cancelButton.gameObject.SetActive(_currentConfig.showCancelButton);

            cancelButton.GetComponentInChildren<Text>().text = config.cancelText;
            confirmButton.GetComponentInChildren<Text>().text = config.confirmText;

            idfvSectionDebugger.UpdateDisplay(_currentConfig.showIdfv, PREFIX_IDFV, _currentConfig.idfv);
            idfaSectionDebugger.UpdateDisplay(_currentConfig.showIdfa, PREFIX_IDFA, _currentConfig.idfa);
        }

        private void OnCancelButtonClicked()
        {
            Close();
            _currentConfig.cancelCallback?.Invoke();
        }

        private void OnConfirmButtonClicked()
        {
            Close();
            _currentConfig.confirmCallback?.Invoke();
        }

        private void Open()
        {
            gameObject.SetActive(true);
        }

        private void Close()
        {
            _closeCallback?.Invoke();
            gameObject.SetActive(false);
        }
    }
    
    public class DebuggerPopupConfig
    {
        public string title = "Warning";
        public string message;
        
        public bool showCancelButton = true;
        public string cancelText = "Cancel";
        public Action cancelCallback;
        
        public bool showConfirmButton = true;
        public string confirmText = "Confirm";
        public Action confirmCallback;

        public bool showIdfv = false;
        public string idfv = "00000000-0000-0000-0000-000000000000";
        
        public bool showIdfa = false;
        public string idfa = "00000000-0000-0000-0000-000000000000";
    }
}
