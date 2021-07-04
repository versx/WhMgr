namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    /// <summary>
    /// Weather filters
    /// </summary>
    public class WebhookFilterWeather
    {
        /// <summary>
        /// Enable weather filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Filter by in-game weather type
        /// </summary>
        [JsonPropertyName("types")]
        public List<WeatherCondition> WeatherTypes { get; set; } = new();
    }
}