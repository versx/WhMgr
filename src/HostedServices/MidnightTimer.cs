//namespace DandTSoftware.Timers
namespace WhMgr.HostedServices
{
    using System;
    using System.Timers;

    using WhMgr.Extensions;

    /// <summary>
    /// Midnight timer event arguments
    /// </summary>
    public class TimeReachedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current midnight time
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// Gets the time zone <seealso cref="Time"/> is in
        /// </summary>
        public string TimeZone { get; }

        public TimeReachedEventArgs(DateTime time, string timeZone)
        {
            Time = time;
            TimeZone = timeZone;
        }
    }

    /// <summary>
    /// Provides the means to detect when midnight is reached.
    /// </summary>
    public class MidnightTimer : IDisposable
    {
        #region Variables

        private bool _disposed;
        private static Timer _timer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets how many minutes after midnight are added to delay the timer
        /// </summary>
        public int MinutesAfterMidnight { get; }

        /// <summary>
        /// Gets the time zone used to check if midnight
        /// </summary>
        public string TimeZone { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs whens midnight occurs, subscribe to this
        /// </summary>
        public event EventHandler<TimeReachedEventArgs> TimeReached;

        /// <summary>
        /// Standard Event/Delegate handler, if its not null, fire the event
        /// </summary>
        /// <param name="timeZone"></param>
        private void OnTimeReached(string timeZone)
        {
            var midnight = GetMidnight(MinutesAfterMidnight, timeZone);
            TimeReached?.Invoke(this, new TimeReachedEventArgs(midnight, timeZone));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the Midnight Timer, which will fire after a set number of minutes after midnight
        /// </summary>
        /// <param name="minutesAfterMidnight">How many Minutes after midnight do we start the timer? between 0 and 59</param>
        /// <param name="timeZone">Time zone to use when checking if midnight</param>
        public MidnightTimer(int minutesAfterMidnight, string timeZone)
        {
            // Check if the supplied minutes is between 0 and 59 mins after midnight
            if (minutesAfterMidnight < 0 || minutesAfterMidnight > 59)
            {
                // If it is outside of this range, throw an exception
                throw new ArgumentException("Minutes after midnight is less than 0 or more than 60!");
            }

            // Set the properties value
            MinutesAfterMidnight = minutesAfterMidnight;
            TimeZone = timeZone;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the Timer to fire a certain amount of Minutes AFTER midnight, every night (based on server time).
        /// </summary>
        public void Start()
        {
            var now = DateTime.Now.ConvertTimeFromTimeZone(TimeZone);

            // Subtract the current time, from midnigh (tomorrow).
            // This will return a value, which will be used to set the Timer interval
            var midnight = GetMidnight(MinutesAfterMidnight, TimeZone);
            var ts = midnight.Subtract(now);

            // We only want the Hours, Minuters and Seconds until midnight
            var tsMidnight = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);

            // Create the timer
            if (tsMidnight.TotalMilliseconds > 0)
            {
                _timer = new Timer(tsMidnight.TotalMilliseconds);

                // Set the event handler
                _timer.Elapsed += OnTimerElapsed;

                // Start the timer
                _timer.Start();
            }
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            // Sanity checking
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

            // (Re)Start the timer
            Start();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Obtains a <seealso cref="DateTime"/> of midnight in the local time zone
        /// </summary>
        /// <param name="minutesAfterMidnight">Number of minutes after midnight to delay the timer by</param>
        /// <returns>Returns the midnight date and time in the local time zone</returns>
        private static DateTime GetMidnight(int minutesAfterMidnight)
        {
            // Lets work out the next occuring midnight
            // Add 1 day and use hours 0, min 0 and second 0 (remember this is 24 hour time)

            // Thanks to Yashar for this code/fix
            var tomorrow = DateTime.Now.AddDays(1);

            // Return a datetime for Tomorrow, but with how many minutes after midnight
            return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, minutesAfterMidnight, 0);
        }

        /// <summary>
        /// Obtains a <seealso cref="DateTime"/> of midnight in the specified time zone
        /// </summary>
        /// <param name="minutesAfterMidnight">Number of minutes after midnight to delay the timer by</param>
        /// <param name="timeZone">The time zone to convert the time to in order to check if it's midnight</param>
        /// <returns>Returns the midnight date and time in the specified time zone</returns>
        private static DateTime GetMidnight(int minutesAfterMidnight, string timeZone)
        {
            return GetMidnight(minutesAfterMidnight).ConvertTimeFromTimeZone(timeZone);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Executes when the timer has elasped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Stop the orginal timer
            Stop();

            // now raise an event that the timer has elapsed
            OnTimeReached(TimeZone); // swapped order thanks to Jeremy

            // reset the timer
            Start();
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