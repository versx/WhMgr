using System.Threading;

namespace WhMgr.Configuration
{
    /// <summary>
    /// This class holds a singleton instance of WhConfig which can be swapped out (e.g. after a config reload) without everybody
    /// needing to update their references to the config itself.
    /// </summary>
    public class WhConfigHolder
    {
        private readonly Mutex _instanceMutex;

        private WhConfig _instance;

        public WhConfigHolder(WhConfig instance)
        {
            _instanceMutex = new Mutex();
            _instance = instance;
        }

        /// <summary>
        /// Provides thread-safe access to the internal WhConfig instance
        /// </summary>
        public WhConfig Instance
        {
            get
            {
                WhConfig value;

                _instanceMutex.WaitOne();
                value = _instance;
                _instanceMutex.ReleaseMutex();

                return value;
            }
            set
            {
                _instanceMutex.WaitOne();
                _instance = value;
                _instanceMutex.ReleaseMutex();
            }
        }
    }
}
