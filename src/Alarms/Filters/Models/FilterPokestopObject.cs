namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    public class FilterPokestopObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}