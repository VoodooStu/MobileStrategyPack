using UnityEngine;
using Voodoo.Sauce.Common.Utils;

namespace Voodoo.Sauce.Internal.Utils
{
    public static class DeviceUtils
    {
        private const string TAG = "DeviceUtils";
        private static string _iosModelMachineName;
        public static string OperatingSystemVersion => _operatingSystemVersion ?? (_operatingSystemVersion = GetOperatingSystemVersion());

        private static string _operatingSystemVersion;

        public static string Manufacturer => _manufacturer ?? (_manufacturer = GetManufacturer());

        private static string _manufacturer;

        public static string Model => _model ?? (_model = GetModel());

        private static string _model;

        /// <summary>
        /// Indicates if the device has a bottom safe area space
        /// </summary>
        /// <returns>Returns true if the device has a bottom safe area space and false otherwise</returns>
        public static bool HasBottomSafeArea() => Screen.height > (Screen.safeArea.y + Screen.safeArea.height);
        
        public static bool HasNotch() => Screen.safeArea.width < Screen.width || Screen.safeArea.height < Screen.height;

        public static string GetResolution()
        {
            int height = Screen.height > Screen.width ? Screen.height : Screen.width;
            int width = Screen.height > Screen.width ? Screen.width : Screen.height;
            return height + "x" + width;
        }

        private const string BuildClassName = "android.os.Build";
        private static readonly string _versionClassName = $"{BuildClassName}$VERSION";

        private static T CallDeviceInformationMethod<T>(string className, string method)
        {
            using (AndroidJavaClass jo = new AndroidJavaClass(className)) {
                return jo.GetStatic<T>(method);
            }
        }

        public static string GetManufacturer()
        {
            if (PlatformUtils.UNITY_EDITOR) {
                return "Unknown(Editor)";
            }

            if (PlatformUtils.UNITY_IOS) {
                return "Apple";
            }

            if (PlatformUtils.UNITY_ANDROID) {
                return CallDeviceInformationMethod<string>(BuildClassName, "MANUFACTURER");
            }

            return "Unknown()";
        }

        public static string GetModel()
        {
            if (PlatformUtils.UNITY_IOS && !PlatformUtils.UNITY_EDITOR) {
                return UnityIosDevice.Generation;
            }

            if (PlatformUtils.UNITY_ANDROID && !PlatformUtils.UNITY_EDITOR) {
                return CallDeviceInformationMethod<string>(BuildClassName, "MODEL");
            }

            return SystemInfo.deviceModel;
        }

        public static string GetOperatingSystemVersion()
        {
            if (PlatformUtils.UNITY_IOS && !PlatformUtils.UNITY_EDITOR) {
                return UnityIosDevice.SystemVersion;
            }

            if (PlatformUtils.UNITY_ANDROID && !PlatformUtils.UNITY_EDITOR) {
                return $"Android API {CallDeviceInformationMethod<int>(_versionClassName, "SDK_INT")}";
            }

            return SystemInfo.operatingSystem;
        }

        private const string ConnectivityOffline = "Offline";
        private const string ConnectivityNetwork = "Network";
        private const string ConnectivityWifi = "Wifi";
        private const string ConnectivityUnknown = "Unknown";

        private static string _lastConnectivity = ConnectivityUnknown;

        internal static string GetConnectivity()
        {
            if (UnityThreadExecutor.IsMainThread) {
                switch (Application.internetReachability) {
                    case NetworkReachability.NotReachable:
                        _lastConnectivity = ConnectivityOffline;
                        break;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        _lastConnectivity = ConnectivityNetwork;
                        break;
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        _lastConnectivity = ConnectivityWifi;
                        break;
                    default:
                        _lastConnectivity = ConnectivityUnknown;
                        break;
                }

                return _lastConnectivity;
            }

            // If this method is not called in the main thread, it returns the last cached value of the connectivity.
            if (_lastConnectivity == ConnectivityUnknown) {
                VoodooLog.LogError(Module.COMMON, TAG, "Impossible to get the device connectivity right now.");
            }

            return _lastConnectivity;
        }

#if UNITY_IOS && !UNITY_EDITOR
		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern string _getNativeLocale();
#else
        private static string _getNativeLocale() => "";
#endif

        internal static string GetAndroidInstallStore()
        {
            if (!PlatformUtils.UNITY_ANDROID || PlatformUtils.UNITY_EDITOR) {
                return null;
            }

            if (_installStore == null) {
                using (var installStoreClass = new AndroidJavaClass("io.voodoo.installstore.InstallStore")) {
                    _installStore = installStoreClass.CallStatic<string>("Get", "not_found", "no_source");
                }
            }

            return _installStore;
        }

        private static string _installStore;

        internal static string GetLocale()
        {
            if (PlatformUtils.UNITY_IOS && !PlatformUtils.UNITY_EDITOR) {
                return _getNativeLocale();
            }

            if (PlatformUtils.UNITY_ANDROID && !PlatformUtils.UNITY_EDITOR) {
                using (var locale = new AndroidJavaClass("java.util.Locale")) {
                    using (var defaultLocale = locale.CallStatic<AndroidJavaObject>("getDefault")) {
                        var country = defaultLocale.Call<string>("getCountry");
                        return country;
                    }
                }
            }

            return "us";
        }

#if UNITY_IOS
        internal static string GetIosMachineName()
        {
            if (_iosModelMachineName == null) {
                _iosModelMachineName = SystemInfo.deviceModel;
            }

            return _iosModelMachineName;
        }
#endif
    }
}