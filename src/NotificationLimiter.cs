namespace WhMgr
{
    using System;

    public class NotificationLimiter
    {
        public const int MaxNotificationsPerMinute = 15;
        public const int ThresholdTimeout = 60;

        //private readonly DateTime _start;
        private DateTime _last;

        public int Count { get; private set; }

        public TimeSpan TimeLeft { get; private set; }

        public NotificationLimiter()
        {
            //_start = DateTime.Now;
            _last = DateTime.Now;

            Count = 0;
            TimeLeft = TimeSpan.MinValue;
        }

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

        public virtual void Reset()
        {
            Count = 0;
        }
    }
}