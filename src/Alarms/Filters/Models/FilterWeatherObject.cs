namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterWeatherObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("weatherType")]
        public bool Lured { get; set; }
    }
}