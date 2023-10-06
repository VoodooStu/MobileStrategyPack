using System;
using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;
using Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen;

namespace Voodoo.Sauce.Internal.Editor
{
    public class VoodooSauceSettingsIntegrationCheck : IIntegrationCheck
    {
        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();

            if (KitchenAPI.CheckForMissingFiles(out string missingFiles)) {
                string message = $"File(s) URL missing in kitchen settings (for store: {settings.Store} & platform: {KitchenStoreJSON.GetCurrentPlatform()}): {missingFiles}."
                    + Environment.NewLine + $"Please note that the build might not work as expected.";
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, message));
            }

            return list;
        }
    }
}