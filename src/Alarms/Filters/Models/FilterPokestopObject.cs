namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

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

        [JsonProperty("lure_types")]
        public List<string> LureTypes { get; set; }

        /// <summary>
        /// Only report Team Rocket invasion Pokestops
        /// </summary>
        [JsonProperty("invasions")]
        public bool Invasions { get; set; }

        public FilterPokestopObject()
        {
            LureTypes = new List<string>();
        }
    }
}