using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class ParternsScreen : MonoBehaviour
    {
        [SerializeField] private Text _headerText;
        [SerializeField] private Text _introText;
        [SerializeField] private Text _advertisingText;
        [SerializeField] private Transform _advertisingContent;
        [SerializeField] private Text _analyticsText;
        [SerializeField] private Transform _analyticsContent;
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _backButtonText;
        [SerializeField] private Image _backButtonImage;
        [SerializeField] private Image _scrollbarHandle;

        [SerializeField] private PrivacyPolicyElement _privacyPolicyElementPrefab;

        public class Parameters
        {
            public Color mainColor;
            public string header;
            public string intro;
            public string advertising;
            public IEnumerable<string> advertisingUrls;
            public string analytics;
            public IEnumerable<string> analyticsUrls;
            public UnityAction backCallback;
            public string back;
        }
        
        public void Initialize(Parameters p)
        {
            _headerText.text = p.header;
            _introText.text = p.intro;
            _advertisingText.text = p.advertising;
            InstantiatePrivacyPolicyElements(_advertisingContent, p.advertisingUrls);
            _analyticsText.text = p.analytics;
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(p.backCallback);
            InstantiatePrivacyPolicyElements(_analyticsContent, p.analyticsUrls);
            _backButtonText.text = p.back;
            _backButtonImage.color = p.mainColor;
            _scrollbarHandle.color = p.mainColor;
        }
        
        private void InstantiatePrivacyPolicyElements(Transform content, IEnumerable<string> texts)
        {
            foreach (string s in texts)
            {
                PrivacyPolicyElement privacyPolicyElement = Instantiate(_privacyPolicyElementPrefab, content);
                privacyPolicyElement.Initialize(new PrivacyPolicyElement.ConsentLineParameters {
                    text = s,
                    buttonAction = () => Application.OpenURL(s)
                });
            }
        }
    }
}
