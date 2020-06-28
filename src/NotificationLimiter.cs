namespace WhMgr
{
    using System;

    /// <summary>
    /// Notification limiter class
    /// </summary>
    public class NotificationLimiter
    {
        /// <summary>
        /// Maximum amount of notifications per user per minute
        /// </summary>
        public const int MaxNotificationsPerMinute = 15;
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
        /// Checks if the current notification with rate limit the Discord user
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLimited()
        {
            TimeLeft = DateTime.Now.Subtract(_last);

            var sixtySeconds = TimeSpan.FromSeconds(ThresholdTimeout);
            var oneMinutePassed = TimeLeft >= sixtySeconds;
            if (oneMinutePassed)
            {
                Reset();
                _last = DateTime.Now;
            }

            if (Count >= MaxNotificationsPerMinute)
            {
                //Limited
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