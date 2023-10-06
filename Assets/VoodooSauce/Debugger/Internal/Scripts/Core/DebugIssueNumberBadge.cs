using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Debugger;

namespace Voodoo.Sauce.Internal.DebugScreen
{
    public class DebugIssueNumberBadge : MonoBehaviour
    {
        [SerializeField]
        private Text numberText;

        [SerializeField]
        private bool hideIfEmpty;

        [SerializeField]
        private GameObject badgeObject;

        private int _issuesCount = 0;

        private void Awake()
        {
            LogConsoleDebugScreen.Closed += RefreshCounts;
            RefreshCounts();
        }

        private void OnDestroy()
        {
            LogConsoleDebugScreen.Closed -= RefreshCounts;
        }

        private void OnEnable()
        {
            LogsTracker.OnLogReceived += LogsTrackerOnIssueMessageReceived;
            RefreshCounts(); 
        }
        
        private void OnDisable()
        {
            LogsTracker.OnLogReceived -= LogsTrackerOnIssueMessageReceived;
        }

        private void RefreshCounts()
        {
            _issuesCount = 0; 
            
            IReadOnlyCollection<LogMessage> issuesCollection = LogsTracker.Issues;
            
            if (issuesCollection == null) {
                numberText.text = "???"; 
            } else {
                foreach (LogMessage issueMessage in issuesCollection) {
                    if (MustIssueBeCounted(issueMessage)) {
                        _issuesCount++; 
                    }
                }
            
                numberText.text = $"{_issuesCount}";
            }
            
            badgeObject.SetActive(!hideIfEmpty || _issuesCount > 0);
        }
        
        private void LogsTrackerOnIssueMessageReceived(LogMessage issueMessage)
        {
            if (!MustIssueBeCounted(issueMessage))
            {
                return;
            }
            
            _issuesCount++;
            numberText.text = $"{_issuesCount}";
            badgeObject.SetActive(true);
        }

        private static bool MustIssueBeCounted(LogMessage issueMessage) =>
            DebugLogTypes.IsEnabled(issueMessage.logType) && LogConsoleDebugScreen.MessageMatchesSearchText(issueMessage);
    }
}