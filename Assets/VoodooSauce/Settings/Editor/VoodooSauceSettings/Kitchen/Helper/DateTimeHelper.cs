using System;

namespace Voodoo.Sauce.Internal.VoodooSauceSettings.Kitchen
{
    internal static class DateTimeHelper
    {
        private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

        internal static DateTime StringToDateTime(string dateTime) => DateTime.Parse(dateTime);
        
        internal static string DateTimeToString(DateTime dateTime) => dateTime.ToString(DATE_FORMAT);
    }
}