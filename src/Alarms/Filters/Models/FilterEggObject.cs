namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterEggObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        public FilterEggObject()
        {
            MinimumLevel = 1;
            MaximumLevel = 5;
        }
    }
}