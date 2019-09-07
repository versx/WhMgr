namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    public class FilterGymObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("underAttack")]
        public bool UnderAttack { get; set; }

        [JsonProperty("team")]
        public PokemonTeam Team { get; set; }
    }
}