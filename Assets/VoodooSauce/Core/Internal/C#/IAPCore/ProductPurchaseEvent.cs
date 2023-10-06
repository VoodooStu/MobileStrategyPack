namespace Voodoo.Sauce.Core
{
    public struct ProductPurchaseEvent
    {
        private const string ConsumableProductEventName = "purchase_consumable";
        private const string NotConsumableProductEventName = "purchase_nonconsumable";
        private const string SubscriptionProductEventName = "purchase_subscription";
        private const string UnknownProductEventName = "product_unknown";

        private const string NoAdsProductEventName = "purchase_noads";
        /*
         internal const string FailedPurchaseEventName = "purchase_failed";
         internal const string NotVerifiedPurchaseEventName = "purchase_notverified";
         internal const string UnknownPurchaseEventName = "purchase_unknown";
        */

        internal static string GetAdjustName(string productId, PurchaseProductType productType, string noAdsBundleId)
        {
            if (productId == noAdsBundleId) return NoAdsProductEventName;

            switch (productType)
            {
                case PurchaseProductType.Consumable:
                    return ConsumableProductEventName;
                case PurchaseProductType.NonConsumable:
                    return NotConsumableProductEventName;
                case PurchaseProductType.Subscription:
                    return SubscriptionProductEventName;
                default:
                    return UnknownProductEventName;
            }
        }
    }
}