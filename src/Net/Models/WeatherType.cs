namespace WhMgr.Net.Models
{
    /// <summary>
    /// S2Cell weather type
    /// </summary>
    public enum WeatherType
    {
        /// <summary>
        /// No weather set
        /// </summary>
        None = 0,

        /// <summary>
        /// Clear or sunny weather
        /// </summary>
        Clear,
        
        /// <summary>
        /// Rainy weather
        /// </summary>
        Rain,

        /// <summary>
        /// Partly cloudy weather
        /// </summary>
        PartlyCloudy,

        /// <summary>
        /// Cloudy skies weather
        /// </summary>
        Cloudy,

        /// <summary>
        /// Windy weather
        /// </summary>
        Windy,

        /// <summary>
        /// Snowy weather
        /// </summary>
        Snow,

        /// <summary>
        /// Foggy weather
        /// </summary>
        Fog

        //All = ushort.MaxValue
    }
}