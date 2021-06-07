namespace WhMgr.Services.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordEmbedFooter
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; }
    }
}