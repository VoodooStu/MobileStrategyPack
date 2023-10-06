using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Debugger;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugLogMessagePopup : MonoBehaviour
    {
        [SerializeField]
        private Text _headerText;

        [SerializeField]
        private Text _bodyText;

        [SerializeField]
        private Button _closeScreen;

        private void Awake()
        {
            _closeScreen.onClick.AddListener(Hide); 
        }

        public void Show(LogMessage logMessage)
        {
            _headerText.text = '[' + logMessage.time + "]  " + logMessage.message;
            _bodyText.text = logMessage.stacktrace;
            
            gameObject.SetActive(true); 
        }

        public void Hide()
        {
            gameObject.SetActive(false); 
        }

        private void OnDestroy()
        {
            _closeScreen.onClick.RemoveAllListeners(); 
        }
    }
}