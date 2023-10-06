using System;
using UnityEngine;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.DeviceInfo
{
    public static class AndroidNativeVolumeService
    {
        private const string TAG = "AndroidNativeVolumeService";
        
        private static int _streamMusic;
        private static int? _maxVolume;
        private static AndroidJavaObject _audioManager;
        private static bool _audioManagerHasError;
        private static bool _maxVolumeHasError;
 
        private static AndroidJavaObject DeviceAudio
        {
            get
            {
                if (_audioManager != null || _audioManagerHasError) {
                    return _audioManager;
                }

                try {
                    AndroidJavaObject currentActivity = AndroidUtils.GetCurrentActivity();
                    var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                    var audioManagerClass = new AndroidJavaClass("android.media.AudioManager");
                    var contextClass = new AndroidJavaClass("android.content.Context");
 
                    _streamMusic = audioManagerClass.GetStatic<int>("STREAM_MUSIC");
                    var contextAudioService = contextClass.GetStatic<string>("AUDIO_SERVICE");
 
                    _audioManager = context.Call<AndroidJavaObject>("getSystemService", contextAudioService);
                    if (_audioManager == null) {
                        throw new Exception($"Android native function 'getSystemService' returns null");
                    }
                } catch (Exception e) {
                    VoodooLog.LogError(Module.COMMON, TAG, $"Could not read Audio Manager: {e.Message}");
                    VoodooSauce.LogException(e);
                    _audioManagerHasError = true;
                    return null;

                }
                
                return _audioManager;
            }
        }
        
        public static int GetSystemVolume()
        {
            // We already tried to call the function 'getStreamMaxVolume' but it returned an error.
            // As calling a native function is expensive, we don't call it everytime, so an error value is returned.
            if (_maxVolumeHasError) {
                return -1;
            }

            if (DeviceAudio == null) {
                return -1;
            }
            
            // Here is the first time this function is called.
            if (_maxVolume == null) {
                _maxVolume = DeviceAudio.Call<int>("getStreamMaxVolume", _streamMusic);
            }

            // We can't get the max volume, so the error is saved in order to not retry the next time. 
            if (_maxVolume == null) {
                _maxVolumeHasError = true;
                return -1;
            }
            
            var deviceVolume = DeviceAudio.Call<int>("getStreamVolume", _streamMusic);
            return (int)(deviceVolume / (float)_maxVolume * 100);
        }

        public static int RetryGetSystemVolume()
        {
            if (_maxVolumeHasError) {
                _maxVolumeHasError = false;
                _maxVolume = null;
            }

            if (_audioManagerHasError) {
                _audioManagerHasError = false;
                _audioManager = null;
            }

            return GetSystemVolume();
        } 
    }
}