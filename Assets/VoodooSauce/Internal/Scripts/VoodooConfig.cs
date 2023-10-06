using UnityEngine;

namespace Voodoo.Sauce.Internal
{
    /// <summary>
    /// Collection of static final variables used throughout the codebase
    /// </summary>
    public static class VoodooConfig
    {
        // VoodooSauce Version
        public static string Version() => VoodooVersion.Load().ToString();
        public static string GetPlatformString() => (Application.platform == RuntimePlatform.Android ? "android" : "ios");
    }
}
