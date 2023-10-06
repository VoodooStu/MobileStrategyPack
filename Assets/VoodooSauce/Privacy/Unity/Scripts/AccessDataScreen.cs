using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class AccessDataScreen : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _introText;
        [SerializeField] private Text _idfaTitle;
        [SerializeField] private InputField _idfa;
        [SerializeField] private Text _emailTitle;
        [SerializeField] private InputField _email;
        [SerializeField] private Button _sendRequestButton;
        [SerializeField] private Text _sendRequestButtonText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _closeButtonText;

        [Header("Debugger Buttons")]
        [SerializeField] private Button _debuggerButton0;
        [SerializeField] private Button _debuggerButton1;
        [SerializeField] private Button _debuggerButton2;

        private UnityAction<UserDetails> _sendRequestAction;

        public class Parameters
        {
            public Color mainColor;
            public string title;
            public string intro;
            public string idfaTitle;
            public string idfa;
            public string emailTitle;
            public UnityAction<UserDetails> sendRequestAction;
            public string sendRequestButtonText;
            public UnityAction closeAction;
            public string closeButtonText;
            public UnityAction tryOpenDebugger;
        }
        
        public void Initialize(Parameters p)
        {
            _titleText.text = p.title;
            _introText.text = p.intro;
            _idfaTitle.text = p.idfaTitle;
            _idfa.text = p.idfa;
            _emailTitle.text = p.emailTitle;
            _sendRequestAction = p.sendRequestAction;
            _sendRequestButton.onClick.RemoveAllListeners();
            _sendRequestButton.onClick.AddListener(SendRequest);
            _sendRequestButton.GetComponent<Image>().color = p.mainColor;
            _sendRequestButtonText.text = p.sendRequestButtonText;
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(p.closeAction);
            _closeButtonText.text = p.closeButtonText;
            _closeButtonText.color = p.mainColor;
            _debuggerButton0.interactable = true;
            _debuggerButton1.interactable = false;
            _debuggerButton2.interactable = false;
            _debuggerButton0.onClick.RemoveAllListeners();
            _debuggerButton0.onClick.AddListener(() => {
                _debuggerButton0.interactable = false;
                _debuggerButton1.interactable = true;
            });
            _debuggerButton1.onClick.RemoveAllListeners();
            _debuggerButton1.onClick.AddListener(() => {
                _debuggerButton1.interactable = false;
                _debuggerButton2.interactable = true;
            });
            _debuggerButton2.onClick.RemoveAllListeners();
            _debuggerButton2.onClick.AddListener(() => {
                _debuggerButton2.interactable = false;
                _debuggerButton0.interactable = true;
                p.tryOpenDebugger?.Invoke();
            });
        }

        private void SendRequest()
        {
            UserDetails userDetails = new UserDetails {
                email = _email.text
            };
            _sendRequestAction.Invoke(userDetails);
        }
    }
}
