using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport.Embrace
{
    public class EmbraceIntegrationCheck : IIntegrationCheck
    {
#region Constants

        private const string INACTIVE_MESSAGE = "Embrace will not be activated because the percentage is less or equal than 0.0";
        private const string MISCONFIGURATION_MESSAGE = "Embrace is misconfigured:\n- the percentage should be set to 0.0 to disable Embrace\n- or the appId is not filled\n- or the apiToken is not filled";

        private const string MAX_PERCENTAGE_MESSAGE = "Embrace will be activate only for #MAX#% of the users (maximum allowed)";

#endregion
        
        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var list = new List<IntegrationCheckMessage>();

            float maxPercentage = settings.embraceMaxPercentage;
            float percentage = settings.EmbraceUserPercentage;
            string appId = settings.EmbraceAppId;
            string apiToken = settings.EmbraceApiToken;

            if (percentage <= 0.0) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, INACTIVE_MESSAGE));
            }
            
            if (percentage > maxPercentage) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, MAX_PERCENTAGE_MESSAGE.Replace("#MAX#", $"{maxPercentage}")));
            }

            if (percentage > 0.0 && (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiToken))) {
                list.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING, MISCONFIGURATION_MESSAGE));
            }
            
            return list;
        }
    }
}