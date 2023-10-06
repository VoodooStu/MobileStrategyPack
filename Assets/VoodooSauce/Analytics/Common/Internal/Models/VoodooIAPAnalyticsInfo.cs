namespace Voodoo.Sauce.Internal.Analytics
{
	public class VoodooIAPAnalyticsInfo
	{
		internal double price; //Formerly known as LocalizedPrice
		internal string currency; //Formerly known as IsoCurrencyCode 
		internal string productId; //Formerly known as ProductId
		internal string productName;
		internal PurchaseProductType purchaseType;
		internal string purchaseTransactionId;
		internal bool?  purchaseRestored;
		internal string failureReason;
		internal string environment;
		internal string id;
		internal bool? isPurchaseValidated;

		/// <summary>
		/// The base constructor for all Analytics regarding IAP
		/// </summary>
		/// <param name="id">The unique id of the transaction (Use TransactionUtility.GetPurchaseUniqueId(product.definition.id)).</param>
		/// <param name="price">The localized price of the product.</param>
		/// <param name="currency">The currency used for the transaction</param>
		/// <param name="productID">The product id</param>
		/// <param name="productName">The product name</param>
		/// <param name="purchaseType">The purchase type between Consumable, NonConsumable and Subscription.</param>
		/// <param name="purchaseTransactionID">The id of the transaction (parsed from the store's receipt).</param>
		/// <param name="purchaseRestored">Is the purchase a restoration or a new purchase.</param>
		/// <param name="environment">The environment on which the purchase was done (Production/Sandbox).</param>
		public VoodooIAPAnalyticsInfo(string id, double price, string currency, string productID, string productName, PurchaseProductType purchaseType,
			string purchaseTransactionID, bool? purchaseRestored, string environment = "Production")
		{
			this.id = id;
			this.price = price;
			this.currency = currency;
			productId = productID;
			this.productName = productName;
			this.purchaseType = purchaseType;
			purchaseTransactionId = purchaseTransactionID;
			this.purchaseRestored = purchaseRestored;
			this.environment = environment;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">The unique id of the transaction (Use TransactionUtility.GetPurchaseUniqueId(product.definition.id)).</param>
		public VoodooIAPAnalyticsInfo(string id)
		{
			this.id = id;
		}
	}

	public enum IAPServerError
	{
		NoServerResponse
	}
}