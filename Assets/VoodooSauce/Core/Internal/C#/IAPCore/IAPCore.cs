using System;
using System.Collections.Generic;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.IntegrationCheck;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// This is the core class of the InAppPurchase module.
    /// A "core" module class defines what the module's methods do when the module is not available/implemented.
    /// </summary>
    internal class IAPCore : IModule
    {
        private const string TAG = "IAPCore";
        
        protected readonly PurchaseDelegateList purchaseDelegateList = new PurchaseDelegateList();
        private bool _isInitCalled;

        internal readonly bool IsEnabled = VoodooSauce.IsIAPEnabled();

        internal virtual bool IsInstalled() => false;

        internal virtual List<ProductReceivedInfo> GetProducts() => null;

        internal virtual ProductReceivedInfo GetProductWithId(string productId) => null;

        public virtual List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings) => null;

        internal virtual void Initialize(VoodooSettings voodooSettings)
        {
            _isInitCalled = true;

            if (!IsEnabled)
            {
                purchaseDelegateList.OnInitializeFailure(VoodooInitializationFailureReason.IAPModuleDisabled);
                return;
            }

            if (!IsInstalled())
            {
                purchaseDelegateList.OnInitializeFailure(VoodooInitializationFailureReason.IAPModuleNotInstalled);
            }
        }

        internal virtual void RefreshIapSubscriptionInfo() { }

        internal virtual void AddIapPurchaseDelegate(IPurchaseDelegateWithInfo purchaseDelegate)
        {
            //This method should never be called. We should be able to empty it.
            if (!IsEnabled)
            {
                purchaseDelegate.OnInitializeFailure(VoodooInitializationFailureReason.IAPModuleDisabled);
                return;
            }

            purchaseDelegateList.Add(purchaseDelegate);
            
            if (_isInitCalled)
            {
                purchaseDelegate.OnInitializeFailure(VoodooInitializationFailureReason.IAPModuleNotInstalled);
            }
        }

        internal void RemoveIapPurchaseDelegate(IPurchaseDelegateWithInfo purchaseDelegate)
        {
            purchaseDelegateList.Remove(purchaseDelegate);
        }

        internal virtual void BuyIAPProduct(string productId, IPurchaseDelegateWithInfo caller, IPurchaseValidator customPurchaseValidator) {}

        internal virtual void RestoreIAPProduct(Action<RestorePurchasesResult> callback)
        {
            callback.Invoke(RestorePurchasesResult.IAPModuleNotInstalled);
        }

        internal virtual string GetLocalizedProductPrice(string productId) => string.Empty;

        internal virtual bool IsSubscribedProduct(string productId) => false;

        internal virtual bool IsSubscribedProduct() => false;

        internal virtual SubscriptionInfoContainer GetSubscriptionDetails(string productId) => null;

        internal virtual LocalizedPriceInfo GetLocalizedProductPriceInfo(string productId)
        {
            VoodooLog.LogWarning(Module.IAP, TAG,
                "You can't get the price of a product because the IAP module isn't installed.");
            return null;
        }
    }
}