// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
using UnityEngine;

namespace Voodoo.Sauce.Common.Utils
{
    public static class PlatformUtils
    {
        public static bool UNITY_IOS {
            get {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool UNITY_ANDROID {
            get {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }
        
        public static bool UNITY_EDITOR {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static readonly bool IS_LINUX = Application.platform == RuntimePlatform.LinuxEditor;

        public static readonly bool IS_OSX = Application.platform == RuntimePlatform.OSXEditor;
    }
}