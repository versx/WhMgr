namespace WhMgr.Configuration
{
    using System;

    /// <summary>
    /// This class holds a singleton instance of Config which can be swapped out (e.g. after a config reload) without everybody
    /// needing to update their references to the config itself.
    /// </summary>
    public class ConfigHolder
    {
        private readonly object _instanceMutex = new();

        private Config _instance;

        public ConfigHolder(Config instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Fired after the config instance was swapped for a new one
        /// </summary>
        public event Action Reloaded;

        /// <summary>
        /// Provides thread-safe access to the internal Config instance
        /// </summary>
        public Config Instance
        {
            get
            {
                Config value;

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