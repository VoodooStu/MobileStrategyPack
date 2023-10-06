using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    internal static class AdjustConstants
    {
        internal static readonly int[] GameCountsToTrack = {2, 10, 100, 1000};

        internal static readonly HashSet<string> TestDeviceIdfas = new HashSet<string>() {
            "9f5f540df1711cc2b9783093a5b785a0", // Android Galaxy S7 Kyle Personal
            "04F89FFC-F0E8-4C63-9203-8E09E0F13FAD", // IOS test device
        };

        internal const string GamePlayedEventName = "_games_played";
        internal const string CustomEventName = "custom_event";

        internal const string CustomEventParameterName = "custom_event_name";
        internal const string AppSubVersionParameterName = "app_subversion";
        internal const string VoodooSauceParameterName = "vs_version";
    }
}