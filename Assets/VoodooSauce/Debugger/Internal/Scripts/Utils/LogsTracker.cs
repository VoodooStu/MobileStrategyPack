using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Debugger
{
    public static class LogsTracker
    {
        static Queue<LogMessage> _issuesQueue = new Queue<LogMessage>();

        public static event Action<LogMessage> OnLogReceived;
        
        public static IReadOnlyCollection<LogMessage> Issues => _issuesQueue;
        public static int Count => _issuesQueue?.Count ?? 0;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            VoodooLog.logReceived += LogReceived;
        }

        static void LogReceived(string message, string trace, LogType logType)
        {
            LogMessage logMessage = new LogMessage(message, trace, logType);
            _issuesQueue.Enqueue(logMessage);
            OnLogReceived?.Invoke(logMessage);
        }

        public static void Clear() => _issuesQueue.Clear();

        public static int CountFor(LogType logType) => _issuesQueue?.Count(message => message.logType == logType) ?? 0;
    }

    public struct LogMessage
    {
        public LogType logType;
        public string time;
        public string message;
        public string stacktrace;

        public LogMessage(string message, string trace, LogType logType)
        {
            time = DateTime.Now.ToShortDateString();
            this.message = message;
            stacktrace = trace;
            this.logType = logType;
        }
    }
}