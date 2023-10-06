using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Extension;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class EventConsoleSessionItem : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Transform eventsContainer;
        [SerializeField] private Button copyButton;

        private int _sessionCount;
        private string _sessionId;

        public void Initialize(int sessionCount, string sessionId)
        {
            _sessionCount = sessionCount;
            _sessionId = sessionId;
            SetupUi();
        }
        
        private void Start()
        {
            SetupUi();
        }

        private void SetupUi()
        {
            if(title != null) 
                title.text = "Session " + _sessionCount + ": " + _sessionId;
            if(copyButton != null) 
                copyButton.onClick.AddListener(() => _sessionId.CopyToClipboard());
        }

        public Transform GetContainer => eventsContainer;
    }
}