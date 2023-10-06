namespace Voodoo.Sauce.Core
{
	public class PurchaseValidation
	{
		public ValidationStatus status;
		public string originalTransactionId;
		public bool? isRestorationPurchase;
		public int? storeStatusCode;

		public PurchaseValidation(ValidationStatus status, string originalTransactionId, bool? isRestorationPurchase = null, int? storeStatusCode = null)
		{
			this.status = status;
			this.originalTransactionId = originalTransactionId;
			this.isRestorationPurchase = isRestorationPurchase;
			this.storeStatusCode = storeStatusCode;
		}
	}

	/// <summary>
	/// Define whether or not the transaction is valid
	/// </summary>
	public enum ValidationStatus
	{
		Accepted,
		Denied,
		InProgress
	}
}