namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Pokemon filters
    /// </summary>
    public class FilterPokestopObject
    {
        /// <summary>
        /// Enable pokestop filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Only report lured Pokestops
        /// </summary>
        [JsonProperty("lured")]
        public bool Lured { get; set; }

        /// <summary>
        /// Only report Team Rocket invasion Pokestops
        /// </summary>
        [JsonProperty("invasions")]
        public bool Invasions { get; set; }
    }
}