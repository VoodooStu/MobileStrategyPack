// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public class AdjustParameters
    {
        public string AdjustIosAppToken;
        public string AdjustAndroidAppToken;
        public string NoAdsBundleId;

        public AdjustParameters(string adjustIosAppToken, string adjustAndroidAppToken, string noAdsBundleId)
        {
            AdjustIosAppToken = adjustIosAppToken;
            AdjustAndroidAppToken = adjustAndroidAppToken;
            NoAdsBundleId = noAdsBundleId;
        }
    }
}