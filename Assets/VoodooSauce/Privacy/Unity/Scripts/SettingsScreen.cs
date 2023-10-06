using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _introText;
        [SerializeField] private SimpleCheckbox _advertisingCheckbox;
        [SerializeField] private Text _advertisingText;
        [SerializeField] private Transform _advertisingContent;
        [SerializeField] private SimpleCheckbox _analyticsCheckbox;
        [SerializeField] private Text _analyticsText;
        [SerializeField] private Transform _analyticsContent;
        [SerializeField] private GameObject _accessDataSeparator;
        [SerializeField] private Button _accessDataButton;
        [SerializeField] private Text _accessDataButtonText;
        [SerializeField] private GameObject _deleteDataSeparator;
        [SerializeField] private Button _deleteDataButton;
        [SerializeField] private Text _deleteDataButtonText;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Text _saveButtonText;
        [SerializeField] private Image _saveButtonImage;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _closeButtonText;
        [SerializeField] private Image[] _scrollbarHandles;

        [SerializeField] private PrivacyPolicyElement _privacyPolicyElementPrefab;
        
        private UnityAction<PrivacyUpdate> _saveAction;

        public class Parameters
        {
            public Color mainColor;
            public string title;
            public string intro;
            public bool advertisingValue;
            public bool isAdvertisingLocked;
            public string advertising;
            public IEnumerable<string> advertisingUrls;
            public bool analyticsValue;
            public bool isAnalyticsLocked;
            public string analytics;
            public IEnumerable<string> analyticsUrls;
            public bool isAccessDataActive;
            public UnityAction accessDataAction;
            public string accessDataText;
            public bool isDeleteDataActive;
            public UnityAction deleteDataCallback;
            public string deleteDataText;
            public UnityAction<PrivacyUpdate> saveAction;
            public string saveText;
            public UnityAction closeCallback;
            public string closeText;
        }
        
        public void Initialize(Parameters p)
        {
            _titleText.text = p.title;
            _introText.text = p.intro;
            _advertisingCheckbox.Initialize(p.advertisingValue, p.isAdvertisingLocked);
            _advertisingText.text = p.advertising;
            InstantiatePrivacyPolicyElements(_advertisingContent, p.advertisingUrls);
            _analyticsCheckbox.Initialize(p.analyticsValue, p.isAnalyticsLocked);
            _analyticsText.text = p.analytics;
            InstantiatePrivacyPolicyElements(_analyticsContent, p.analyticsUrls);
            _accessDataSeparator.SetActive(p.isAccessDataActive);
            _accessDataButton.gameObject.SetActive(p.isAccessDataActive);
            _accessDataButton.onClick.AddListener(p.accessDataAction);
            _accessDataButtonText.text = p.accessDataText;
            _deleteDataSeparator.SetActive(p.isDeleteDataActive);
            _deleteDataButton.gameObject.SetActive(p.isDeleteDataActive);
            _deleteDataButton.onClick.AddListener(p.deleteDataCallback);
            _deleteDataButtonText.text = p.deleteDataText;
            _saveAction = p.saveAction;
            _saveButton.onClick.AddListener(SavePrivacy);
            _saveButtonText.text = p.saveText;
            _saveButtonImage.color = p.mainColor;
            _closeButton.onClick.AddListener(p.closeCallback);
            _closeButtonText.text = p.closeText;
            _closeButtonText.color = p.mainColor;
            foreach (var handle in _scrollbarHandles) {
                handle.color = p.mainColor;
            }
        }
        
        private void SavePrivacy()
        {
            PrivacyUpdate consent = new PrivacyUpdate {
                adsConsent = _advertisingCheckbox.IsChecked(),
                analyticsConsent = _analyticsCheckbox.IsChecked(),
            };
            _saveAction.Invoke(consent);
        }
        
        private void InstantiatePrivacyPolicyElements(Transform content, IEnumerable<string> texts)
        {
            var textsCount = texts.Count();
            // I remove the extra UI elements
            if (content.childCount > textsCount)
            {
                for (int childIndex = textsCount; childIndex < content.childCount; ++childIndex)
                {
                    Destroy(content.GetChild(childIndex));
                }
            }

            // And I update or add new UI elements
            var index = 0;
            foreach (string text in texts)
            {
                PrivacyPolicyElement privacyPolicyElement = index < content.childCount
                    ? content.GetChild(index).GetComponent<PrivacyPolicyElement>()
                    : Instantiate(_privacyPolicyElementPrefab, content);
                privacyPolicyElement.Initialize(new PrivacyPolicyElement.ConsentLineParameters
                {
                    text = text,
                    buttonAction = () => Application.OpenURL(text)
                });
                index++;
            }
        }
    }
}
