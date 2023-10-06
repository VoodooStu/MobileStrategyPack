using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Extension;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Debugger
{
    // ReSharper disable once CheckNamespace
    public class EventConsoleListScreen : Screen
    {
        private const string DEFAULT_WRAPPER_NAME_FILTER = "VoodooAnalytics";

        [Header("EventConsole"), SerializeField]
        private EventConsoleInformationScreen eventInformationScreen;
        [SerializeField]
        private EventConsoleFiltersScreen eventFiltersScreen;
        [SerializeField]
        private Transform sessionContainer;
        [SerializeField]
        private Button recordButton;
        [SerializeField]
        private Button flushButton;
        [SerializeField]
        private Button filterButton;
        
        [SerializeField]
        private Image recordingImage;
        [SerializeField]
        private Image pauseImage;

        [SerializeField]
        private Button recordAtStartupBtnOn;
        [SerializeField]
        private Button recordAtStartupBtnOff;
        
        [Header("Prefab"), SerializeField]
        private EventConsoleItem eventLogItemPrefab;
        [SerializeField]
        private EventConsoleSessionItem sessionItemPrefab;
        
        private static readonly ConcurrentQueue<Action> _runOnMainThread = new ConcurrentQueue<Action>();
        private readonly Dictionary<string, EventConsoleItem> _eventDictionary = new Dictionary<string, EventConsoleItem>();
        private readonly Dictionary<string, EventConsoleSessionItem> _sessionDictionary = new Dictionary<string, EventConsoleSessionItem>();
        private readonly Stack<EventConsoleItem> _eventPool = new Stack<EventConsoleItem>();

        private void Awake()
        {
            recordButton.onClick.AddListener(RecordPressed);
            flushButton.onClick.AddListener(FlushLog);
            filterButton.onClick.AddListener(ShowFiltersScreen);
            recordAtStartupBtnOn.onClick.AddListener(OnRecordAtStartupBtnClick);
            recordAtStartupBtnOff.onClick.AddListener(OnRecordAtStartupBtnClick);
            eventFiltersScreen.EventConsoleListScreen = this;
        }

        private void Start()
        {
            RefreshEventLogToScreen();
            AnalyticsEventLogger.OnAnalyticsEventStateChanged += HandleNewEventEmitted;
        }

        private void OnDestroy()
        {
            AnalyticsEventLogger.OnAnalyticsEventStateChanged -= HandleNewEventEmitted;
            recordButton.onClick.RemoveListener(RecordPressed);
        }

        private void Update()
        {
            if (!_runOnMainThread.IsEmpty) {
                while (_runOnMainThread.TryDequeue(out Action action)) {
                    action?.Invoke();
                }
            }
        }

        private void OnEnable()
        {
            StartCoroutine(RefreshOnEnable());
            UpdateRecordAtStartupBtnDisplay();
            UpdateRecordButtonState();
        }

        private IEnumerator RefreshOnEnable()
        {
            yield return null;
            RefreshSessionsSize();
        }

        private static void EnableAnalyticsLoggingAndDebugging(bool isEnabled)
        {
            AnalyticsEventLogger.GetInstance().SetAnalyticsEventRecording(isEnabled);
        }

        private static void UpdateButtonState(bool enable, Selectable button, Graphic buttonIcon)
        {
            if (button.interactable == enable) return;
            button.interactable = enable;
            ColorBlock colors = button.colors;
            buttonIcon.color = enable ? colors.normalColor : colors.disabledColor;
        }

        private void ShowFiltersScreen()
        {
            Debugger.Show(eventFiltersScreen);
        }

        internal void RefreshEventLogToScreen()
        {
            IEnumerable<DebugAnalyticsLog> analyticsLogs = AnalyticsEventLogger.GetInstance().GetLocalAnalyticsLog(DEFAULT_WRAPPER_NAME_FILTER);
            //Update buttons state
            bool hasAnalyticsLogs = analyticsLogs.Any();
            flushButton.interactable = hasAnalyticsLogs;
            filterButton.interactable = hasAnalyticsLogs;
            
            analyticsLogs = eventFiltersScreen.FilterEvents(analyticsLogs);
            
            foreach (KeyValuePair<string, EventConsoleItem> eventObject in _eventDictionary.ToList()) {
                eventObject.Value.gameObject.SetActive(false);
                _eventDictionary.Remove(eventObject.Key);
                _eventPool.Push(eventObject.Value);
            }

            foreach (KeyValuePair<string, EventConsoleSessionItem> session in _sessionDictionary.ToList()) {
                session.Value.gameObject.SetActive(false);
            }

            foreach (DebugAnalyticsLog log in analyticsLogs) {
                CreateEvent(log);
            }
        }

        private void HandleNewEventEmitted(DebugAnalyticsLog log, bool isUpdateFromExisting)
        {
            // Ignore not VAN event 
            if (!log.WrapperName.Contains(DEFAULT_WRAPPER_NAME_FILTER)) return;
            
            flushButton.interactable = true;
            filterButton.interactable = true;
            
            if (eventFiltersScreen.IsExcluded(log)) return;

            if (UnityThreadExecutor.IsMainThread) {
                DisplayEvent(log);
            } else {
                _runOnMainThread.Enqueue(() => DisplayEvent(log));
            }
        }

        private void DisplayEvent(DebugAnalyticsLog log)
        {
            if (!_eventDictionary.ContainsKey(log.EventId)) {
                CreateEvent(log);
            } else {
                UpdateEvent(log);
            }
        }

        private void CreateEvent(DebugAnalyticsLog log)
        {
            EventConsoleSessionItem sessionItem = _sessionDictionary.ContainsKey(log.SessionId)
                ? _sessionDictionary[log.SessionId]
                : CreateSession(log.SessionId);

            EventConsoleItem eventConsoleLine;
            if (_eventPool.Count == 0) {
                eventConsoleLine = Instantiate(eventLogItemPrefab, sessionItem.GetContainer);
            } else {
                eventConsoleLine = _eventPool.Pop();
                eventConsoleLine.transform.SetParent(sessionItem.GetComponent<EventConsoleSessionItem>().GetContainer);
                sessionItem.gameObject.SetActive(true);
                eventConsoleLine.gameObject.SetActive(true);
            }

            _eventDictionary.Add(log.EventId, eventConsoleLine);
            eventConsoleLine.transform.SetAsFirstSibling();
            eventConsoleLine.UpdateData(log, () => ShowEventDescription(log));
            eventConsoleLine.transform.RefreshHierarchySize();
        }

        private void ShowEventDescription(DebugAnalyticsLog log)
        {
            Debugger.Show(eventInformationScreen);
            eventInformationScreen.ShowEventDescription(log);
        }

        private void RefreshSessionsSize()
        {
            foreach (KeyValuePair<string, EventConsoleSessionItem> session in _sessionDictionary.ToList()) {
                session.Value.GetContainer.RefreshHierarchySize();
            }
        }

        private void UpdateEvent(DebugAnalyticsLog log)
        {
            _eventDictionary.TryGetValue(log.EventId, out EventConsoleItem eventGameObject);
            if (eventGameObject != null) eventGameObject.UpdateData(log, () => ShowEventDescription(log));
        }

        private EventConsoleSessionItem CreateSession(string sessionId)
        {
            var session = Instantiate(sessionItemPrefab, sessionContainer);
            session.transform.SetAsFirstSibling();
            _sessionDictionary.Add(sessionId, session);
            session.Initialize(_sessionDictionary.Count, sessionId);
            return session;
        }

        private void FlushLog()
        {
            AnalyticsEventLogger.GetInstance().FlushAnalyticsLogs();
            RefreshEventLogToScreen();
            RefreshSessionsSize();
        }

        private void RecordPressed()
        {
            EnableAnalyticsLoggingAndDebugging(!AnalyticsEventLogger.GetInstance().IsRecordingEvents);
            UpdateRecordButtonState();
        }

        private void OnRecordAtStartupBtnClick()
        {
            AnalyticsEventLogger.GetInstance().IsRecordingAtStartup =
                !AnalyticsEventLogger.GetInstance().IsRecordingAtStartup;
            UpdateRecordAtStartupBtnDisplay();
        }

        private void UpdateRecordAtStartupBtnDisplay()
        {
            bool recordingAtStartup = AnalyticsEventLogger.GetInstance().IsRecordingAtStartup;
            recordAtStartupBtnOn.gameObject.SetActive(recordingAtStartup);
            recordAtStartupBtnOff.gameObject.SetActive(!recordingAtStartup);
        }

        private void UpdateRecordButtonState()
        {
            bool isRecording = AnalyticsEventLogger.GetInstance().IsRecordingEvents;
            recordingImage.gameObject.SetActive(!isRecording);
            pauseImage.gameObject.SetActive(isRecording);
        }
    }
}
