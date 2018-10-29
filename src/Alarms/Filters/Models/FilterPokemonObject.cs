namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class FilterPokemonObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        //TODO: Allow pokemon names and ids for pokemon filter.
        [JsonProperty("pokemon")]
        public List<int> Pokemon { get; set; }

        [JsonProperty("min_iv")]
        public uint MinimumIV { get; set; }

        [JsonProperty("max_iv")]
        public uint MaximumIV { get; set; }

        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }

        public FilterPokemonObject()
        {
            MinimumIV = 0;
            MaximumIV = 100;
        }
    }
}