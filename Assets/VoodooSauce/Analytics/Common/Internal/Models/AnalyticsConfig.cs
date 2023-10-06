using System;
using Voodoo.Analytics;

namespace Voodoo.Sauce.Internal.Analytics
{
    [Serializable]
    public class AnalyticsConfig : IConfig
    {
        private int waitIntervalSeconds = 5;
        private int maxNumberOfEventsPerFile = 50;
        private string enabledEvents = "";
        private int sessionIdRenewalIntervalInSeconds = 300;
        private int eventLifeTimeInDays = 7;
        private int fileIntervalInSeconds = 300;
        
        public void SetSenderWaitIntervalSeconds(int seconds)
        {
            waitIntervalSeconds = seconds;
        }
        
        public int GetSenderWaitIntervalSeconds()
        {
            return waitIntervalSeconds;
        }

        public int GetMaxNumberOfEventsPerFile()
        {
            return maxNumberOfEventsPerFile;
        }

        public string[] EnabledEvents()
        {
            if (String.IsNullOrEmpty(enabledEvents)) {
                return new string[] { };
            }

            return enabledEvents.Split('|');
        }

        public int GetSessionIdRenewalIntervalInSeconds()
        {
            return sessionIdRenewalIntervalInSeconds;
        }

        public int GetEventLifeTimeInDays()
        {
            return eventLifeTimeInDays;
        }

        public int GetFileIntervalInSeconds()
        {
            return fileIntervalInSeconds;
        }

        public override string ToString()
        {
            var enabledEventsString = "all";
            if (EnabledEvents().Length > 0) {
                enabledEventsString = string.Join(", ", EnabledEvents());
            }

            return "- wait interval: " + waitIntervalSeconds +
                "\n- max number of events per file: " + maxNumberOfEventsPerFile +
                "\n- session id renewal interval: " + sessionIdRenewalIntervalInSeconds +
                "\n- enabled events: " + enabledEventsString;
        }
    }
}