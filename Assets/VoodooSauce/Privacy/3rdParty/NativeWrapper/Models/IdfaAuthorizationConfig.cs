using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Privacy
{
    public static class IdfaAuthorizationConfig
    {
        private const string TAG = "IdfaAuthorizationConfig";
        
        private static IdfaAuthorizationPopupLocalisation _popup;
        public static IdfaAuthorizationPopupLocalisation Popup
        {
            get
            {
                if (_popup == null)
                {
                    _popup = Get<IdfaAuthorizationPopupLocalisation>(Application.systemLanguage);
                }
                return _popup;
            }
        }
        
        public static T Get<T>(SystemLanguage language, List<T> popupLocalisations = null) where T : IdfaAuthorizationAbstractPopupLocalisation, new()
        {
            VoodooLog.LogDebug(Module.VOODOO_TUNE, TAG,"Get " + typeof(T).ToString() + " for " + language.ToString());
            if (popupLocalisations == null)
            {
                popupLocalisations = VoodooSauce.GetItemsOrDefaults<T>();
            }
            
            T popup = null;
            foreach (var popupLocalisation in popupLocalisations)
            {
                if (popupLocalisation.language == language.ToString())
                {
                    popup = popupLocalisation;
                    break;
                }
            }
            if (popup == null && language != SystemLanguage.English)
            {
                popup = Get(SystemLanguage.English, popupLocalisations);
            }
            return popup;
        }

        public static List<SystemLanguage> GetLanguages<T>() where T : IdfaAuthorizationAbstractPopupLocalisation, new()
        {
            var languages = new List<SystemLanguage>();
            List<T> popupLocalisations = VoodooSauce.GetItemsOrDefaults<T>();
            foreach (var popupLocalisation in popupLocalisations)
            {
                if (Enum.IsDefined(typeof(SystemLanguage), popupLocalisation.language)) {
                    languages.Add((SystemLanguage)Enum.Parse(typeof(SystemLanguage), popupLocalisation.language));
                }
            }

            return languages;
        }
    }
    
}