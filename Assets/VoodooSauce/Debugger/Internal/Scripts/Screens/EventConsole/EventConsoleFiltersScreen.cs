using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Debugger
{
    public class EventConsoleFiltersScreen : Screen
    {
        private const string DEFAULT_WRAPPER_NAME_FILTER = "VoodooAnalytics";
        
        [Header("EventFilters"), SerializeField]
        private Transform filtersContainer;
        
        private readonly Queue<DebugToggleButton> _eventFilterItemsPool = new Queue<DebugToggleButton>();
        private readonly List<string> _excludedEventFilters = new List<string>();
        internal EventConsoleListScreen EventConsoleListScreen { private get; set; }

        private void OnEnable()
        {
            ClearFiltersBody();
            FillFiltersBody();
        }

        public override void OnScreenHide()
        {
            base.OnScreenHide();
            EventConsoleListScreen.RefreshEventLogToScreen();
        }

        private IEnumerable<KeyValuePair<string, bool>> GetFiltersWithStates()
        {
            var filtersStates = new SortedDictionary<string, bool>();
            List<DebugAnalyticsLog> analyticsLogs = AnalyticsEventLogger.GetInstance().GetLocalAnalyticsLog(DEFAULT_WRAPPER_NAME_FILTER);
            foreach (DebugAnalyticsLog log in analyticsLogs) {
                filtersStates[log.EventName] = true;
            }

            if (_excludedEventFilters != null) {
                foreach (string filter in _excludedEventFilters.Where(filter => filtersStates.ContainsKey(filter))) {
                    filtersStates[filter] = false;
                }
            }

            return filtersStates;
        }

        private void ClearFiltersBody()
        {
            foreach (Transform filterItem in filtersContainer) {
                filterItem.gameObject.SetActive(false);
                _eventFilterItemsPool.Enqueue(filterItem.GetComponent<DebugToggleButton>());
            }
        }

        private void FillFiltersBody()
        {
            IEnumerable<KeyValuePair<string, bool>> filtersStates = GetFiltersWithStates();
            foreach (KeyValuePair<string, bool> filterState in filtersStates) {
                DebugToggleButton filterItem;
                if (_eventFilterItemsPool.Count == 0) {
                    filterItem = Toggle("", false, null);
                    filterItem.transform.SetParent(filtersContainer);
                } else {
                    filterItem = _eventFilterItemsPool.Dequeue();
                    filterItem.gameObject.SetActive(true);
                    filterItem.SetCallback(null);
                }

                filterItem.Initialize(filterState.Key, filterState.Value, newState => UpdateSavedFilterList(filterState.Key, newState));
            }
        }

        private void UpdateSavedFilterList(string filter, bool isChecked)
        {
            if (isChecked && _excludedEventFilters.Contains(filter)) {
                _excludedEventFilters.Remove(filter);
            } else if (!isChecked && !_excludedEventFilters.Contains(filter)) {
                _excludedEventFilters.Add(filter);
            }
        }

        public bool IsExcluded(DebugAnalyticsLog log)
        {
            return _excludedEventFilters != null && _excludedEventFilters.Any(filter => filter == log.EventName);
        }

        public IEnumerable<DebugAnalyticsLog> FilterEvents(IEnumerable<DebugAnalyticsLog> analyticsLogs)
        {
            if (_excludedEventFilters != null && _excludedEventFilters.Any()) {
                analyticsLogs = analyticsLogs.Where(analyticLog => _excludedEventFilters.All(filter => filter != analyticLog.EventName));
            }

            return analyticsLogs;
        }
    }
}
