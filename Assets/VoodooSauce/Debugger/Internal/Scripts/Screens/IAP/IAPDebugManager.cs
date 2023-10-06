using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Debugger;
using Voodoo.Sauce.Internal.IAP;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.IAP
{
    internal class IAPDebugManager : IPurchaseDelegateWithInfo
    {
        private const string CALLBACK_PURCHASE_SUCCESS_WITH_TRANSACTION_ID_FORMAT = "You have successfully bought this product. With transaction ID: {0}";

        private const string LIGHT_MESSAGE_NO_PRODUCTS =
            "There is no product in this project.\nYou can add them in the VoodooSettings.";

        private const string LIGHT_MESSAGE_INITIALIZING = "Waiting for the In-App Purchase initialization...";
        private const string LIGHT_MESSAGE_DISABLED = "In-App Purchase isn't activated on your project.";
        private const string LIGHT_MESSAGE_NOT_INSTALLED = "In-App Purchase isn't installed on your project.";
        private const string ERROR_MESSAGE_HEADER = "In-App purchase initialization has failed.\n";

        private const string ERROR_MESSAGE_APP_NOT_KNOWN =
            ERROR_MESSAGE_HEADER + "Is your App correctly uploaded on the relevant publisher console?";

        private const string ERROR_MESSAGE_PURCHASING_UNAVAILABLE = ERROR_MESSAGE_HEADER + "Billing is disabled.";
        private const string ERROR_MESSAGE_NO_PRODUCTS = ERROR_MESSAGE_HEADER + "No products available for purchase.";

        private readonly IAPCore _iap;
        private readonly IAPDebugScreen _screen;
        private bool _initializationStarted;
        private bool _initialized;
        private VoodooSettings _voodooSettings;

        internal IAPDebugManager(IAPDebugScreen screen, VoodooSettings voodooSettings)
        {
            _screen = screen;
            _iap = VoodooSauceCore.GetInAppPurchase();
            _voodooSettings = voodooSettings;
        }

        internal void OnScreenDisplayed()
        {
            if (!_initializationStarted)
            {
                Initialize();
            }
            else if(_initialized)
            {
                RefreshDisplayedProducts();
            }

            RefreshVoodooPremiumToggle();
        }
        
        private void Initialize()
        {
            _screen.DisplayLightMessage(LIGHT_MESSAGE_INITIALIZING);
            _screen.EnableRestoreButton(PlatformUtils.UNITY_IOS || PlatformUtils.UNITY_ANDROID);
            VoodooSauce.RegisterPurchaseDelegate(this);
            _initializationStarted = true;
        }

        private IAPDebugProduct.Status GetProductStatus(ProductReceivedInfo product)
        {
            if (PlatformUtils.UNITY_ANDROID && !ProductDescriptor.IsValidAndroidProductId(product.ProductId))
            {
                return IAPDebugProduct.Status.BadID;
            }

            if (!product.IsAvailable)
            {
                return IAPDebugProduct.Status.Missing;
            }

            if (!string.IsNullOrEmpty(product.TransactionID))
            {
                return IAPDebugProduct.Status.Owned;
            }

            return IAPDebugProduct.Status.Available;
        }

        private void RefreshDisplayedProducts()
        {
            List<IAPDebugProduct> productList = _iap.GetProducts()?.Select(
                product => new IAPDebugProduct(
                    id: product.ProductId,
                    type: product.ProductType.ToString(),
                    price: product.LocalizedPriceInfo.ToString(),
                    status: GetProductStatus(product),
                    hasPurchaseButton: product.IsAvailable)
            ).ToList();
            
            if (productList != null && productList.Count > 0)
            {
                _screen.CreateProductsButtons(productList);
            }
            else
            {
                _screen.DisplayLightMessage(LIGHT_MESSAGE_NO_PRODUCTS);
            }
        }

        internal void OnVoodooPremiumToggle(bool isOn)
        {
            VoodooPremium.SetEnabledPremium(isOn);
            if (!isOn) {
                VoodooSauce.ShowBanner();
            }
        }

        private void RefreshVoodooPremiumToggle()
        {
            _screen.SetVoodooPremiumToggleState(VoodooSauce.IsPremium());
        }

        #region IPurchaseDelegate

        public void OnInitializeSuccess()
        {
            RefreshDisplayedProducts();
            _initialized = true;
        }

        public void OnInitializeFailure(VoodooInitializationFailureReason reason)
        {
            switch (reason)
            {
                case VoodooInitializationFailureReason.PurchasingUnavailable:
                    _screen.DisplayErrorMessage(ERROR_MESSAGE_PURCHASING_UNAVAILABLE);
                    break;
                case VoodooInitializationFailureReason.AppNotKnown:
                    _screen.DisplayErrorMessage(ERROR_MESSAGE_APP_NOT_KNOWN);
                    break;
                case VoodooInitializationFailureReason.NoProductsAvailable:
                    _screen.DisplayErrorMessage(ERROR_MESSAGE_NO_PRODUCTS);
                    break;
                case VoodooInitializationFailureReason.IAPModuleNotInstalled:
                    _screen.DisplayLightMessage(LIGHT_MESSAGE_NOT_INSTALLED);
                    break;
                case VoodooInitializationFailureReason.IAPModuleDisabled:
                    _screen.DisplayLightMessage(LIGHT_MESSAGE_DISABLED);
                    break;
            }
        }

        public void OnPurchaseComplete(ProductReceivedInfo productInfo, PurchaseValidation purchaseValidation)
        {
            // The popup is only displayed when the user is in the IAP debugger screen.
            if (_screen.gameObject.activeInHierarchy) {
                var message = String.Format(CALLBACK_PURCHASE_SUCCESS_WITH_TRANSACTION_ID_FORMAT, productInfo.TransactionID); 
                Debugger.Debugger.DisplayPopup(message);
            }

            RefreshDisplayedProducts();

            if (productInfo.ProductId == _voodooSettings.NoAdsBundleId)
            {
                VoodooPremium.SetEnabledPremium();
                RefreshVoodooPremiumToggle();
            }
        }

        public void OnPurchaseFailure(VoodooPurchaseFailureReason reason, [CanBeNull] ProductReceivedInfo productInfo)
        {
            // The popup is only displayed when the user is in the IAP debugger screen.
            if (!_screen.gameObject.activeInHierarchy) return;

            string dialogMessage;
            switch (reason)
            {
                case VoodooPurchaseFailureReason.PaymentDeclined:
                    dialogMessage = "This transaction has stopped because the payment failed.";
                    break;
                case VoodooPurchaseFailureReason.DuplicateTransaction:
                    dialogMessage = "This transaction has failed because you already did it.";
                    break;
                case VoodooPurchaseFailureReason.PurchasingUnavailable:
                    dialogMessage = "The In-App Purchasing is not available.";
                    break;
                case VoodooPurchaseFailureReason.ExistingPurchasePending:
                    dialogMessage = "This transaction has stopped because the same one is currently pending.";
                    break;
                case VoodooPurchaseFailureReason.ProductUnavailable:
                    dialogMessage = "You can't buy this product because it's unavailable.";
                    break;
                case VoodooPurchaseFailureReason.SignatureInvalid:
                    dialogMessage =
                        "This purchase has been canceled because its signature validation has failed.\nDid you correctly update the GoogleTangle?";
                    break;
                case VoodooPurchaseFailureReason.UserCancelled:
                    dialogMessage = "You have canceled this purchase.";
                    break;
                case VoodooPurchaseFailureReason.IAPValidationFailed:
                    dialogMessage = "This purchase has been canceled because its validation has failed.";
                    break;
                case VoodooPurchaseFailureReason.IAPValidationAbortedEmptyReceipt:
                    dialogMessage =
                        "This purchase has been canceled because its validation has failed.\nThe transaction has had no receipt.";
                    break;
                default:
                    dialogMessage = "An unknown error has just happened, this transaction has failed.";
                    break;
            }

            
            dialogMessage += "\nProduct ID: " + productInfo?.ProductId;
            if (!string.IsNullOrEmpty(productInfo?.TransactionID))
            {
                dialogMessage += "\nTransaction ID: " + productInfo.TransactionID;
            }
            
            RefreshVoodooPremiumToggle();
            Debugger.Debugger.DisplayPopup(dialogMessage);
        }

        #endregion
    }
}