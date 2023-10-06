using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal
{
    //This class is used to detect if the user purchased the ads free version
    [Preserve]
    public class NoAdsPurchaseDelegate : IPurchaseDelegateWithInfo
    {
        public NoAdsPurchaseDelegate(Action onPurchaseComplete, Action onPurchaseFailure)
        {
            _OnPurchaseComplete = onPurchaseComplete;
            _OnPurchaseFailure = onPurchaseFailure;
        }

        private Action _OnPurchaseComplete;
        private Action _OnPurchaseFailure;

        public void OnInitializeSuccess()
        {
        }

        public void OnInitializeFailure(VoodooInitializationFailureReason reason)
        {
        }

        public void OnPurchaseComplete(ProductReceivedInfo productInfo, PurchaseValidation purchaseValidation)
        {
            if (productInfo.ProductId == VoodooSettings.Load().NoAdsBundleId)
            {
                VoodooSauce.UnregisterPurchaseDelegate(this);
                _OnPurchaseComplete.Invoke();
            }
        }

        public void OnPurchaseFailure(VoodooPurchaseFailureReason reason, [CanBeNull] ProductReceivedInfo productInfo)
        {
            if (productInfo?.ProductId == VoodooSettings.Load().NoAdsBundleId)
            {
                VoodooSauce.UnregisterPurchaseDelegate(this);
                _OnPurchaseFailure.Invoke();
            }
        }
    }
}