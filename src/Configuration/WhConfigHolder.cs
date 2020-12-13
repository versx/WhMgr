using System;

namespace WhMgr.Configuration
{
    /// <summary>
    /// This class holds a singleton instance of WhConfig which can be swapped out (e.g. after a config reload) without everybody
    /// needing to update their references to the config itself.
    /// </summary>
    public class WhConfigHolder
    {
        private readonly object _instanceMutex = new object();

        private WhConfig _instance;

        public WhConfigHolder(WhConfig instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Fired after the config instance was swapped for a new one
        /// </summary>
        public event Action Reloaded;

        /// <summary>
        /// Provides thread-safe access to the internal WhConfig instance
        /// </summary>
        public WhConfig Instance
        {
            get
            {
                WhConfig value;

                lock (_instanceMutex)
                    value = _instance;

                return value;
            }
            set
            {
                lock (_instanceMutex)
                    _instance = value;

                Reloaded?.Invoke();
            }
        }
    }
}
