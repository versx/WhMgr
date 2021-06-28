namespace WhMgr.Services
{
    using System;

    /// <summary>
    /// Notification limiter class
    /// </summary>
    public class NotificationLimiter
    {
        public const int ThresholdTimeout = 60;

        //private readonly DateTime _start;
        private DateTime _last;

        /// <summary>
        /// Gets the current notification count within 60 seconds
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the time left before rate limit is lifted
        /// </summary>
        public TimeSpan TimeLeft { get; private set; }

        /// <summary>
        /// Instantiates a new <see cref="NotificationLimiter"/> class
        /// </summary>
        public NotificationLimiter()
        {
            //_start = DateTime.Now;
            _last = DateTime.Now;

            Count = 0;
            TimeLeft = TimeSpan.MinValue;
        }

        /// <summary>
        /// Checks if the current notification will rate limit the Discord user
        /// </summary>
        /// <param name="maxNotificationsPerMinute">Maximum amount of notifications a user can receive per minute</param>
        /// <returns></returns>
        public virtual bool IsLimited(int maxNotificationsPerMinute = 15)
        {
            TimeLeft = DateTime.Now.Subtract(_last);

            var sixtySeconds = TimeSpan.FromSeconds(ThresholdTimeout);
            var oneMinutePassed = TimeLeft >= sixtySeconds;
            if (oneMinutePassed)
            {
                Reset();
                _last = DateTime.Now;
            }

            if (Count >= maxNotificationsPerMinute)
            {
                // Rate Limited
                return true;
            }

            Count++;
            return false;
        }

        /// <summary>
        /// Resets the rate limit notification count
        /// </summary>
        public virtual void Reset()
        {
            Count = 0;
        }
    }
}