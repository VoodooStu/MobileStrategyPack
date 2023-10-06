
using UnityEngine;
using Voodoo.Sauce.Common.Utils.Data.ScreenUtilsConfig;

namespace Voodoo.Sauce.Internal.Utils
{
    public static class ScreenSizeUtils
    {
        private const int CUTOUT_HEIGHT_ANDROID = 80;
        private static Vector2 _nativeScreenResolution;
        private static VoodooScreenUtilsConfig _config = null;

        
        private static Vector2 GetUnityResolutions() => new Vector2Int(Screen.width, Screen.height);

        private static Vector2 GetNativeResolutions()
        {
            if (_nativeScreenResolution.x != 0 && _nativeScreenResolution.y != 0)
                return _nativeScreenResolution;
            if (_config == null)
                _config = VoodooScreenUtilsConfig.Load();
            
            int w = Display.main.systemWidth;
            int h = Display.main.systemHeight;
            if (!_config.IsAndroidRenderOutsideSafeArea) {
                if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight) {
                    w -= CUTOUT_HEIGHT_ANDROID;
                } else {
                    h -= CUTOUT_HEIGHT_ANDROID;
                }
            }
            _nativeScreenResolution = new Vector2Int(w, h);
            return _nativeScreenResolution;
        }

        public static Vector2 GetResolutionNativeToUnityRatio()
        {
            Vector2 unityResolution = GetUnityResolutions();
            Vector2 nativeResolution = GetNativeResolutions();
            if (nativeResolution.x == 0 || nativeResolution.y == 0 || unityResolution.y == 0 || unityResolution.x == 0) 
                return new Vector2(1, 1);
            
            return new Vector2(unityResolution.x/nativeResolution.x , unityResolution.y/nativeResolution.y);
        }

        public static Vector2 GetResolutionUnityToNativeRatio()
        {
            Vector2 unityResolution = GetUnityResolutions();
            Vector2 nativeResolution = GetNativeResolutions();
            if (nativeResolution.x == 0 || nativeResolution.y == 0 || unityResolution.y == 0 || unityResolution.x == 0) 
                return new Vector2(1, 1);
            
            return new Vector2(nativeResolution.x / unityResolution.x , nativeResolution.y / unityResolution.y);
        }
#if UNITY_IOS
        private static float GetIosDpi() => IosDeviceDpiMapping.GetIosDpiFromIosMachineName(DeviceUtils.GetIosMachineName());
#endif
        public static float GetDpi()
        {
#if UNITY_IOS
            return GetIosDpi();
#endif
            return Screen.dpi;
        }
    }
}