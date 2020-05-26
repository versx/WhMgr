namespace WhMgr.Data.Models.Discord
{
    using Newtonsoft.Json;

    public class DiscordField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }
}