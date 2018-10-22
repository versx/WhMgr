namespace WhMgr.Net.Models.Providers.RocketMap
{
    using Newtonsoft.Json;

    public class RocketMapProviderPokemon : IMapProviderPokemon
    {
        [JsonProperty("pokemon_id")]       
        public int PokemonId { get; set; }
    }
}