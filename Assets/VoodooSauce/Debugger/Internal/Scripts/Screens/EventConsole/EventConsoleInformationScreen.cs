using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Extension;

namespace Voodoo.Sauce.Debugger
{
    public class EventConsoleInformationScreen : Screen
    {
        private const string DEFAULT_HEADER_FIELDS = "other header fields";
        private const string EVENT_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
        
        [Header("Console Information"), SerializeField]
        private EventConsoleErrorScreen errorScreen;
        
        [SerializeField]
        private EventInformationList eventInformationListPrefab;
        [SerializeField]
        private EventInformationLineItem eventInformationLineItemPrefab;
        [SerializeField]
        private Text eventName;
        [SerializeField]
        private Text eventDate;

        [SerializeField]
        private GameObject eventSendingStatus;
        [SerializeField]
        private GameObject eventSentStatus;
        [SerializeField]
        private GameObject eventErrorStatus;
        
        [SerializeField]
        private Transform eventInformationContainer;
        [SerializeField]
        private Button eventCopyJson;
        [SerializeField]
        private Button eventSeeError;
        
        private readonly Dictionary<string, EventInformationList> _eventInformationListDictionary = new Dictionary<string, EventInformationList>();
        private readonly Stack<EventInformationLineItem> _eventInformationLinePool = new Stack<EventInformationLineItem>();

        private void OnEnable()
        {
            RefreshInformationSize();
        }

        private EventInformationList CreateEventInformationList(string title)
        {
            var eventInformationList = Instantiate(eventInformationListPrefab, eventInformationContainer);
            _eventInformationListDictionary.Add(title, eventInformationList);
            eventInformationList.Initialize(title);
            return eventInformationList;
        }

        private void CreateEventInformationLine(KeyValuePair<string, object> information, Transform parent)
        {
            EventInformationLineItem informationLine;
            if (_eventInformationLinePool.Count == 0) {
                informationLine = Instantiate(eventInformationLineItemPrefab, parent);
            } else {
                informationLine = _eventInformationLinePool.Pop();
                informationLine.transform.SetParent(parent);
                informationLine.gameObject.SetActive(true);
            }

            informationLine.UpdateData(information);
        }

        private void UpdateEventInformationHeader(DebugAnalyticsLog log)
        {
            eventName.text = log.EventName.BoldText();
            eventCopyJson.onClick.RemoveAllListeners();
            eventCopyJson.onClick.AddListener(() => CopyEventJson(log));
            eventDate.text = log.Timestamp.ToString(EVENT_DATE_FORMAT).BoldText();
            switch (log.StateEnum) {
                case DebugAnalyticsStateEnum.ForwardedTo3rdParty:
                    eventSendingStatus.SetActive(true);
                    eventSentStatus.SetActive(false);
                    eventErrorStatus.SetActive(false);
                    break;
                case DebugAnalyticsStateEnum.Sent:
                    eventSendingStatus.SetActive(false);
                    eventSentStatus.SetActive(true);
                    eventErrorStatus.SetActive(false);
                    break;
                case DebugAnalyticsStateEnum.Error:
                case DebugAnalyticsStateEnum.ErrorSending:
                case DebugAnalyticsStateEnum.SentButErrorFromServer:
                    eventSendingStatus.SetActive(false);
                    eventSentStatus.SetActive(false);
                    eventErrorStatus.SetActive(true);
                    eventSeeError.onClick.RemoveAllListeners();
                    eventSeeError.onClick.AddListener(() => OnErrorClicked(log));
                    break;
            }
        }

        private void UpdateEventInformationBody(DebugAnalyticsLog log)
        {
            EventInformationList defaultEventInformationList = _eventInformationListDictionary.ContainsKey(DEFAULT_HEADER_FIELDS)
                ? _eventInformationListDictionary[DEFAULT_HEADER_FIELDS]
                : CreateEventInformationList(DEFAULT_HEADER_FIELDS);

            foreach (KeyValuePair<string, object> parameter in log.Parameters) {
                if (parameter.Value is Dictionary<string, object> value) {
                    EventInformationList eventInformationList = _eventInformationListDictionary.ContainsKey(parameter.Key)
                        ? _eventInformationListDictionary[parameter.Key]
                        : CreateEventInformationList(parameter.Key);

                    foreach (KeyValuePair<string, object> information in value) {
                        CreateEventInformationLine(information, eventInformationList.GetContainer);
                    }

                    eventInformationList.gameObject.SetActive(true);
                } else {
                    CreateEventInformationLine(parameter, defaultEventInformationList.GetContainer);
                    defaultEventInformationList.gameObject.SetActive(true);
                }

                defaultEventInformationList.transform.SetAsLastSibling();
            }
        }

        private void OnErrorClicked(DebugAnalyticsLog log)
        {
            errorScreen.ShowErrorMessage(log);
        }

        public void ShowEventDescription(DebugAnalyticsLog log)
        {
            UpdateEventInformationHeader(log);
            CleanEventInformationBody();
            UpdateEventInformationBody(log);
            RefreshInformationSize();
        }
        
        private void CleanEventInformationBody()
        {
            foreach (KeyValuePair<string, EventInformationList> eventInformationList in _eventInformationListDictionary) {
                foreach (Transform eventInformationLine in eventInformationList.Value.GetContainer) {
                    eventInformationLine.gameObject.SetActive(false);
                    _eventInformationLinePool.Push(eventInformationLine.GetComponent<EventInformationLineItem>());
                }

                eventInformationList.Value.gameObject.SetActive(false);
            }
        }

        private void RefreshInformationSize()
        {
            foreach (KeyValuePair<string, EventInformationList> eventInformationList in _eventInformationListDictionary
                        .Where(keyValue => keyValue.Value.gameObject.activeSelf)) {
                eventInformationList.Value.GetContainer.RefreshHierarchySize();
            }
        }

        private static void CopyEventJson(DebugAnalyticsLog log)
        {
            log.Parameters.ToJson().CopyToClipboard();
        }
    }
}
