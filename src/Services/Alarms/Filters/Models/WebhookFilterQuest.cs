namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Field research quest filters
    /// </summary>
    public class WebhookFilterQuest
    {
        /// <summary>
        /// Enable field research quest filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Field research quest filter type
        /// </summary>
        [JsonPropertyName("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Field research quest reward keywords
        /// </summary>
        [JsonPropertyName("rewards")]
        public List<string> RewardKeywords { get; set; }

        /// <summary>
        /// Only shiny field research quest rewards
        /// </summary>
        [JsonPropertyName("isShiny")]
        public bool IsShiny { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterQuestObject"/> class.
        /// </summary>
        public WebhookFilterQuest()
        {
            RewardKeywords = new List<string>();
            FilterType = FilterType.Include;
        }
    }
}