
using System;
using UnityEngine.Scripting;
using Voodoo.Sauce.Core;

[Preserve]
[Obsolete("IPurchaseDelegate is deprecated, please use IPurchaseDelegateWithInfo instead. We will be removing" +
          "IPurchaseDelegate interface and move all the method to IPurchaseDelegateWithInfo on next major version", true)]
public interface IPurchaseDelegate: IPurchaseDelegateBase
{
    /// <summary>
    ///     Calle whenever a purchase is complete.
    ///     You should reward the user with the purchase's content in your implementation. Use the given product ID to track
    ///     which purchase did complete and respond accordingly.
    /// </summary>
    /// <param name="productId">
    ///     The ID of the product that has just been successfully purchased, as registered in the
    ///     VoodooSettings file
    /// </param>
    void OnPurchaseComplete(string productId);

    /// <summary>
    ///     Called whenever a purchase is complete.
    ///     You should handle gracefully any suspicious failure reason (ie: something else than user cancellation) in your
    ///     implementation.
    /// </summary>
    /// <param name="productId">
    ///     The ID of the product that has just been successfully purchased, as registered in the
    ///     VoodooSettings file
    /// </param>
    /// <param name="reason">The reason returned for the failure</param>
    /// <param name="transactionId">Id of the IAP transaction if relevant, empty otherwise.</param>
    void OnPurchaseFailure(string productId, VoodooPurchaseFailureReason reason, string transactionId);
}