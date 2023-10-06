using System;
using Voodoo.Sauce.Internal.VoodooTune;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.RemoteConfig
{
    [Serializable]
    public class RemoteConfigConfiguration
    {
        // Please don't use directly this value as it doesn't take into account the VS Debugger value (VoodooTuneTimeoutLocal.LocalTimeout())
        public int initTimeoutInMillisecondsWithCache = 3000;

        public int InitTimeoutInMillisecondsWithCache => DebugVTLocalTimeout.LocalTimeout() <= -1 ?
            initTimeoutInMillisecondsWithCache : DebugVTLocalTimeout.LocalTimeout();
    }
}