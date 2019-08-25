namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterPokestopObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("lured")]
        public bool Lured { get; set; }

        [JsonProperty("invasions")]
        public bool Invasions { get; set; }
    }
}