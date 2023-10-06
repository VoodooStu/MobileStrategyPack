using System;
using System.Collections.Generic;
using System.Linq;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Core
{
	public class PurchaseDelegateList
	{
		private const string TAG = "PurchaseDelegateList";

		private readonly List<IPurchaseDelegateWithInfo> _purchaseDelegates;

		public PurchaseDelegateList()
		{
			_purchaseDelegates = new List<IPurchaseDelegateWithInfo>();
		}

		public void Add(IPurchaseDelegateWithInfo item)
		{
			if (!_purchaseDelegates.Contains(item))
				_purchaseDelegates.Add(item);
		}

		public void Remove(IPurchaseDelegateWithInfo item)
		{
			if (_purchaseDelegates.Contains(item))
				_purchaseDelegates.Remove(item);
		}

		public void OnInitializeSuccess()
		{
			List<IPurchaseDelegateWithInfo> purchaseDelegatesClone = _purchaseDelegates.ToList();
			foreach (IPurchaseDelegateWithInfo del in purchaseDelegatesClone)
			{
				try
				{
					del.OnInitializeSuccess();
				}
				catch (Exception e)
				{
                    VoodooLog.LogError(Module.IAP, TAG, e.Message);
				}
			}
		}

		public void OnInitializeFailure(VoodooInitializationFailureReason reason)
		{
			List<IPurchaseDelegateWithInfo> purchaseDelegatesClone = _purchaseDelegates.ToList();
			foreach (IPurchaseDelegateWithInfo del in purchaseDelegatesClone)
			{
				try
				{
					del.OnInitializeFailure(reason);
				}
				catch (Exception e)
				{
                    VoodooLog.LogError(Module.IAP, TAG, e.Message);
				}
			}
		}

		// Disable warning obsolete on these 2 method below only, since these two method are the only one that use the 
		// deprecated IPurchaseDelegate class
#pragma warning disable 612, 618
		public void OnPurchaseComplete(ProductReceivedInfo productInfo, PurchaseValidation purchaseValidation, bool sendRewardEvent)
		{
			if (_purchaseDelegates.Count > 0 && sendRewardEvent)
			{
				IAPAnalytics.Reward(productInfo, purchaseValidation);
			}

			var purchaseDelegatesClone = _purchaseDelegates.ToList();
			foreach (IPurchaseDelegateWithInfo del in purchaseDelegatesClone)
			{
				try
				{
					del.OnPurchaseComplete(productInfo, purchaseValidation);
				}
				catch (Exception e)
				{
					VoodooLog.LogError(Module.IAP, TAG, e.Message);
				}
			}
		}

		public void OnPurchaseFailure(VoodooPurchaseFailureReason reason, ProductReceivedInfo productInfo)
		{
			List<IPurchaseDelegateWithInfo> purchaseDelegatesClone = _purchaseDelegates.ToList();
			foreach (IPurchaseDelegateWithInfo del in purchaseDelegatesClone)
			{
				try
				{
					del.OnPurchaseFailure(reason, productInfo);
				}
				catch (Exception e)
				{
                    VoodooLog.LogError(Module.IAP, TAG, e.Message);
				}
			}
		}
#pragma warning restore 612, 618
	}
}