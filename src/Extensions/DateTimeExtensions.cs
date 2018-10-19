namespace T.Extensions
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
            var start = startTime;
            var end = DateTime.Parse(endTime.ToLongTimeString());
            var remaining = TimeSpan.FromTicks(end.Ticks - start.Ticks);
            return remaining;
        }
    }
}