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
    }
}