
using Voodoo.Sauce.Internal.Ads.Adn;

public class AdnSdk :
#if UNITY_EDITOR
    // Check for Unity Editor first since the editor also responds to the currently selected platform.
    AdnSdkUnityEditor
#elif UNITY_ANDROID
    AdnSdkAndroid
#elif UNITY_IPHONE || UNITY_IOS
    AdnSdkiOS
#else
    AdnSdkUnityEditor
#endif
{
}