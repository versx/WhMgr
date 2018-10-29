namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class FilterRaidObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        //TODO: Allow pokemon names and ids for raid filter.
        [JsonProperty("pokemon")]
        public List<int> Pokemon { get; set; }

        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }
    }
}