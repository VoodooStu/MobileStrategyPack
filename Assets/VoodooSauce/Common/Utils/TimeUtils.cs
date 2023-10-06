using System;

namespace Voodoo.Sauce.Internal.Utils
{
    public static class TimeUtils
    {
        public static DateTime? TimeStampToDateTime(int unixTimeStamp)
        {
            if (unixTimeStamp < 0) return null;
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
        }

        public static int NowAsTimeStamp() => (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static int GetDaysFromSeconds(int seconds) => seconds / (3600 * 24);
    }
}