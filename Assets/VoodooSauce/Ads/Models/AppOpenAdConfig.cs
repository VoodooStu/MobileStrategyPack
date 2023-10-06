using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [Serializable]
    public class AppOpenAdConfig
    {
        public float minimumBackgroundTime = 60;
        public float aoToAoCooldown = 60;
        public float fsToAoCooldown = 0;
        public float rvToAoCooldown = 5;
        public float aoToFsCooldown = 0;
        
        public override string ToString() => "- delay in background before AO: " + minimumBackgroundTime +
            "\n- delay between two AOs: " + aoToAoCooldown +
            "\n- delay between FS and AO: " + fsToAoCooldown +
            "\n- delay between RV and AO: " + rvToAoCooldown +
            "\n- delay between AO and FS: " + aoToFsCooldown;
    }
}