namespace WhMgr.Extensions
{
    using System;

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
    }
}