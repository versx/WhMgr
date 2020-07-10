namespace WhMgr.Extensions
{
    using System;

    using TimeZoneConverter;

    public static class IntegerExtensions
    {
        /*
        public static char NumberToAlphabet(this int num)
        {
            return Convert.ToChar(num + 64);
        }
        */

        public static DateTime FromUnix(this long unixSeconds, string timeZone)
        {
            var epochTime = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
            var unixDateTime = epochTime.AddSeconds(unixSeconds);
            var convertedDateTime = unixDateTime.ToTimeZone(timeZone);
            return convertedDateTime;
        }

        public static DateTime ToTimeZone(this DateTime dateTime, string timeZone)
        {
            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            var date = TimeZoneInfo.ConvertTimeFromUtc(dateTime, tzi);
            return date;
        }
    }
}
