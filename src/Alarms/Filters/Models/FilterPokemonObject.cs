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

        [JsonProperty("min_cp")]
        public uint MinimumCP { get; set; }

        [JsonProperty("max_cp")]
        public uint MaximumCP { get; set; }

        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        //TODO: Filter by gender.
        //TODO: Filter by move?

        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }

        public FilterPokemonObject()
        {
            MinimumIV = 0;
            MaximumIV = 100;
            MinimumCP = 0;
            MaximumCP = 999999;
            MinimumLevel = 0;
            MaximumLevel = 100; //Support for when they increase level cap. :wink:
        }
    }
}