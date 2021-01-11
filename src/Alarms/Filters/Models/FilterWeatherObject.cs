namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

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
        [JsonProperty("types")]
        public List<WeatherCondition> WeatherTypes { get; set; }
    }
}