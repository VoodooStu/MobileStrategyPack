using System;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.CrashReport
{
    public interface ICrashlyticsProvider
    {
        /// <summary>
        /// Configure the crash reporter without initializing it or starting it.
        /// </summary>
        /// <param name="settings">The VoodooSauce settings that could contain useful settings to initialize this crash reporter</param>
        void Configure(VoodooSettings settings);
        
        /// <summary>
        /// This method is called when this crash reporter is chosen for this user among all the crash reporters.
        /// In this method the initialization of the crash reporter should only be prepared, the complete initialization
        /// must be done when the event analyticsConsentEvent is fired and if the boolean parameter is equal to true.
        /// </summary>
        /// <param name="analyticsConsentEvent">This event is called when the user gives its consent or not</param>
        void Initialize(ref Action<bool> analyticsConsentEvent);

        /// <summary>
        /// Returns the enum value from CrashReportCore.CrashReporter corresponding to that crash reporter.
        /// </summary>
        /// <returns>Enum value of the type of the loaded crash reporter</returns>
        internal CrashReportCore.CrashReporter CrashReporterType();
        
        /// <summary>
        /// Returns the percentage of users which will use this crash reporter.
        /// The returned value must be between 0.0 (disabled) and 100.0
        /// Return float.MinValue to automatically manage the percentage.
        /// </summary>
        /// <returns>The percentage of users using this crash reporter</returns>
        internal float UserPercentage();

        /// <summary>
        /// Log the start of a level to the crash reporter.
        /// </summary>
        /// <param name="level">Current level to log</param>
        void LogLevelStart(string level);

        /// <summary>
        /// Log the end of a level to the crash reporter.
        /// </summary>
        /// <param name="level">Current level to log</param>
        void LogLevelFinish(string level);

        /// <summary>
        /// Log an exception in the crash reporter.
        /// </summary>
        /// <param name="exception">Exception to log</param>
        void LogException(Exception exception);

        /// <summary>
        /// Set a key value pair to be sent when crashes are registered in the crash reporter.
        /// </summary>
        /// <param name="key">Key to track in the crash reporter</param>
        /// <param name="value">Associated value to track in the crash reporter</param>
        void SetCustomData(string key, string value);

        /// <summary>
        /// Set the identifier of the user to track in the crash reporter.
        /// </summary>
        /// <param name="userId">User identifier to track in the crash reporter</param>
        void SetUserId(string userId);

        /// <summary>
        /// Set the profile of the user to track in the crash reporter.
        /// </summary>
        /// <param name="profile">User profile to track in the crash reporter</param>
        void SetUserProfile(string profile);

        /// <summary>
        /// Log a message in the crash reporter.
        /// </summary>
        /// <param name="message">Message to log in the crash reporter</param>
        void Log(string message);
    }
}