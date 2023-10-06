using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal;
using Voodoo.Sauce.Internal.DebugScreen;

namespace Voodoo.Sauce.Debugger
{
    public class LogConsoleDebugScreen : Screen
    {
        [Serializable]
        internal class LogTypeFields
        {
            public Toggle toggle;
            public Color color;
            public Text titleText;

            [NonSerialized]
            public int count;
        }

        [SerializeField]
        private Transform issueListItemParent;
        [SerializeField]
        private DebugLogMessageListItem issueListItemPrefab;
        [SerializeField]
        private DebugLogMessagePopup logMessagePopup;
        [SerializeField]
        private Button clearAllButton;
        [SerializeField]
        private Button optionButton;
        [SerializeField]
        private LogsOptionDebugScreen optionScreen;

        [SerializeField]
        private ScrollRect issuesScrollRect;
        [SerializeField]
        private Button scrollDownButton;

        [SerializeField]
        private InputField searchInputField;
        [SerializeField]
        private Text searchCountText;
        
        [SerializeField]
        private LogTypeFields exceptionFields;
        [SerializeField]
        private LogTypeFields errorFields;
        [SerializeField]
        private LogTypeFields warningFields;

        private static string _searchString;
        private Dictionary<LogType, LogTypeFields> _logTypeDict;
        private readonly List<DebugLogMessageListItem> _issueMessageList = new List<DebugLogMessageListItem>();
        
        public static Action Closed;

        private void Awake()
        {
            optionButton.onClick.AddListener(() => Debugger.Show(optionScreen));

            _logTypeDict = new Dictionary<LogType, LogTypeFields> {
                { LogType.Exception, exceptionFields },
                { LogType.Error, errorFields },
                { LogType.Warning, warningFields }
            };

            logMessagePopup.Hide();

            foreach (KeyValuePair<LogType, LogTypeFields> pair in _logTypeDict) {
                LogType logType = pair.Key;
                LogTypeFields logFields = pair.Value;
                logFields.toggle.isOn = DebugLogTypes.IsEnabled(logType);
                logFields.toggle.onValueChanged.AddListener((isToggled) => ToggleLogType(isToggled, logType));
                logFields.toggle.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = logFields.color;
            }

            searchInputField.onEndEdit.AddListener(OnSearchEndEdit);

            clearAllButton.onClick.AddListener(delegate {
                LogsTracker.Clear();
                ClearExceptionsList();
            });

            scrollDownButton.onClick.AddListener(delegate { issuesScrollRect.normalizedPosition = new Vector2(0, 0); });
        }

        void OnLogEnableChange(bool value) => VoodooLog.EnableDebugLogs(value);
        
        private void OnDestroy()
        {
            searchInputField.onEndEdit.RemoveAllListeners();
            clearAllButton.onClick.RemoveAllListeners();
            scrollDownButton.onClick.RemoveAllListeners();
        }

        private int CountActiveMessageListItems() => _issueMessageList.Count(messageListItem => messageListItem.gameObject.activeInHierarchy);

        private void OnSearchEndEdit(string searchString)
        {
            _searchString = searchString.ToLower();

            UpdateMessageList();
            UpdateSearchCountText(CountActiveMessageListItems());
        }

        private void UpdateMessageList()
        {
            foreach (DebugLogMessageListItem messageListItem in _issueMessageList) {
                if (MessageMatchesSearchText(messageListItem.LogMessage)) {
                    bool isEnabled = DebugLogTypes.IsEnabled(messageListItem.LogMessage.logType);
                    messageListItem.gameObject.SetActive(isEnabled);
                } else {
                    messageListItem.gameObject.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            LogsTracker.OnLogReceived += LogsTrackerOnIssueMessageReceived;
            InitIssuesList();
        }

        private void OnDisable()
        {
            LogsTracker.OnLogReceived -= LogsTrackerOnIssueMessageReceived;
            ClearExceptionsList();

            Closed?.Invoke();
        }

        private void InitIssuesList()
        {
            IReadOnlyCollection<LogMessage> issuesCollection = LogsTracker.Issues;

            if (issuesCollection == null) return;

            int count = 0;
            foreach (LogMessage issueMessage in issuesCollection) 
            {
                DebugLogMessageListItem item = AddLogMessage(issueMessage);
                if (item == null)
                {
                    continue;
                }
                
                if (DebugLogTypes.IsEnabled(issueMessage.logType) && MessageMatchesSearchText(issueMessage))
                { 
                    count++;
                }
                else
                { 
                    item.gameObject.SetActive(false);
                }
            }

            UpdateSearchCountText(count);

            foreach (KeyValuePair<LogType, LogTypeFields> pair in _logTypeDict) {
                SetLogTypeCount(pair.Key, LogsTracker.CountFor(pair.Key));
            }
        }

        private void ShowLogMessagePopup(LogMessage logMessage) => logMessagePopup.Show(logMessage);
        
        private void SetLogTypeCount(LogType logType, int count = -1)
        {
            LogTypeFields logFields = _logTypeDict[logType];
            logFields.titleText.text = logType + " (" + count + ")";
            logFields.count = count;
        }

        private void IncrementLogTypeCount(LogType logType)
        {
            _logTypeDict[logType].count += 1;
            SetLogTypeCount(logType, _logTypeDict[logType].count++);
        }

        private void ToggleLogType(bool isActive, LogType logType)
        {
            foreach (DebugLogMessageListItem messageListItem in _issueMessageList.Where(messageListItem =>
                         messageListItem.LogMessage.logType == logType)) {
                messageListItem.gameObject.SetActive(isActive && MessageMatchesSearchText(messageListItem.LogMessage));
            }

            DebugLogTypes.Update(logType, isActive);

            UpdateSearchCountText(CountActiveMessageListItems());
        }

        private void ClearExceptionsList()
        {
            foreach (Transform child in issueListItemParent) {
                Destroy(child.gameObject);
            }

            _issueMessageList.Clear();

            UpdateSearchCountText(0);

            foreach (KeyValuePair<LogType, LogTypeFields> pair in _logTypeDict) {
                SetLogTypeCount(pair.Key, 0);
            }
        }

        private void LogsTrackerOnIssueMessageReceived(LogMessage issueMessage) => AddLogMessage(issueMessage);
        
        private DebugLogMessageListItem AddLogMessage(LogMessage issueMessage)
        {
            if (_logTypeDict.ContainsKey(issueMessage.logType) == false)
            {
                return null;
            }

            DebugLogMessageListItem messageListItem = Instantiate(issueListItemPrefab, issueListItemParent);
            messageListItem.Initialize(_logTypeDict[issueMessage.logType].color, issueMessage, OnMessageListClicked);
            _issueMessageList.Add(messageListItem);
            IncrementLogTypeCount(issueMessage.logType);

            messageListItem.gameObject.SetActive(DebugLogTypes.IsEnabled(issueMessage.logType));

            if (!MessageMatchesSearchText(issueMessage)) {
                messageListItem.gameObject.SetActive(false);
            }

            return messageListItem;
        }

        private void OnMessageListClicked(LogMessage logMessage)
        {
            ShowLogMessagePopup(logMessage);
        }

        public static bool MessageMatchesSearchText(LogMessage issueMessage) 
            => string.IsNullOrWhiteSpace(_searchString) || 
            issueMessage.message.ToLower().Contains(_searchString) || 
            issueMessage.stacktrace.ToLower().Contains(_searchString);

        private void UpdateSearchCountText(int count) => searchCountText.text = $"[{count} results]";
        
        public override BadgeCounter Counter() => new IssuesCounter();
    }

    public class IssuesCounter : BadgeCounter
    {
        private int _issuesCount = 0;

        public override void Start()
        {
            Refresh();
            update?.Invoke(_issuesCount);
            LogsTracker.OnLogReceived += LogsTrackerOnIssueMessageReceived;
        }

        public override void Stop()
        {
            LogsTracker.OnLogReceived -= LogsTrackerOnIssueMessageReceived;
            update?.Invoke(_issuesCount);
        }

        private void Refresh()
        {
            _issuesCount = 0; 
            
            IReadOnlyCollection<LogMessage> issuesCollection = LogsTracker.Issues;
            
            if (issuesCollection == null) {
                _issuesCount = -1; 
            } else {
                foreach (LogMessage issueMessage in issuesCollection) {
                    if (MustIssueBeCounted(issueMessage)) {
                        _issuesCount++; 
                    }
                }
            }
        }
        
        private void LogsTrackerOnIssueMessageReceived(LogMessage issueMessage)
        {
            if (!MustIssueBeCounted(issueMessage))
            {
                return;
            }
            
            _issuesCount++;
            update?.Invoke(_issuesCount);
        }

        private static bool MustIssueBeCounted(LogMessage issueMessage) =>
            DebugLogTypes.IsEnabled(issueMessage.logType) && LogConsoleDebugScreen.MessageMatchesSearchText(issueMessage);
    }
}