using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Debugger
{
    public class IAPDebugScreen : Screen
    {
        [SerializeField] private Text lightMessage;
        [SerializeField] private Image errorIcon;
        [SerializeField] private Text errorMessage;

        private DebugToggleButton premiumToggle;
        private DebugButtonWithInputField restoreButton;
        private IAPDebugManager _manager;

        private void Awake()
        {
            _manager = new IAPDebugManager(this, VoodooSettings.Load());
        }

        private void OnEnable()
        {
            ClearMessages();
            ClearDisplay();

            premiumToggle = Toggle("VoodooPremium", VoodooSauce.IsPremium(), _manager.OnVoodooPremiumToggle);
            restoreButton = Button("Restore Purchases", () => VoodooSauce.RestorePurchases(OnRestorePurchasesSuccess));
            
            _manager.OnScreenDisplayed();
        }

        internal void SetVoodooPremiumToggleState(bool isOn)
        {
            premiumToggle.SetValue(isOn);
        }

        internal void EnableRestoreButton(bool active)
        {
            restoreButton.gameObject.SetActive(active);
        }

        internal void CreateProductsButtons(List<IAPDebugProduct> products)
        {
            if (products != null)
            {
                foreach (IAPDebugProduct product in products)
                {
                    var productWidget = WidgetUtility.InstanceOf<IAPProductWidget>(Parent);
                    productWidget.Refresh(product);
                }
            }
        }

        private void ClearMessages()
        {
            errorIcon.gameObject.SetActive(false);
            errorMessage.gameObject.SetActive(false);
            lightMessage.gameObject.SetActive(false);
        }

        internal void DisplayLightMessage(string message)
        {
            ClearMessages();
            lightMessage.gameObject.SetActive(true);
            lightMessage.text = message;
        }

        internal void DisplayErrorMessage(string message)
        {
            ClearMessages();
            errorIcon.gameObject.SetActive(true);
            errorMessage.gameObject.SetActive(true);
            errorMessage.text = message;
        }
        
        private void OnRestorePurchasesSuccess(RestorePurchasesResult result)
        {
            Debugger.DisplayPopup("Success!", $"Restore purchases successful!: RestorePurchasesResult: {result}");
        }
    }
}