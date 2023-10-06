using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    /// <summary>
    /// This class is located outside of the Odeeo folder because it's also used by <see cref="Voodoo.Sauce.Core.VoodooSettings"/>.
    /// Even if Odeeo network is removed from the build, the Voodoo settings still have the parameters.
    /// </summary>
    [Serializable]
    public class OdeeoConfig
    {
        public string appKey;
        public string buttonType;
    }
}