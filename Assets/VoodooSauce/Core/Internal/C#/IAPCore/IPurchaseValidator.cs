using System.Threading.Tasks;

namespace Voodoo.Sauce.Core
{
    public interface IPurchaseValidator
    {
        /// <summary>Sends a request to the backend to verify a purchase.</summary>
        /// <param name="purchaseInfo">The purchase data</param>
        /// <param name="settings">The settings of the Voodoo Sauce plugin</param>
        /// <returns>A <see cref="PurchaseValidation"/> object.</returns>
        Task<PurchaseValidation> Validate(ProductReceivedInfo purchaseInfo, VoodooSettings settings);
    }
}