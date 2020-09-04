namespace WhMgr.Extensions
{
    using System;

    using GeoTimeZone;
    using TimeZoneConverter;

    public static class DateTimeExtensions
    {
        public static TimeSpan GetTimeRemaining(this DateTime endTime)
        {
            return GetTimeRemaining(DateTime.Now, endTime);
        }

        public static TimeSpan GetTimeRemaining(this DateTime startTime, DateTime endTime)
        {
            var remaining = TimeSpan.FromTicks(endTime.Ticks - startTime.Ticks);
            return remaining;
        }

        public static DateTime ConvertTime(this DateTime timeUtc, string timeZoneId = "Pacific Standard Time")
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
                Console.WriteLine("The date and time are {0} {1}.",
                                  cstTime,
                                  cstZone.IsDaylightSavingTime(cstTime) ?
                                          cstZone.DaylightName : cstZone.StandardName);
                return cstTime;
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("The registry does not define the Central Standard Time zone.");
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine("Registry data on the Central Standard Time zone has been corrupted.");
            }
            return timeUtc;
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