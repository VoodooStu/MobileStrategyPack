// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.IAP
{
    internal class IAPDebugProduct
    {
        internal enum Status
        {
            Available,
            Missing,
            BadID,
            Owned
        }

        internal readonly string id;
        internal readonly string type;
        internal readonly string price;
        internal readonly Status status;
        internal readonly bool hasPurchaseButton;

        internal IAPDebugProduct(string id, string type, string price, Status status, bool hasPurchaseButton)
        {
            this.id = id;
            this.type = type;
            this.price = price;
            this.status = status;
            this.hasPurchaseButton = hasPurchaseButton;
        }
    }
}