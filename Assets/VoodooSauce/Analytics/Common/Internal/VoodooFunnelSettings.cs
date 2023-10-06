using System;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Serializable]
    public class VoodooFunnelsSettings
    {
        public VoodooFunnelSettings[] funnels;
    }
    
    [Serializable]
    public class VoodooFunnelSettings
    {
        public string name;
        public VoodooFunnelStepSettings[] steps;
    }

    [Serializable]
    public class VoodooFunnelStepSettings
    {
        public string stepName;
        public int stepPosition; 
    }
}