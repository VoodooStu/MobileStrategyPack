using System;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Extension
{
    public static class DateTimeExtension
    {
#region Constants

        private const string ISO_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string DEFAULT_CULTURE_INFO = "fr-FR";

#endregion
        
        public static string ToIsoFormat(this DateTime dateTime, IFormatProvider cultureInfo = null) =>
            dateTime.ToString(ISO_FORMAT, cultureInfo ?? new CultureInfo(DEFAULT_CULTURE_INFO));
    }
}