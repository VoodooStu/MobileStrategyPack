using System;
using System.Collections.Generic;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.IntegrationCheck;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport
{
    /// <summary>
    /// This is the core class of the CrashReport module.
    /// A "core" module class defines what the module's methods do when the module is not available/implemented.
    /// </summary>
    internal class CrashReportCore : IModule
    {
        public enum CrashReporter
        {
            None,
            Firebase,
            Embrace
        }

        // Firebase is used as the default crash reporter
        private const CrashReporter DEFAULT_CRASH_REPORTER = CrashReporter.Firebase;

        public class CrashReportManagerParameters
        {
            internal VoodooSettings VoodooSettings;
            internal Action<bool> AnalyticsConsentEvent;
        }

        public virtual void Initialize(CrashReportManagerParameters parameters)
        {
        }

        public virtual CrashReporter GetCrashReporter() => DEFAULT_CRASH_REPORTER;

        internal virtual void ForceCrashReporter(CrashReporter crashReporter)
        {
        }

        public virtual void LogLevelStart(string level)
        {
        }

        public virtual void LogLevelFinish(string level)
        {
        }

        public virtual void LogException(Exception exception)
        {
        }

        public virtual void SetCustomData(string key, string value)
        {
        }

        public virtual void Log(string message)
        {
        }

        public virtual void SetUserId(string userId)
        {
        }

        public virtual void SetUserProfile(string profile)
        {
        }

        public virtual float GetUserPercentage(CrashReporter crashReporter) => 0.0f;

        public virtual List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings) => null;
    }
}
