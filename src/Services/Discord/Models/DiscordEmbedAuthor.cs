namespace WhMgr.Services.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordEmbedAuthor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; }
    }
}