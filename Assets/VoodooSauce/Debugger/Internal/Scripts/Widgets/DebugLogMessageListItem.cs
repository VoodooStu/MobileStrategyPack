using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Debugger;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugLogMessageListItem : MonoBehaviour
    {
        [SerializeField] private Text _headerText;
        [SerializeField] private Text _bodyText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;

        private Action<LogMessage> _callback;
        public LogMessage LogMessage { get; private set; }

        public void Initialize(Color color, LogMessage logMessage, Action<LogMessage> callback)
        {
            LogMessage = logMessage;
            _callback = callback;
            
            _headerText.text = '[' + logMessage.time + "]  " + logMessage.message;
            _bodyText.text = logMessage.stacktrace;

            _image.color = color; 
            
            _button.onClick.AddListener(OnButtonClick); 
        }

        private void OnButtonClick()
        {
            _callback?.Invoke(LogMessage);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners(); 
        }
    }
}