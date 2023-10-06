using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Voodoo.Sauce.Internal;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.LoadingTime
{
    internal static class ElapsedTime
    {
        /// <summary>
        /// Returns the timestamp of the application launch start date.
        /// </summary>
        /// <returns>Timestamp in milliseconds of the application launch start date</returns>
        internal static long GetStartTimestamp() => _getStartTimestamp();

#region iOS Native Methods

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern long _getStartTimestamp();
#endif

#endregion

#region Android Native Methods

#if UNITY_ANDROID && !UNITY_EDITOR
        private static long _getStartTimestamp() {
            try {
                var androidClass = new AndroidJavaClass("io.voodoo.tools.LoadingTimeTracker");
                return androidClass.CallStatic<long>("getStartTimestamp");
            } catch (Exception e) {
                VoodooSauce.LogException(e);
            }

            return -1;
        }
#endif

#endregion

#region Other platform methods

#if UNITY_EDITOR
        // Define the start time of the application loading.
        private static DateTimeOffset? _startTime;

        private static long _getStartTimestamp()
        {
            if (_startTime == null) {
                _startTime = new DateTimeOffset(DateTime.UtcNow).Subtract(TimeSpan.FromMilliseconds(Time.realtimeSinceStartup * 1000));
            }

            return ((DateTimeOffset) _startTime).ToUnixTimeMilliseconds();
        }
#endif

#endregion
    }
}