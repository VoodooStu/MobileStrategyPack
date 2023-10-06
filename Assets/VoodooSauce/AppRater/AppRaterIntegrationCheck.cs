#if UNITY_IOS
using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.AppRater
{
    public class AppRaterIntegrationCheck : IIntegrationCheck
    {
        private const string SettingsNoAppleStoreIDError = "VoodooSauce Settings is missing Apple Store Id";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();
            if (string.IsNullOrEmpty(settings.AppleStoreId)) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, SettingsNoAppleStoreIDError, null, true));
            }

            return list;
        }
    }
}
#endif