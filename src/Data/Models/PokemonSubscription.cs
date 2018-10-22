namespace WhMgr.Data.Models
{
    using Newtonsoft.Json;

    public class PokemonSubscription
    {
        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("min_cp")]
        public int MinimumCP { get; set; }

        [JsonProperty("miv_iv")]
        public int MinimumIV { get; set; }

        [JsonProperty("min_lvl")]
        public int MinimumLevel { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        //public string City { get; set; }

        public PokemonSubscription()
        {
            Gender = "*";
        }
    }
}