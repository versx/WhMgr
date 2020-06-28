namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Main filter object
    /// </summary>
    [JsonObject("filter")]
    public class FilterObject
    {
        /// <summary>
        /// Pokemon filters
        /// </summary>
        [JsonProperty("pokemon")]
        public FilterPokemonObject Pokemon { get; set; }

        /// <summary>
        /// Raid filters
        /// </summary>
        [JsonProperty("raids")]
        public FilterRaidObject Raids { get; set; }

        /// <summary>
        /// Raid egg filters
        /// </summary>
        [JsonProperty("eggs")]
        public FilterEggObject Eggs { get; set; }

        /// <summary>
        /// Field research quest filters
        /// </summary>
        [JsonProperty("quests")]
        public FilterQuestObject Quests { get; set; }

        /// <summary>
        /// Pokestop filters
        /// </summary>
        [JsonProperty("pokestops")]
        public FilterPokestopObject Pokestops { get; set; }

        /// <summary>
        /// Gym filters
        /// </summary>
        [JsonProperty("gyms")]
        public FilterGymObject Gyms { get; set; }

        /// <summary>
        /// Weather cell filters
        /// </summary>
        [JsonProperty("weather")]
        public FilterWeatherObject Weather { get; set; }
    }
}