using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// Clones the <see cref="UnityEngine.Purchasing.ProductType"/> type to break the dependency with this Unity package.
    /// </summary>
    public enum VoodooSettingsProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    /// <summary>
    /// This class is used to represent a In-App product in the <see cref="VoodooSettings"/>.
    /// </summary>
    [Serializable]
    public class ProductDescriptor
    {
        public ProductDescriptor(string productId, VoodooSettingsProductType type)
        {
            _productId = productId;
            _type = type;
        }

        [Tooltip("The product identifier, as entered in stores. Usually of the form com.companyname.iap.productname")]
        [SerializeField]
        private string _productId;

        public string ProductId
        {
            get
            {
#if UNITY_ANDROID
                return GetAndroidProductId();
#else
                return _productId;
#endif
            }
        }

        public string OriginalProductId
        {
            get { return _productId; }
        }

        [SerializeField] private VoodooSettingsProductType _type;
        public VoodooSettingsProductType Type => _type;

        public bool IsValidAndroidProductId()
        {
            return IsValidAndroidProductId(_productId);
        }

        internal static bool IsValidAndroidProductId(string productId)
        {
            Regex rx = new Regex(@"^[a-z0-9_.]*$",
                RegexOptions.Compiled);
            return rx.IsMatch(productId);
        }

        // Android IAP Product ID: Must start with a number or lowercase letter, and can contain numbers, lowercase letters, underscores, and periods 
        public String GetAndroidProductId() => ParseAndroidProductId(_productId);

        public static String ParseAndroidProductId(String productIdString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < productIdString.Length; i++)
            {
                char c = productIdString[i];
                if (Char.IsLetterOrDigit(c))
                {
                    c = char.ToLower(c);
                }
                else
                {
                    if (i == 0 || (c != '_' && c != '.'))
                    {
                        c = '0';
                    }
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}