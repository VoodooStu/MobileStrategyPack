using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Utils;

#pragma warning disable 1998
#pragma warning disable 4014

namespace Voodoo.Analytics
{
    internal static class Tracker
    {
        private const string TAG = "Tracker";
        private const int MAX_SUSPEND_SENDING_COUNT = 16;
        private static readonly string SaveFolder = Application.persistentDataPath + "/VoodooAnalyticsSDK/";
        private static readonly object CurrentFileLock = new object();

        private static string _currentFilePath;
        private static int _currentFileTimeStamp;
        private static int _currentFileEventCount;
        private static string _bundleId;
        private static IConfig _config;
        private static Timer _sendEventsTimer;
        private static ConcurrentQueue<string> _eventFilesToSend;

        private static int _currentSendingCount;
        private static int _suspendSending;
        internal static void Initialise(IConfig config, string proxyServer, string gatewayUrl)
        {
            _config = config;
            _bundleId = Application.identifier;
            AnalyticsApi.SetAnalyticsGatewayUrl(gatewayUrl);
            AnalyticsApi.ProxyServer = proxyServer;
#if ALTTESTER
            AnalyticsTextFileLogger.ConfigureAutomatedTextLogger(Application.persistentDataPath, Debug.isDebugBuild);
#endif
            Directory.CreateDirectory(SaveFolder);
            EnqueueEventFilesToSend();
            StartSendEventsTimer();
        }

        private static void StartSendEventsTimer()
        {
            _sendEventsTimer = new Timer(_config.GetSenderWaitIntervalSeconds() * 1000);
            _sendEventsTimer.Elapsed += (sender, args) => {
                try {
                    _currentSendingCount++;
                    if (_currentSendingCount >= _suspendSending) {
                        SendEvents();
                        _currentSendingCount = 0;
                    }
                } catch (Exception exception) {
                    // Unfortunately C# Timers catch every exception and make them silent.
                    // That's why I am catching and logging here any exceptions. We shouldn't miss them.
                    AnalyticsLog.CustomLoggerLogE(TAG, exception);
                    AnalyticsLog.CustomLoggerReportE(exception);
                }
            };
            _sendEventsTimer.AutoReset = true;
            _sendEventsTimer.Enabled = true;
        }
        
        internal static void Start() => _sendEventsTimer?.Start();
        internal  static void Stop() => _sendEventsTimer?.Stop();

        private static void EnqueueEventFilesToSend()
        {
            _eventFilesToSend = new ConcurrentQueue<string>();
            var saveFolder = new DirectoryInfo(SaveFolder);
            if (!saveFolder.Exists)
                return;
            //Ignore files starting by a "." to avoid sending .DS_Store created automatically on macOS
            string[] eventFiles = saveFolder.GetFiles().Where(f => !f.Name.StartsWith(".")).OrderBy(f => f.CreationTimeUtc).Select(f => f.FullName).ToArray();
            foreach (var eventFile in eventFiles) {
                _eventFilesToSend.Enqueue(eventFile);
            }
        }

        private static void SendEvents()
        {
            lock (CurrentFileLock) {
                if (_currentFilePath != null)
                    _eventFilesToSend.Enqueue(_currentFilePath);
                _currentFilePath = null;
            }
            while (_eventFilesToSend.TryDequeue(out string filePath)) {
                if (new FileInfo(filePath).LastWriteTimeUtc.AddDays(_config.GetEventLifeTimeInDays()) < DateTimeOffset.UtcNow) {
                    DeleteFile(filePath);
                } else {
                    SendFile(filePath);
                }
            }
        }

        private static void SendFile(string filePath)
        {
            try {
                using (StreamReader streamReader = File.OpenText(filePath)) {
                    List<string> events = new List<string>();
                    events.AddRange(streamReader.ReadToEnd().Split('\n').Where(value => value != ""));
                    if (events.Count == 0) {
                        return;
                    }
                    AnalyticsApi.SendEvents(events, _bundleId, (succeeded,statusCode) => {
                        if (succeeded) {
                            _suspendSending = 0;
                            DeleteFile(filePath);
                        } else if (statusCode == HttpStatusCode.BadRequest) {
                            AnalyticsLog.LogE(TAG, "VAN rejected the event file, please check the events console for more information");
                            //we delete the file to not try to send it infinitely
                            DeleteFile(filePath);
                        } else {
                            if (_suspendSending == 0)
                                _suspendSending = 2;
                            else 
                                _suspendSending *= 2;
                            _suspendSending = _suspendSending > MAX_SUSPEND_SENDING_COUNT ?
                                MAX_SUSPEND_SENDING_COUNT: _suspendSending;
                            _eventFilesToSend.Enqueue(filePath);
                        }
                    });
                }
            } catch (Exception exception) {
                if (exception is UnauthorizedAccessException || exception is DirectoryNotFoundException || exception is IOException) {
                    AnalyticsLog.CustomLoggerLogE(TAG, exception);
                } else {
                    VoodooSauce.LogException(exception);
                }
            }
            
        }

        private static void DeleteFile(string filePath)
        {
            try {
                File.Delete(filePath);
            } catch (Exception exception) {
                if (exception is UnauthorizedAccessException || exception is DirectoryNotFoundException || exception is IOException) {
                    AnalyticsLog.CustomLoggerLogE(TAG, exception);
                } else {
                    VoodooSauce.LogException(exception);
                }
            }
        }

        private static void CreateNewFile()
        {
            _currentFileTimeStamp = TimeUtils.NowAsTimeStamp();
            _currentFilePath = SaveFolder + "events_" + _currentFileTimeStamp + ".json";
            _currentFileEventCount = 0;
        }

        internal static async Task TrackEvent(VanEvent vanEvent)
        {
            if (_config == null) {
                AnalyticsLog.LogE(TAG, $"Can't track event: {vanEvent.GetName()} because the Tracker is not initialised");
            } else if (IsEventAuthorised(vanEvent)) {
                SaveEvent(vanEvent);
            }
        }

        private static bool IsEventAuthorised(VanEvent vanEvent)
        {
            string[] enabledEvents = _config.EnabledEvents();
            if (enabledEvents.Length > 0 && !enabledEvents.Contains(vanEvent.GetName())) {
                AnalyticsLog.Log(TAG, $"Event ignored: {vanEvent.GetName()}");
                return false;
            }
            return true;
        }

        private static async Task SaveEvent(VanEvent vanEvent)
        {
            lock (CurrentFileLock) {
                if (string.IsNullOrEmpty(_currentFilePath)) {
                    CreateNewFile();
                } else if (_currentFileEventCount >= _config.GetMaxNumberOfEventsPerFile() ||
                    TimeUtils.NowAsTimeStamp() > _currentFileTimeStamp + _config.GetFileIntervalInSeconds()) {
                    _eventFilesToSend.Enqueue(_currentFilePath);
                    CreateNewFile();
                }
            
                try {
                    //Remove all newline character so the splitting when processing the file can be done properly
                    String eventJson = vanEvent.ToJson().Replace("\n","");
                    //If the json string are empty should not save the event to the file
                    if(string.IsNullOrEmpty(eventJson)) 
                        return;
                    using (StreamWriter streamWriter = File.AppendText(_currentFilePath)) {
                        streamWriter.Write(eventJson + "\n");
                    }
                    _currentFileEventCount++;
                } catch (Exception exception) {
                    if (exception is UnauthorizedAccessException || exception is DirectoryNotFoundException || exception is IOException) {
                        AnalyticsLog.CustomLoggerLogE(TAG, exception);
                    } else {
                        VoodooSauce.LogException(exception);
                    }
                }
            }
        }
    }
}