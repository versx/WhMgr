namespace T.Extensions
{
    using System;

    public static class IntegerExtensions
    {
        public static char NumberToAlphabet(this int num, bool caps = false)
        {
            return Convert.ToChar(num + (caps ? 64 : 96));
        }

        public static DateTime FromUnix(this long unixSeconds)
        {
            var now = DateTime.Now;
            var timeSpan = TimeSpan.FromSeconds(unixSeconds);
            var localDateTime = new DateTime(timeSpan.Ticks).ToLocalTime();

            //return localDateTime.AddHours(Convert.ToInt32(localDateTime.IsDaylightSavingTime()));
            //return localDateTime.AddHours(1);
            //return localDateTime;
            return new DateTime(now.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second, DateTimeKind.Local);
        }
    }
}