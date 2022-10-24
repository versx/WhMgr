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
            // Convert to Windows standard time zone i.e. America/Los_Angeles -> Pacific Standard Time
            tzIana = TZConvert.IanaToWindows(tzIana);
#endif
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzIana);
            var dt = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(dt, tzInfo);
            return convertedTime;
        }

        public static DateTime ConvertTimeFromTimeZone(this DateTime date, string tzIana)
        {
            var result = ConvertTimeZone(tzIana);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(result);
            var dt = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(dt, tzInfo);
            return convertedTime;
        }

        public static string ConvertTimeZone(this string tzIana)
        {
            var result = tzIana;
            // Check if we were passed a Windows standard time zone, if so convert it to Iana
            // standard. Below will trigger with the MasterFileDownloaderHostedService class
            if (TZConvert.KnownWindowsTimeZoneIds.Contains(result))
            {
                // Converts to Iana standard time zone i.e. Pacific Standard Time -> America/Los_Angeles
                result = TZConvert.WindowsToIana(result);
            }
            return result;
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