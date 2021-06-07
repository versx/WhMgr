namespace WhMgr.Services.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordEmbedImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}