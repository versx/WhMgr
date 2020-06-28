namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Weather filters
    /// </summary>
    public class FilterWeatherObject
    {
        /// <summary>
        /// Enable weather filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Filter by in-game weather type
        /// </summary>
        [JsonProperty("weatherType")]
        public bool WeatherType { get; set; }
    }
}