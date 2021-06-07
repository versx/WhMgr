namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Main filter object
    /// </summary>
    public class WebhookFilter
    {
        /// <summary>
        /// Pokemon filters
        /// </summary>
        [JsonPropertyName("pokemon")]
        public WebhookFilterPokemon Pokemon { get; set; }

        /// <summary>
        /// Raid filters
        /// </summary>
        [JsonPropertyName("raids")]
        public WebhookFilterRaid Raids { get; set; }

        /// <summary>
        /// Raid egg filters
        /// </summary>
        [JsonPropertyName("eggs")]
        public WebhookFilterEgg Eggs { get; set; }

        /// <summary>
        /// Field research quest filters
        /// </summary>
        [JsonPropertyName("quests")]
        public WebhookFilterQuest Quests { get; set; }

        /// <summary>
        /// Pokestop filters
        /// </summary>
        [JsonPropertyName("pokestops")]
        public WebhookFilterPokestop Pokestops { get; set; }

        /// <summary>
        /// Gym filters
        /// </summary>
        [JsonPropertyName("gyms")]
        public WebhookFilterGym Gyms { get; set; }

        /// <summary>
        /// Weather cell filters
        /// </summary>
        [JsonPropertyName("weather")]
        public WebhookFilterWeather Weather { get; set; }
    }
}