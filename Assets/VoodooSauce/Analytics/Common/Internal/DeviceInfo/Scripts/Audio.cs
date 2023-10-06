using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.DeviceInfo
{
    internal static class Audio
    {
        /// <summary>
        /// Returns the volume of the device.
        /// </summary>
        /// <returns>An integer between 0 (mute) and 100</returns>
        internal static int GetVolume()
        {
            int volume = _getVolume();

            if (volume < 0) {
                volume = 0;
            }

            if (volume > 100) {
                volume = 100;
            }

            return volume;
        }

#region iOS Native Methods

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int _getVolume();
#endif

#endregion

#region Android Native Methods

#if UNITY_ANDROID && !UNITY_EDITOR
        private static int _getVolume() => AndroidNativeVolumeService.GetSystemVolume();
#endif

#endregion

#region Other platform methods

#if UNITY_EDITOR
        private static int _getVolume() => 50;
#endif

#endregion
    }
}