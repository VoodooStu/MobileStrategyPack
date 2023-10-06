using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Voodoo.Sauce.Core
{
    
    /// <summary>
    ///     Used as callback from IAP process
    /// </summary>
    [Preserve]
    public interface IPurchaseDelegateWithInfo: IPurchaseDelegateBase
    {
        /// <summary>
        ///     Called whenever a purchase is complete.
        ///     You should reward the user with the purchase's content in your implementation.
        ///     Use the given ProductReceivedInfo to track the productId or transactionId
        /// </summary>
        /// <param name="productReceivedInfo">
        /// It returns the product info of the purchased products, the class it selves
        /// represents information received from the store
        /// </param>
        /// <param name="purchaseValidation">Various data on the purchase</param>
        void OnPurchaseComplete(ProductReceivedInfo productReceivedInfo, PurchaseValidation purchaseValidation);
        
        /// <summary>
        ///     Called whenever a purchase is complete.
        ///     You should handle gracefully any suspicious failure reason (ie: something else than user cancellation) in your
        ///     implementation.
        /// </summary>
        /// <param name="reason">The reason returned for the failure</param>
        /// <param name="productReceivedInfo">
        /// It returns the product info of the purchased products, the class it selves represents information received
        /// from the store.
        /// On some occasions where the UnityPurchasing API is not returning any product to the internal callback
        /// this callback will pass productReceivedInfo with null value
        /// </param>
        void OnPurchaseFailure(VoodooPurchaseFailureReason reason, [CanBeNull] ProductReceivedInfo productReceivedInfo);
    }
}