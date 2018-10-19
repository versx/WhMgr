namespace T.Alarms.Filters.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    [JsonObject("filter")]
    public class FilterObject
    {
        //[JsonProperty("pokemon")]
        //public List<int> Pokemon { get; set; }

        //[JsonProperty("min_iv")]
        //public uint MinimumIV { get; set; }

        //[JsonProperty("max_iv")]
        //public uint MaximumIV { get; set; }

        //[JsonProperty("type")]
        //public FilterType FilterType { get; set; }

        //[JsonProperty("ignoreMissing")]
        //public bool IgnoreMissing { get; set; }

        [JsonProperty("pokemon")]
        public FilterPokemonObject Pokemon { get; set; }

        [JsonProperty("raids")]
        public FilterRaidObject Raids { get; set; }

        [JsonProperty("eggs")]
        public FilterEggObject Eggs { get; set; }

        public FilterObject()
        {
            //Pokemon = new List<int>();
        }
    }

    public class FilterPokemonObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

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

    public class FilterRaidObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("pokemon")]
        public List<int> Pokemon { get; set; }

        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }
    }

    public class FilterEggObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        public FilterEggObject()
        {
            MinimumLevel = 1;
            MaximumLevel = 5;
        }
    }
}