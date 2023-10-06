using System;
using UnityEngine.Scripting;
using Voodoo.Sauce.Privacy;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Preserve]
    internal abstract class BaseAnalyticsProviderWithLogger: IAnalyticsProvider
    {
        public bool IsInitialized { get; protected set; }
        internal abstract VoodooSauce.AnalyticsProvider GetProviderEnum();
        public abstract void Instantiate(string mediation);
        public abstract void Initialize(PrivacyCore.GdprConsent consent, bool isChinaBuild);
        internal virtual string GetUninitializedErrorMessage() => AnalyticsEventLoggerConstant.DEFAULT_NOT_INITIALIZED_MESSAGE;
    }
}