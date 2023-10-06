namespace Voodoo.Sauce.Internal.Analytics
{
    public class VoodooTuneInitAnalyticsInfo
    {
        public long HttpResponseCode;

        public double DurationInMilliseconds;

        public bool HasTimeout;

        public bool HasCache;

        public bool FormatIssue;
    }

    public class VoodooTuneInitAnalyticsInfoLog
    {
        public VoodooTuneInitAnalyticsInfo infos;
        public string response;
        public string error;
        public string message;

        public VoodooTuneInitAnalyticsInfoLog() { }

        public VoodooTuneInitAnalyticsInfoLog(VoodooTuneInitAnalyticsInfo infos, string response, string error, string message)
        {
            this.infos = infos;
            this.response = response;
            this.error = error;
            this.message = message;
        }
    }
}