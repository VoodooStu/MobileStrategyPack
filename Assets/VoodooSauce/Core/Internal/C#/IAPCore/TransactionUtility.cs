using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Voodoo.Sauce.Core
{
	public static class TransactionUtility
	{
		private const string PURCHASE_ID_PREFIX_KEY = "PurchaseId_";

		public static string GetProductKey(string productId) => PURCHASE_ID_PREFIX_KEY + productId;

		/// <summary>
		/// Returns a unique purchase id for the given product id.
		/// It will be saved in PlayerPrefs until the <see cref="DeletePurchaseId"/> method is called.
		/// </summary>
		/// <param name="productId">The Product Id</param>
		/// <param name="generate">Force to generate a new purchase id</param>
		public static string GetPurchaseUniqueId(string productId, bool generate)
		{
			string productKey = GetProductKey(productId);
			if (generate) return GeneratePurchaseId(productKey);
			Stack<string> purchaseIds =  GetPurchaseIds(productKey);
			return purchaseIds.Count > 0 ? purchaseIds.Peek() : "";
		}
		
		/// <summary>
		/// Create a unique purchase id and save it into the PlayerPrefs.
		/// </summary>
		/// <param name="productKey">The key to store the purchase unique id</param>
		private static string GeneratePurchaseId(string productKey)
		{
			string purchaseId = Guid.NewGuid().ToString();
			Stack<string> purchaseIds = GetPurchaseIds(productKey);
			purchaseIds.Push(purchaseId);
			SavePurchaseIds(purchaseIds, productKey);
			return purchaseId;
		}

		/// <summary>
		/// Delete the last purchase id saved for the product id present in the PlayerPrefs.
		/// </summary>
		/// <param name="productId">The Product Id</param>
		/// <returns>The last purchase id saved for the product id that was setup before calling the method.</returns>
		public static string DeletePurchaseId(string productId)
		{
			string purchaseId = "";
			string productKey = GetProductKey(productId);
			Stack<string> purchaseIds =  GetPurchaseIds(productKey);
			if (purchaseIds.Count > 0) {
				purchaseId = purchaseIds.Pop();
				SavePurchaseIds(purchaseIds, productKey);
			}
			return purchaseId;
		}
		
		private static void SavePurchaseIds(Stack<string> purchaseIds, string productKey)
		{
			if (purchaseIds.Count == 0) {
				PlayerPrefs.DeleteKey(productKey);
				return;
			}
			List<string> purchaseIdsList = purchaseIds.ToList();
			purchaseIdsList.Reverse();
			//Serializing Stacks with Newtonsoft is not working well so we prefer to serialize lists
			string json = JsonConvert.SerializeObject(purchaseIdsList);
			PlayerPrefs.SetString(productKey, json);
		}

		private static Stack<string> GetPurchaseIds(string productKey)
		{
			string json = PlayerPrefs.GetString(productKey, "");
			if (string.IsNullOrEmpty(json)) return new Stack<string>();
			return new Stack<string>(JsonConvert.DeserializeObject<List<string>>(json));
		}
	}
}