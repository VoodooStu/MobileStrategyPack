
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    [System.Serializable]
    public class BannerPopupLocalisation
    {
        public string language;
        public string body;
        public string purchaseButtonText;
        public string cancelButtonText;
        
        public static string Body => Localisation.body;
        public static string PurchaseButtonText => Localisation.purchaseButtonText;
        public static string CancelButtonText => Localisation.cancelButtonText;

        private static BannerPopupLocalisation _localisation;
        private static BannerPopupLocalisation Localisation
        {
            get
            {
                if (_localisation == null) _localisation = GetLocalisation();
                return _localisation;
            }
            set => _localisation = value;
        }
        
        private static BannerPopupLocalisation GetLocalisation(SystemLanguage language = SystemLanguage.Unknown)
        {
            if (language == SystemLanguage.Unknown) language = Application.systemLanguage;
            List<BannerPopupLocalisation> localisations = VoodooSauce.GetItemsOrDefaults<BannerPopupLocalisation>();
            foreach (var l in localisations)
            {
                if (l.language == language.ToString()) {
                    return l;
                }
            }
            return GetLocalisation(SystemLanguage.English);
        }
    }
}
