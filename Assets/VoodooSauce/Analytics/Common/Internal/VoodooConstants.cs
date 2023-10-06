// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal
{
    internal static class VoodooConstants
    {
#if UNITY_ANDROID
        internal const string TEST_APP_BUNDLE = "io.voodoo.saucetestapp";
#else
        internal const string TEST_APP_BUNDLE = "io.voodoo.voodoosaucetestapp";
#endif
    }
}