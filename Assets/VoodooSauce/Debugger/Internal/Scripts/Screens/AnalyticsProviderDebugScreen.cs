using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;

namespace Voodoo.Sauce.Debugger
{
    public class AnalyticsProviderDebugScreen : Screen
    {
        private const string DEFAULT_TEST_EVENT_NAME = "test_analytics_provider_";

        private readonly List<AnalyticsProviderButton> _providerUiItemList = new List<AnalyticsProviderButton>();
        private List<VoodooSauce.AnalyticsProvider> _providerEnumList;

        private void Start()
        {
            _providerEnumList = GetAllProviderEnum();
            
            for (var i = 0; i < _providerEnumList.Count; i++)
            {
                SetupAnalyticsItem(i);
            }

            AnalyticsEventLogger.OnAnalyticsEventStateChanged += ReceiveAnalyticsEvent;
        }

        private List<VoodooSauce.AnalyticsProvider> GetAllProviderEnum()
            => Enum.GetValues(typeof(VoodooSauce.AnalyticsProvider))
                   .OfType<VoodooSauce.AnalyticsProvider>()
                   .ToList();

        private void SetupAnalyticsItem(int index)
        {
            VoodooSauce.AnalyticsProvider provider = _providerEnumList[index];

            var analyticsLineItem = WidgetUtility.InstanceOf<AnalyticsProviderButton>(Parent);
            analyticsLineItem.SetOnClickListener(() => ProviderTestClicked(index));
            analyticsLineItem.SetTitleText(provider.ToString());
            analyticsLineItem.SetDescText("");

            _providerUiItemList.Add(analyticsLineItem);
        }

        private void ProviderTestClicked(int index)
        {
            AnalyticsEventLogger.GetInstance().SetAnalyticsDebugging(true);
            BaseAnalyticsProviderWithLogger providerLogger = AnalyticsManager.GetProviderLoggerWithEnum(_providerEnumList[index]);
            _providerUiItemList[index].SetDescText(GetDescMessageOnSending(providerLogger));
            if (providerLogger == null)
            {
                _providerUiItemList[index].SetButtonColor(Color.red);
            }
            else if (providerLogger.IsInitialized)
            {
                AnalyticsManager.TrackCustomEvent(GetTestEventName(index), null, null, GetProviderList(index));
            }
            else
            {
                _providerUiItemList[index].SetButtonColor(Color.red);
            }
        }

        private static string GetDescMessageOnSending(BaseAnalyticsProviderWithLogger providerWithLogger)
        {
            if (providerWithLogger == null) return "Provider is not implementing logger interface";
            return !providerWithLogger.IsInitialized ? providerWithLogger.GetUninitializedErrorMessage() : "Sending test event";
        }
        
        private static string GetTestEventName(int index) => $"{DEFAULT_TEST_EVENT_NAME}{index}";
        
        private List<VoodooSauce.AnalyticsProvider> GetProviderList(int index)
        {
            VoodooSauce.AnalyticsProvider provider = _providerEnumList[index];
            return new List<VoodooSauce.AnalyticsProvider>() { provider };
        }

        private void ReceiveAnalyticsEvent(DebugAnalyticsLog log, bool isUpdateFromExisting)
        {
            if (int.TryParse(log.EventName?.Replace(DEFAULT_TEST_EVENT_NAME, string.Empty), out int index) == false)
            {
                return;
            }

            UnityThreadExecutor.Execute(() => {
                _providerUiItemList[index].SetDescText(GetDescTextFromState(log));
                _providerUiItemList[index].SetButtonColor(log.StateEnum == DebugAnalyticsStateEnum.Sent ? Color.blue : Color.green);
            });
        }

        private static string GetDescTextFromState(DebugAnalyticsLog log)
        {
            switch (log.StateEnum)
            {
                case DebugAnalyticsStateEnum.Sent:
                    return "Event Test Sent to Analytics Server Successfully";
                case DebugAnalyticsStateEnum.ForwardedTo3rdParty:
                    return "Event Test Forwarded to 3rd Party SDK";
                case DebugAnalyticsStateEnum.ErrorSending:
                    return "Error Sending Event Test";
                case DebugAnalyticsStateEnum.SentButErrorFromServer:
                    return "Sent but Error from Server";
                case DebugAnalyticsStateEnum.Error:
                    return "Event generated an error";
                default:
                    return "Error Sending Event Test";
            }
        }

        private void OnDestroy()
        {
            AnalyticsEventLogger.OnAnalyticsEventStateChanged -= ReceiveAnalyticsEvent;
            AnalyticsEventLogger.GetInstance().SetAnalyticsDebugging(false);
        }
    }
}