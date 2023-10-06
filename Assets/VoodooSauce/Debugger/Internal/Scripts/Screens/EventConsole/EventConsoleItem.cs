using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Voodoo.Sauce.Internal.Analytics;

namespace Voodoo.Sauce.Debugger
{
    public class EventConsoleItem : MonoBehaviour
    {
        [SerializeField] private Text eventNameText;
        [SerializeField] private Text details;
        [SerializeField] private Text timestamp;
        [SerializeField] private GameObject sendingStatus;
        [SerializeField] private GameObject sentStatus;
        [SerializeField] private GameObject errorStatus;
        [SerializeField] private Button infoButton;
        
        internal void UpdateData(DebugAnalyticsLog log, UnityAction infoAction)
        {
            eventNameText.text = log.EventName;
            details.text = log.AdditionalInformation;
            timestamp.text = log.Timestamp.ToLongTimeString();
            switch (log.StateEnum) {
                case DebugAnalyticsStateEnum.ForwardedTo3rdParty:
                    sendingStatus.SetActive(true);
                    sentStatus.SetActive(false);
                    errorStatus.SetActive(false);
                    break;
                case DebugAnalyticsStateEnum.Sent:
                    sendingStatus.SetActive(false);
                    sentStatus.SetActive(true);
                    errorStatus.SetActive(false);
                    break;
                case DebugAnalyticsStateEnum.Error:
                case DebugAnalyticsStateEnum.ErrorSending:
                case DebugAnalyticsStateEnum.SentButErrorFromServer:
                    sendingStatus.SetActive(false);
                    sentStatus.SetActive(false);
                    errorStatus.SetActive(true);
                    break;
            }

            if (infoAction != null) {
                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(infoAction);
            }
        }
    }
}