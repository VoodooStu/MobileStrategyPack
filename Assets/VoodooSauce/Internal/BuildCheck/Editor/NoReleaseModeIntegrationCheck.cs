using System.Collections.Generic;
using UnityEditor;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Editor
{
    public class NoReleaseModeIntegrationCheck : IIntegrationCheck
    {
        private const string WARNING_MESSAGE =
            "Development mode is activated, you can ignore this message if this is expected.";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var result = new List<IntegrationCheckMessage>();

            if (EditorUserBuildSettings.development)
            {
                result.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, WARNING_MESSAGE));
            }

            return result;
        }
    }
}