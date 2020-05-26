namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    public class FilterEggObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        [JsonProperty("onlyEx")]
        public bool OnlyEx { get; set; }

        [JsonProperty("team")]
        public PokemonTeam Team { get; set; }

        public FilterEggObject()
        {
            MinimumLevel = 1;
            MaximumLevel = 5;

            Team = PokemonTeam.All;
        }
    }
}