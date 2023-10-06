using System;


namespace Voodoo.Sauce.Internal.CrossPromo.Configuration
{
    /// <summary>
    /// Class used to remotely configure cross promotion from VoodooTune
    /// Technical name: Voodoo.Sauce.Internal.CrossPromo.Configuration.BackupAdsConfig
    /// </summary>
    [Serializable]
    public class BackupAdsConfig
    {
        /// <summary>
        /// If true, the backup ads will be enabled
        /// Default to false
        /// </summary>
        public bool enabled;

        /// <summary>
        /// If true, the backup ads will show whenever the user has disabled its internet connection
        /// Default to false
        /// </summary>
        public bool enabledIfNoConnection;

        /// <summary>
        /// If true, ads will be shown randomly amongst the waterfall
        /// Default to false
        /// </summary>
        public bool randomizeWaterfall;
    }
    
    /// <summary>
    /// Old class, in case it was already used
    /// Technical name: Voodoo.Sauce.Internal.CrossPromo.Configuration.BackupInterstitialConfig
    /// </summary>
    [Serializable]
    public class BackupInterstitialConfig : BackupAdsConfig { }
}
