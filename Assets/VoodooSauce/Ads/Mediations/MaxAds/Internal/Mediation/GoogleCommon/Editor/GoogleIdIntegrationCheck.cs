using System.Collections.Generic;
using UnityEditor;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Ads.MaxMediation
{
    public abstract class GoogleIdIntegrationCheck : IIntegrationCheck
    {
        private const string SETTINGS_NO_AD_MOB_IOS_APP_ID_ERROR = "VoodooSauce Settings is missing AdMob iOS application identifier";
        private const string SETTINGS_NO_AD_MOB_ANDROID_APP_ID_ERROR = "VoodooSauce Settings is missing AdMob Android application identifier";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();
            
            // [VS-3649] The AdMob identifier must be provided
            if (string.IsNullOrEmpty(settings.AdMobIosAppId) && PlatformUtils.UNITY_IOS) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_NO_AD_MOB_IOS_APP_ID_ERROR));
            }
            
            if (string.IsNullOrEmpty(settings.AdMobAndroidAppId) && PlatformUtils.UNITY_ANDROID) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SETTINGS_NO_AD_MOB_ANDROID_APP_ID_ERROR));
            }

            return list;
        }
    }
}