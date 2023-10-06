using System;
using System.Collections.Generic;
using Voodoo.Sauce.Core;

namespace Voodoo.Sauce.Internal.Analytics
{
    public static class VoodooFunnelsManager
    {
        private const string TAG = "VOODOO_FUNNELS";

        private const string NONEXISTENT_FUNNEL = "The Funnel you are attempting to track was not set in your VoodooSettings"; 
        private const string NONEXISTENT_FUNNEL_STEP = "The Funnel Step you are attempting to track was not set in your VoodooSettings";

        private static readonly Dictionary<string, VoodooFunnel> FunnelsDict = new Dictionary<string, VoodooFunnel>();

        private static bool _isInitialized; 
        private static void Initialize()
        {
            VoodooFunnelsSettings voodooFunnelsSettings = VoodooSettings.Load().VoodooFunnels;

            if (voodooFunnelsSettings == null) return; 
            
            foreach (VoodooFunnelSettings funnelSettings in voodooFunnelsSettings.funnels) {
                FunnelsDict.Add(funnelSettings.name, new VoodooFunnel(funnelSettings));
            }

            _isInitialized = true; 
        }

        internal static void Reset()
        {
            _isInitialized = false;
            FunnelsDict.Clear(); 
        }
        
        public static void TrackFunnel(string funnelName, string funnelStep)
        {
            if(!_isInitialized) Initialize(); 
            
            // Check funnelName and step exists 
            if (!FunnelsDict.ContainsKey(funnelName)) {
                VoodooLog.LogError(Module.ANALYTICS, TAG, NONEXISTENT_FUNNEL);
                return; 
            }
            if (!FunnelsDict[funnelName].HasStepName(funnelStep)) {
                VoodooLog.LogError(Module.ANALYTICS, TAG, NONEXISTENT_FUNNEL_STEP);
                return; 
            }

            AnalyticsManager.TrackVoodooFunnel(funnelName, funnelStep, FunnelsDict[funnelName].GetStepPosition(funnelStep)); 
        }
    }
    
    public class VoodooFunnel
    {
        private readonly Dictionary<string, int> _stepsDict = new Dictionary<string, int>();

        public bool HasStepName(string stepName) => _stepsDict.ContainsKey(stepName);

        public int GetStepPosition(string stepName) => _stepsDict[stepName]; 

        public VoodooFunnel(VoodooFunnelSettings voodooFunnelSettings)
        {
            foreach (VoodooFunnelStepSettings step in voodooFunnelSettings.steps) {
                _stepsDict.Add(step.stepName, step.stepPosition); 
            }
        }
    }
}