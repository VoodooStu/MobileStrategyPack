using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Core
{
	//TODO : find a way to manage NewtonSoft usage (Assembly ?)
	public static class UserPurchasedProducts
	{
		private const string JsonKey = "UserPurchasedProductsKey";

		public static readonly List<ProductReceivedInfo> All;

		public static List<ProductReceivedInfo> Consumables => Get(PurchaseProductType.Consumable);
		public static List<ProductReceivedInfo> NonConsumables => Get(PurchaseProductType.NonConsumable);
		public static List<ProductReceivedInfo> Subscriptions => Get(PurchaseProductType.Subscription);
		
		private static List<ProductReceivedInfo> Get(PurchaseProductType type) => All.FindAll(product => product.ProductType == type);
		
		static UserPurchasedProducts()
		{
			string json = PlayerPrefs.GetString(JsonKey);
			All = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductReceivedInfo>>(json) ?? new List<ProductReceivedInfo>();
		}

		public static void Add(ProductReceivedInfo purchasedProduct)
		{
			if (All.Contains(purchasedProduct))
			{
				return;
			}

			All.Add(purchasedProduct);
			Save();
		}

		private static void Save()
		{
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(All);
			PlayerPrefs.SetString(JsonKey, json);
			PlayerPrefs.Save();
		}

		public static void ReSendPurchases(PurchaseProductType purchaseType, PurchaseDelegateList purchaseDelegateList)
		{
			List<ProductReceivedInfo> purchasedProducts = Get(purchaseType);
			foreach (var purchasedProduct in purchasedProducts)
			{
				PurchaseValidation purchaseValidation = new PurchaseValidation(ValidationStatus.Accepted, purchasedProduct.TransactionID, true);
				bool sendRewardedEvent = PlatformUtils.UNITY_IOS;
				
				purchaseDelegateList.OnPurchaseComplete(purchasedProduct, purchaseValidation, sendRewardedEvent);
			}
		}
		
		public static void ReSendPurchases(PurchaseProductType purchaseType, IPurchaseDelegateWithInfo purchaseDelegateWithInfo)
		{
			List<ProductReceivedInfo> purchasedProducts = Get(purchaseType);
			foreach (var purchasedProduct in purchasedProducts)
			{
				PurchaseValidation purchaseValidation = new PurchaseValidation(ValidationStatus.Accepted, purchasedProduct.TransactionID, true);
				purchaseDelegateWithInfo.OnPurchaseComplete(purchasedProduct, purchaseValidation);
			}
		}
	}
}