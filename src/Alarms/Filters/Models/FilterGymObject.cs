namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterGymObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}