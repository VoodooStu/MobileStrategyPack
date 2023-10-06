using System;
using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Analytics.Editor
{
    [Serializable]
    public class VoodooFunnelsIntegrationCheck : IIntegrationCheck
    {
        private const string DUPLICATE_FUNNEL_ERROR_MESSAGE = "Funnel name duplicated"; 

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            // funnel names unique 
            // step names in funnel unique 
            // step positions in funnel unique 

            List<IntegrationCheckMessage> errors = new List<IntegrationCheckMessage>();

            if (settings.VoodooFunnels?.funnels == null) return errors; 
            
            HashSet<string> seenFunnels = new HashSet<string>();

            foreach (VoodooFunnelSettings funnel in settings.VoodooFunnels.funnels) {
                if (seenFunnels.Contains(funnel.name)) {
                    errors.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR,
                        DUPLICATE_FUNNEL_ERROR_MESSAGE + " Funnel: " + funnel.name));
                } 
                seenFunnels.Add(funnel.name);
                errors.AddRange(funnel.IntegrationCheck()); 
            }
            return errors; 
        }
    }

    [Serializable]
    public static class VoodooFunnelIntegrationTestExtension
    {
        private const string DUPLICATE_FUNNEL_STEP_NAME_ERROR_MESSAGE = "Step name duplicated in the same funnel";
        private const string DUPLICATE_FUNNEL_STEP_POSITION_ERROR_MESSAGE = "Step position duplicated in the same funnel";

        public static List<IntegrationCheckMessage> IntegrationCheck(this VoodooFunnelSettings voodooFunnel)
        {
            // step names in funnel unique 
            // step positions in funnel unique

            var seenStepNames = new HashSet<string>();
            var seenStepPositions = new HashSet<int>();

            var errors = new List<IntegrationCheckMessage>();

            foreach (VoodooFunnelStepSettings step in voodooFunnel.steps) {
                if (seenStepNames.Contains(step.stepName))
                    errors.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR,
                        DUPLICATE_FUNNEL_STEP_NAME_ERROR_MESSAGE + " Funnel:" + voodooFunnel.name + " StepName:" + step.stepName));

                if (seenStepPositions.Contains(step.stepPosition))
                    errors.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR,
                        DUPLICATE_FUNNEL_STEP_POSITION_ERROR_MESSAGE + " Funnel:" + voodooFunnel.name + " StepName:" + step.stepName
                        + " StepPosition: " + step.stepPosition));

                seenStepNames.Add(step.stepName);
                seenStepPositions.Add(step.stepPosition);
            }

            return errors;
        }
    }
}
