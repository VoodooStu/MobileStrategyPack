using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Core
{
	public static class IAPAnalytics
	{
        private static EnvironmentSettings.Server? _environment;

        private static EnvironmentSettings.Server Environment
        {
            get
            {
                if (_environment == null)
                {
                    _environment = EnvironmentSettings.GetAnalyticsServer();
                }
                
                return _environment.Value;
            }
        }
        
        /// <summary>
        /// Method called when the user starts a purchase.
        /// It is called just before initiating the purchase with unity IAP.
        /// </summary>
        /// <param name="product">The product to be purchased.</param>
        internal static void Start(ProductReceivedInfo product)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, null, true);
            payload.purchaseTransactionId = "";
            
            AnalyticsManager.TrackPurchaseStarted(payload);
        }
        
        /// <summary>
        /// Method called when the user starts a purchase and the product doesn't exist yet.
        /// </summary>
        /// <param name="productId">The productId to be purchased.</param>
        internal static void Start(string productId)
        {
            string purchaseId = TransactionUtility.GetPurchaseUniqueId(productId, true);
            string environment = Environment == EnvironmentSettings.Server.Tech ? "Production" : "Sandbox";
            
            VoodooIAPAnalyticsInfo payload = new VoodooIAPAnalyticsInfo(purchaseId)
            {
                productId = productId,
                environment = environment,
            };
            
            AnalyticsManager.TrackPurchaseStarted(payload);
        }

        /// <summary>
        /// Method called when the purchase starts being processed by the store.
        /// It is only called once per transaction per session.
        /// It can be called multiple time for the same transaction if it's delay over multiple session  
        /// </summary>
        /// <param name="product">The product to be purchased.</param>
        internal static void Process(ProductReceivedInfo product)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, null);
            AnalyticsManager.TrackPurchaseProcessing(payload);
        }

        /// <summary>
        /// Method called when the purchase failed.
        /// </summary>
        /// <param name="product">The product that was attempted to be purchased.</param>
        /// <param name="purchaseValidation">Additional information on the failed transaction.</param>
        /// <param name="reason">The failure reason.</param>
        internal static void Failure(ProductReceivedInfo product, PurchaseValidation purchaseValidation, VoodooPurchaseFailureReason reason)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, purchaseValidation?.isRestorationPurchase);
            payload.failureReason = reason.ToString();
            AnalyticsManager.TrackPurchaseFailed(payload);
        }

        /// <summary>
        /// Method called when the purchase failed and the product isn't available.
        /// </summary>
        /// <param name="productId">The product id that was attempted to be purchased.</param>
        /// <param name="reason">The failure reason.</param>
        internal static void Failure(string productId, VoodooPurchaseFailureReason reason)
        {
            string purchaseId = TransactionUtility.GetPurchaseUniqueId(productId, false);
            string environment = Environment == EnvironmentSettings.Server.Tech ? "Production" : "Sandbox";
            
            VoodooIAPAnalyticsInfo payload = new VoodooIAPAnalyticsInfo(purchaseId)
            {
                productId = productId,
                environment = environment,
                failureReason = reason.ToString()
            };

            AnalyticsManager.TrackPurchaseFailed(payload);
        }
        
        /// <summary>
        /// Method called when the purchase succeeded
        /// </summary>
        /// <param name="product">The product that got purchased successfully.</param>
        /// <param name="purchaseValidation">Additional information on the transaction</param>
        internal static void Validate(ProductReceivedInfo product, PurchaseValidation purchaseValidation)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, purchaseValidation?.isRestorationPurchase);
            AnalyticsManager.TrackPurchaseValidated(payload);
        }
        
        /// <summary>
        /// Method called if at least one <see cref="IPurchaseDelegateWithInfo"/> is registered to the IAPManager.
        /// You can register using the <see cref="VoodooSauce.RegisterPurchaseDelegate(IPurchaseDelegateWithInfo)"/> method.
        /// </summary>
        /// <param name="product">The product that got purchased successfully.</param>
        /// <param name="purchaseValidation">Additional information on the transaction</param>
        internal static void Reward(ProductReceivedInfo product, PurchaseValidation purchaseValidation)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, purchaseValidation?.isRestorationPurchase);
            AnalyticsManager.TrackPurchaseRewarded(payload);
        }

        internal static void ServerError(ProductReceivedInfo product, PurchaseValidation purchaseValidation, IAPServerError serverError)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, purchaseValidation?.isRestorationPurchase);
            AnalyticsManager.TrackPurchaseServerError(payload, serverError);
        }

        internal static void TrackIAPRevenues(ProductReceivedInfo product, bool isPurchaseRestored, bool isPurchaseValidated)
        {
            VoodooIAPAnalyticsInfo payload = FormatPurchaseEvent(product, isPurchaseRestored, true);
            payload.isPurchaseValidated = isPurchaseValidated;
            payload.environment = payload.environment.ToLower(); //Specific case because the VAN schema for "iap" event expects the environment to be a lowercase enum.
            
            AnalyticsManager.TrackPurchase(payload);
            TransactionUtility.DeletePurchaseId(product.ProductId);
        }

        private static VoodooIAPAnalyticsInfo FormatPurchaseEvent(ProductReceivedInfo product, bool? purchaseRestored, bool generatePurchaseId = false)
        {
            string purchaseId = TransactionUtility.GetPurchaseUniqueId(product.ProductId, generatePurchaseId);
            double price = product.LocalizedPriceInfo.price;
            string currency = product.LocalizedPriceInfo.isoCurrencyCode;
            string productId = product.ProductId;
            string productName = product.ProductName;
            PurchaseProductType purchaseType = product.ProductType;
            string transactionId = product.TransactionID;
            string environment = Environment == EnvironmentSettings.Server.Tech ? "Production" : "Sandbox";

            return new VoodooIAPAnalyticsInfo(purchaseId, price, currency, productId, productName, purchaseType, transactionId, purchaseRestored, environment);
        }
	}
}
