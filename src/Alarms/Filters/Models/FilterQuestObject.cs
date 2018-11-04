namespace WhMgr.Alarms.Filters.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class FilterQuestObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        [JsonProperty("rewards")]
        public List<string> RewardKeywords { get; set; }

        public FilterQuestObject()
        {
            RewardKeywords = new List<string>();
            FilterType = FilterType.Include;
        }
    }
}