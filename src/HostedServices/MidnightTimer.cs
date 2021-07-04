//namespace DandTSoftware.Timers
namespace WhMgr.HostedServices
{
    using Microsoft.Win32;
    using System;
    using System.Timers;

    using WhMgr.Extensions;

    /// <summary>
    /// Midnight Timer Delegate for the event
    /// </summary>
    /// <param name="time"></param>
    public delegate void TimeReachedEventHandler(DateTime time, string timezone);

    /// <summary>
    /// Provides the means to detect when midnight is reached.
    /// </summary>
    public class MidnightTimer : IDisposable
    {
        #region Static Variables

        private bool _disposed;

        /// <summary>
        /// Internal Timer
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// How many Minutes after midnight are added to the timer
        /// </summary>
        private static int _minutesAfterMidnight;

        /// <summary>
        /// 
        /// </summary>
        private static string _timezone;

        /// <summary>
        /// Occurs whens midnight occurs, subscribe to this
        /// </summary>
        public event TimeReachedEventHandler TimeReached;

        #endregion

        #region Constructors

        /*
        /// <summary>
        /// Creates an instance of the Midnight Timer
        /// </summary>
        public MidnightTimer()
        {
        }
        */

        /// <summary>
        /// Creates an instance of the Midnight Timer, which will fire after a set number of minutes after midnight
        /// </summary>
        /// <param name="MinutesAfterMidnight">How many Minutes after midnight do we start the timer? between 0 and 59</param>
        public MidnightTimer(int minutesAfterMidnight, string timezone)// : this()
        {
            // Check if the supplied m is between 0 and 59 mins after midnight
            if (minutesAfterMidnight < 0 || minutesAfterMidnight > 59)
            {
                // if it is outside of this range, throw a exception
                throw new ArgumentException("Minutes after midnight is less than 0 or more than 60!");
            }

            // Set the internal value
            _minutesAfterMidnight = minutesAfterMidnight;
            _timezone = timezone;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the Timer to fire a certain amount of Minutes AFTER midnight, every night (based on server time).
        /// </summary>
        public void Start()
        {
            var now = DateTime.Now.ConvertTimeFromTimeZone(_timezone);

            // Subtract the current time, from midnigh (tomorrow).
            // This will return a value, which will be used to set the Timer interval
            var ts = GetMidnight(_minutesAfterMidnight, _timezone).Subtract(now);

            // We only want the Hours, Minuters and Seconds until midnight
            var tsMidnight = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);

            // Create the timer
            _timer = new Timer(tsMidnight.TotalMilliseconds);

            // Set the event handler
            _timer.Elapsed += OnTimerElapsed;

            // Start the timer
            _timer.Start();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            // sanity checking
            if (_timer != null)
            {
                // Stop the orginal timer
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
            }
        }

        /// <summary>
        /// Restarts the timer
        /// </summary>
        public void Restart()
        {
            // Stop the timer
            Stop();

            // (Re)Start
            Start();
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Standard Event/Delegate handler, if its not null, fire the event
        /// </summary>
        private void OnTimeReached(string timezone) =>
            TimeReached?.Invoke(GetMidnight(_minutesAfterMidnight, timezone), timezone);

        /// <summary>
        /// Executes when the timer has elasped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Stop the orginal timer
            Stop();

            // now raise a event that the timer has elapsed
            OnTimeReached(_timezone); // swapped order thanks to Jeremy

            // reset the timer
            Start();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Obtains a DateTime of Midngiht
        /// </summary>
        /// <param name="MinutesAfterMidnight">How many minuets after midnight to add?</param>
        /// <returns></returns>
        private DateTime GetMidnight(int minutesAfterMidnight)
        {
            // Lets work out the next occuring midnight
            // Add 1 day and use hours 0, min 0 and second 0 (remember this is 24 hour time)

            // Thanks to Yashar for this code/fix
            var tomorrow = DateTime.Now.AddDays(1);

            // Return a datetime for Tomorrow, but with how many minutes after midnight
            return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, minutesAfterMidnight, 0);
        }

        private DateTime GetMidnight(int minutesAfterMidnight, string timezone)
        {
            return GetMidnight(minutesAfterMidnight).ConvertTimeFromTimeZone(timezone);
        }

        #endregion

        #region Disposing

        /// <summary>
        /// Dispose of the timer (also stops the timer)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // free managed resources
                // Pass to Stop to unsubscribe the event handler of Windows System Time Changes
                Stop();
                _timer.Dispose();
            }

            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //}

            _disposed = true;
        }

        #endregion
    }
}