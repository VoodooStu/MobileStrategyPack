using System;
using UnityEngine;

namespace Voodoo.Sauce.Internal
{
    public interface ILogger
    {
        int ModuleFilter { get; set; }
        LogType LogLevel { get; set; }

        event Action<string, string, LogType> logReceived;

        void Log(string message, Module module, LogType logType);
    }

    public class VoodooLogger : ILogger
    {
        public int ModuleFilter { get; set; }

        public LogType LogLevel { get; set; }

        public event Action<string, string, LogType> logReceived;

        public VoodooLogger()
        {
            Application.logMessageReceived += ApplicationLogReceived;

            LogLevel = LogType.Log;
            ModuleFilter = (int)Module.ALL_MODULES;
        }

        void ApplicationLogReceived(string message, string stackTrace, LogType type)
        {
            logReceived?.Invoke(message, stackTrace, type);
        }

        public void Log(string message, Module module, LogType logType)
        {
            if (LogLevel < logType)
            {
                return;
            }

            if (module != Module.NONE && (ModuleFilter & (int)module) == 0)
            {
                return;
            }

            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}
