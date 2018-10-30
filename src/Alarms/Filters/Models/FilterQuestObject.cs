namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterQuestObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}