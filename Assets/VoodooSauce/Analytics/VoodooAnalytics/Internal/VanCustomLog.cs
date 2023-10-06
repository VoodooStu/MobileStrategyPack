using System;
using Voodoo.Analytics;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal class VanCustomLog : IVanCustomLog
    {
        public void LogException(string tag, Exception exception)
        {
            VoodooLog.LogError(Module.ANALYTICS, tag, exception.Message);
        }

        public void ReportException(Exception exception)
        {
            VoodooSauce.LogException(exception);
        }
    }
}