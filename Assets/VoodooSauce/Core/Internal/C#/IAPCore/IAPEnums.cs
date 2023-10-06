// These enumerations and classes aren't in the "Voodoo.Sauce.Internal.Scripts" namespace because they are used by game developers across the VoodooSauce class.
public enum RestorePurchasesResult
{
    FailNotIphone,
    FailStoreNotInitialized,
    FailAppleStoreExtNotFound,
    FailRestorationProcess,
    SuccessRestorationProcess,
    IAPModuleNotInstalled
}

/// <summary>
/// Clones the <see cref="UnityEngine.Purchasing.InitializationFailureReason"/> enumeration and adds some VoodooSauce related values.
/// More than that this enumeration permits to break the dependency between VoodooSauce and UnityPurchasing.
/// </summary>
public enum VoodooInitializationFailureReason
{
    PurchasingUnavailable,
    NoProductsAvailable,
    AppNotKnown,
    IAPModuleNotInstalled,
    IAPModuleDisabled
}

/// <summary>
/// Clones the <see cref="UnityEngine.Purchasing.PurchaseFailureReason"/> enumeration and adds some VoodooSauce related values.
/// More than that this enumeration permits to break the dependency between VoodooSauce and UnityPurchasing.
/// </summary>
public enum VoodooPurchaseFailureReason
{
    PurchasingUnavailable,
    ExistingPurchasePending,
    ProductUnavailable,
    SignatureInvalid,
    UserCancelled,
    PaymentDeclined,
    DuplicateTransaction,
    IAPValidationFailed,
    IAPValidationAbortedEmptyReceipt,
    NotInitialized,
    Unknown,
}

public enum PurchaseProductType
{
    Consumable,
    NonConsumable,
    Subscription,
    Unknown
}