namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// Url configuration class
    /// </summary>
    public class UrlConfig
    {
        /// <summary>
        /// Gets or sets the Pokemon image url
        /// </summary>
        [JsonProperty("pokemonImage")]
        public string PokemonImage { get; set; }

        /// <summary>
        /// Gets or sets the raid egg image url
        /// </summary>
        [JsonProperty("eggImage")]
        public string EggImage { get; set; }

        /// <summary>
        /// Gets or sets the field research quests image url
        /// </summary>
        [JsonProperty("questImage")]
        public string QuestImage { get; set; }

        /// <summary>
        /// Gets or sets the weather image url
        /// </summary>
        [JsonProperty("weatherImage")]
        public string WeatherImage { get; set; }

        /// <summary>
        /// Gets or sets the static map image url
        /// </summary>
        [JsonProperty("staticMap")]
        public string StaticMap { get; set; }
    }
}