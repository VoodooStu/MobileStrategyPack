using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voodoo.Sauce.Privacy.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class OfferWallScreen : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _bodyText;
        [SerializeField] private Button _consentButton;
        [SerializeField] private Text _consentButtonText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Text _purchaseButtonText;
        [SerializeField] private Button _limitedPlayTimeButton;
        [SerializeField] private Text _limitedPlayTimeButtonText;
        [SerializeField] private GameObject _popupFrame;

        public class Parameters
        {
            public string title;
            public string body;
            public UnityAction consentAction;
            public string consentText;
            public UnityAction purchaseAction;
            public string purchaseText;
            public UnityAction limitedPlaytimeAction;
            public string limitedPlaytimeText;
        }

        public void Initialize(Parameters p)
        {
            _titleText.text = p.title;
            _bodyText.text = p.body;
            _consentButton.onClick.RemoveAllListeners();
            _consentButton.onClick.AddListener(p.consentAction);
            _consentButtonText.text = p.consentText;
           _purchaseButton.onClick.RemoveAllListeners();
           _purchaseButton.onClick.AddListener(p.purchaseAction);
           _purchaseButtonText.text = p.purchaseText;
           _limitedPlayTimeButton.onClick.RemoveAllListeners();
           _limitedPlayTimeButton.onClick.AddListener(p.limitedPlaytimeAction);
           _limitedPlayTimeButtonText.text = p.limitedPlaytimeText;
           _limitedPlayTimeButton.gameObject.SetActive(p.limitedPlaytimeAction != null);
           _popupFrame.SetActive(true);
           RefreshHierarchySize(transform);
        }

        private static void RefreshHierarchySize(Transform transform)
        {
            if (transform.GetComponent<ContentSizeFitter>() != null) 
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
            }
            foreach (var contentSizeFitter in transform.GetComponentsInChildren<ContentSizeFitter>()) 
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentSizeFitter.GetComponent<RectTransform>());
            }
        }

        public void HidePurchasePopup()
        {
            _popupFrame.SetActive(false);
        }
        
        public void ShowPurchasePopup()
        {
            _popupFrame.SetActive(true);
        }
    }
}
