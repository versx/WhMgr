namespace T.Data.Models
{
    using Newtonsoft.Json;

    public class RaidSubscription
    {
        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }
    }
}