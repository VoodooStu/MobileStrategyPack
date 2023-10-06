using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class PrivacyScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _intro;
        [SerializeField] private TextMeshProUGUI _outro;
        [SerializeField] private ConsentLine _advertising;
        [SerializeField] private ConsentLine _analytics;
        [SerializeField] private ConsentLine _age;
        [SerializeField] private TextMeshProUGUI _acceptText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private TextMeshProUGUI _learnMore;
        [SerializeField] private Button _learnMoreButton;
        
        [SerializeField] private ScrollRect _body;
        [SerializeField] private Image _scrollbarHandle;
        [SerializeField] private GameObject[] _separators;

        private UnityAction<PrivacyConsent> _acceptCallback;
        
        public class Parameters
        {
            public Color mainColor;

            public string title; //title
            public string intro; //intro_thanks
            public string outro; //outro_thanks
            public string advertisingText; //consent_advertising
            public UnityAction advertisingAction;
            public bool advertisingDefaultValue;
            public bool advertisingActive;
            public string  analyticsText; //consent_analytics
            public UnityAction analyticsAction;
            public bool analyticsDefaultValue;
            public bool analyticsActive;
            public string  ageText; //pgpd_sixteen
            public UnityAction ageAction;
            public bool ageDefaultValue;
            public string accept; //play
            public UnityAction<PrivacyConsent> acceptCallback;
            public string learnMore; //more
            public UnityAction learnMoreAction;
            public bool automaticallyAccept;
            
            public bool isSeparatorsActive;
        }

        public void Initialize(Parameters p)
        {
            _title.text = p.title;
            _intro.text = p.intro;
            _outro.text = p.outro;
            _advertising.Initialize(new ConsentLine.Parameters {
                text = p.advertisingText,
                buttonAction = p.advertisingAction,
                isChecked = p.advertisingDefaultValue,
                toggleEvent = null
            });
            _advertising.gameObject.SetActive(p.advertisingActive);
            _analytics.Initialize(new ConsentLine.Parameters {
                text = p.analyticsText,
                buttonAction = p.analyticsAction,
                isChecked = p.analyticsDefaultValue,
                toggleEvent = null
            });
            _analytics.gameObject.SetActive(p.analyticsActive);
            _age.Initialize(new ConsentLine.Parameters {
                text = p.ageText,
                buttonAction = p.ageAction,
                isChecked = p.ageDefaultValue,
                toggleEvent = (b)=>{_acceptButton.interactable = b;}
            });
            _acceptText.text = p.accept;
            _acceptButton.GetComponent<Image>().color = p.mainColor;
            _acceptCallback = p.acceptCallback;
            _acceptButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(AcceptPrivacy);
            _acceptButton.interactable = p.ageDefaultValue;
            _learnMore.text = p.learnMore;
            _learnMore.color = p.mainColor;
            _learnMoreButton.onClick.RemoveAllListeners();
            _learnMoreButton.onClick.AddListener(p.learnMoreAction);
            _scrollbarHandle.color = p.mainColor;
            foreach (var separator in _separators) {
                separator.SetActive(p.isSeparatorsActive);
            }
            if (p.automaticallyAccept) AcceptPrivacy();
        }

        private void AcceptPrivacy()
        {
            PrivacyConsent consent = new PrivacyConsent {
                adsConsent = _advertising.IsChecked(),
                analyticsConsent = _analytics.IsChecked(),
                isSixteenOrOlder = _age.IsChecked()
            };
            _acceptCallback.Invoke(consent);
        }
    }
}
