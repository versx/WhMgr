namespace WhMgr.Services.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("inline")]
        public bool Inline { get; set; }
    }
}