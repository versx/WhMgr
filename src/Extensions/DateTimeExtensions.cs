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
            tzIana = ConvertIanaToWindowsTimeZone(tzIana);
#endif
            return GetConvertedDateTime(date, tzIana);
        }

        public static DateTime ConvertTimeFromTimeZone(this DateTime date, string timezone)
        {
            var result = ConvertIanaToWindowsTimeZone(timezone);
            return GetConvertedDateTime(date, result);
        }

        public static TimeZoneInfo GetTimeZoneInfoFromName(this string timezone, bool createUnknownTimeZone = false)
        {
            try
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                return tzInfo;
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"Failed to find timezone '{timezone}' on system, creating custom timezone using UTC offset or local timezone as fallback.");
            }

            return createUnknownTimeZone
                ? timezone.CreateCustomTimeZone()
                : TimeZoneInfo.Local;
        }

        public static TimeZoneInfo CreateCustomTimeZone(this string timezone, short offsetFromUtcH = 0, string? displayName = null, string? standardName = null)
        {
            var tzInfo = TimeZoneInfo.CreateCustomTimeZone(
                timezone,
                TimeSpan.FromHours(offsetFromUtcH),
                displayName ?? timezone,
                standardName ?? timezone
            );
            return tzInfo;
        }

        public static string ConvertIanaToWindowsTimeZone(this string timezone)
        {
            var result = timezone;
            // Check if we were passed a Windows standard time zone, if so convert it to Iana
            // standard. Below will trigger with the MasterFileDownloaderHostedService class
            if (TZConvert.KnownIanaTimeZoneNames.Contains(result))
            {
                // Converts Iana standard time zone to Windows time zone
                // i.e. America/Los_Angeles -> Pacific Standard Time
                result = TZConvert.IanaToWindows(result);
            }
            return result;
        }

        private static DateTime GetConvertedDateTime(DateTime localDate, string timezone)
        {
            var tzInfo = timezone.GetTimeZoneInfoFromName(createUnknownTimeZone: true);
            var dt = DateTime.SpecifyKind(localDate, DateTimeKind.Utc);
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