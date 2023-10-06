using System.Collections.Generic;
using UnityEditor;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Editor
{
    public class SuperPremiumModeBuildCheck : IIntegrationCheck
    {
        private const string WARNING_MESSAGE_SUPER_PREMIUM_ACTIVATED =
            "Super Premium Mode is Activated, you can ignore this message if this is expected.";
        private const string ERROR_MESSAGE_SUPER_PREMIUM_ACTIVATED_WITHOUT_DEVELOPMENT_MODE_ENABLED =
            "Super Premium Mode is Activated but no development mode is used. If you wish to proceed forward please enable development mode (in Player Settings) or disable the Super Premium Mode in VoodooSettings";
        
        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var result = new List<IntegrationCheckMessage>();

            if (EditorUserBuildSettings.development && settings.EnableSuperPremiumMode) {
                result.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, WARNING_MESSAGE_SUPER_PREMIUM_ACTIVATED));
            } else if (!EditorUserBuildSettings.development && settings.EnableSuperPremiumMode) {
                result.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, ERROR_MESSAGE_SUPER_PREMIUM_ACTIVATED_WITHOUT_DEVELOPMENT_MODE_ENABLED));
            }

            return result;
        }
    }
}