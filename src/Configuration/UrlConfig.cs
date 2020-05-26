namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class UrlConfig
    {
        [JsonProperty("pokemonImage")]
        public string PokemonImage { get; set; }

        [JsonProperty("eggImage")]
        public string EggImage { get; set; }

        [JsonProperty("questImage")]
        public string QuestImage { get; set; }

        [JsonProperty("staticMap")]
        public string StaticMap { get; set; }
    }
}