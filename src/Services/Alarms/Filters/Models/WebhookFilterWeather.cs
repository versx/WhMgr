namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    //using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

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

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeatherCondition
    {
        None = 0,
        Clear,
        Rainy,
        PartlyCloudy,
        Overcast,
        Windy,
        Snow,
        Fog,
    }
}