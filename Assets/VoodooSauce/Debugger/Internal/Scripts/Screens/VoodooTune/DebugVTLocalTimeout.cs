using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.VoodooTune
{
    public static class DebugVTLocalTimeout
    {
        private const string LOCAL_TIMEOUT_KEY = "localTimeout";

        public static int LocalTimeout() => PlayerPrefs.GetInt(LOCAL_TIMEOUT_KEY, -1);

        public static void SetLocalTimeout(int value)
        {
            PlayerPrefs.SetInt(LOCAL_TIMEOUT_KEY, value);
        }
    }
}