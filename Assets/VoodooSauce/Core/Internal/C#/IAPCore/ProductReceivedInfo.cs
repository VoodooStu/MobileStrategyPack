using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// This class represents information received from the store (GooglePlay or AppStore) for a In-App product.
    /// </summary>
    public class ProductReceivedInfo
    {
        public readonly string ConnectivityType;
        public readonly string DeviceLocal;
        public readonly LocalizedPriceInfo LocalizedPriceInfo;
        public readonly string ProductId;
        public readonly string ProductName;
        public readonly PurchaseProductType ProductType;
        public readonly string Token;
        public readonly string TransactionID;
        public readonly bool IsAvailable;

        public ProductReceivedInfo(string productId, PurchaseProductType productType, string transactionID,
            string isoCurrencyCode, double localizedPrice, string token, string productName, bool isAvailable)
        {
            ProductId = productId;
            ProductType = productType;
            TransactionID = transactionID;
            LocalizedPriceInfo = new LocalizedPriceInfo(localizedPrice, isoCurrencyCode);
            ConnectivityType = DeviceUtils.GetConnectivity();
            Token = token;
            ProductName = productName;
            DeviceLocal = DeviceUtils.GetLocale();
            IsAvailable = isAvailable;
        }
    }

    public class LocalizedPriceInfo
    {
        public readonly double price;

        /// <summary>
        /// Iso currency code of this <see cref="LocalizedPriceInfo"/>.
        /// For example, it can be "USD".
        /// </summary>
        public readonly string isoCurrencyCode;

        public LocalizedPriceInfo(double price, string isoCurrencyCode)
        {
            this.price = price;
            this.isoCurrencyCode = isoCurrencyCode;
        }

        public override string ToString()
        {
            return price + " " + isoCurrencyCode;
        }
    }
}