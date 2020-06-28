namespace WhMgr.Alarms.Filters.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Field research quest filters
    /// </summary>
    public class FilterQuestObject
    {
        /// <summary>
        /// Enable field research quest filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Field research quest filter type
        /// </summary>
        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Field research quest reward keywords
        /// </summary>
        [JsonProperty("rewards")]
        public List<string> RewardKeywords { get; set; }

        /// <summary>
        /// Only shiny field research quest rewards
        /// </summary>
        [JsonProperty("isShiny")]
        public bool IsShiny { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterQuestObject"/> class.
        /// </summary>
        public FilterQuestObject()
        {
            RewardKeywords = new List<string>();
            FilterType = FilterType.Include;
        }
    }
}