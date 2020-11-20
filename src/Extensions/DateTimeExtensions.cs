namespace WhMgr.Extensions
{
    using System;

    using GeoTimeZone;
    using TimeZoneConverter;

    public static class DateTimeExtensions
    {
        public static TimeSpan GetTimeRemaining(this DateTime startTime, DateTime endTime)
        {
            var remaining = TimeSpan.FromTicks(endTime.Ticks - startTime.Ticks);
            return remaining;
        }

        public static DateTime ConvertTimeFromCoordinates(this DateTime date, double lat, double lon)
        {
            var tzIana = TimeZoneLookup.GetTimeZone(lat, lon).Result;
#if Windows
            // Convert to Windows acceptable TimeZone
            tzIana = TZConvert.IanaToWindows(tzIana);
#endif
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzIana);
            var dt = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(dt, tzInfo);
            return convertedTime;
        }
    }
}