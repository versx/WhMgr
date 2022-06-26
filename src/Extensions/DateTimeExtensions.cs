namespace WhMgr.Extensions
{
    using System;
    using System.Linq;

    using GeoTimeZone;
    using TimeZoneConverter;

    using WhMgr.Services.Webhook.Models;

    public static class DateTimeExtensions
    {
        public static TimeSpan GetTimeRemaining(this DateTime startTime, DateTime endTime)
        {
            var remaining = TimeSpan.FromTicks(endTime.Ticks - startTime.Ticks);
            return remaining;
        }

        public static DateTime ConvertTimeFromCoordinates(this DateTime date, IWebhookPoint coord)
        {
            return ConvertTimeFromCoordinates(date, coord.Latitude, coord.Longitude);
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

        public static DateTime ConvertTimeFromTimeZone(this DateTime date, string tzIana)
        {
            var result = tzIana;
#if Windows
            // Check if timezone is Iana to prevent error when converting if already
            // in the expected timezone format on Windows.
            if (TZConvert.KnownIanaTimeZoneNames.Contains(tzIana))
            {
                // Convert to Windows acceptable TimeZone
                result = TZConvert.IanaToWindows(tzIana);
            }
#elif Linux || macOS
            if (TZConvert.KnownWindowsTimeZoneIds.Contains(tzIana))
            {
                result = TZConvert.WindowsToIana(tzIana);
            }
#endif
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(result);
            var dt = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(dt, tzInfo);
            return convertedTime;
        }

        /// <summary>
        /// Get Unix timestamp from current date time
        /// </summary>
        /// <param name="now">Date and time to get unix variation from</param>
        /// <returns>Returns Unix timestamp</returns>
        public static double GetUnixTimestamp(this DateTime now)
        {
            return Math.Round(now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public static DateTime FromUnix(this long unixSeconds)
        {
            var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var localDateTime = epochTime.AddSeconds(unixSeconds);//.ToLocalTime();

            return localDateTime;
        }

        public static DateTime FromUnix(this ulong unixSeconds)
        {
            return FromUnix((long)unixSeconds);
        }
    }
}