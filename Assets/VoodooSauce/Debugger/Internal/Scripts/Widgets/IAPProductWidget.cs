using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.IAP;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Debugger
{
    public class IAPProductWidget : Widget
    {
        [SerializeField] GameObject _statusAvailable;
        [SerializeField] GameObject _statusMissing;
        [SerializeField] GameObject _statusBadID;
        [SerializeField] GameObject _statusOwned;
        [SerializeField] Text _productId;
        [SerializeField] Text _details;
        [SerializeField] Button _copyButton;
        [SerializeField] Button _purchaseButton;

        void Awake()
        {
            _copyButton.onClick.AddListener(() => { _productId.text.CopyToClipboard(); });
        }

        internal void Refresh(IAPDebugProduct product) 
        {
            UpdateStatus(product.status);
            SetProductID(product.id);
            SetDetails(product.price + " | " + product.type);
            UpdatePurchaseButton(product);
        }

        internal void UpdateStatus(IAPDebugProduct.Status status)
        {
            _statusAvailable.SetActive(status == IAPDebugProduct.Status.Available);
            _statusMissing.SetActive(status == IAPDebugProduct.Status.Missing);
            _statusBadID.SetActive(status == IAPDebugProduct.Status.BadID);
            _statusOwned.SetActive(status == IAPDebugProduct.Status.Owned);
        }

        public void SetProductID(string text) => _productId.text = text;
        
        public void SetDetails(string details) => _details.text = details;

        internal void UpdatePurchaseButton(IAPDebugProduct product) 
        {
            _purchaseButton.interactable = product.hasPurchaseButton;
            if (_purchaseButton.interactable)
            {
                _purchaseButton.onClick.AddListener(() => VoodooSauce.Purchase(product.id));
            }
        }
    }
}
