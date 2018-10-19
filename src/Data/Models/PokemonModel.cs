namespace T.Data.Models
{
    using Newtonsoft.Json;

    public class PokemonModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}