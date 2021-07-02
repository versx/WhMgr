namespace WhMgr.Services.Discord.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DiscordEmbedMessage : DiscordEmbedImage
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("color")]
        public int Color { get; set; }

        [JsonPropertyName("author")]
        public DiscordEmbedAuthor Author { get; set; }

        [JsonPropertyName("fields")]
        public List<DiscordField> Fields { get; set; } = new();

        [JsonPropertyName("footer")]
        public DiscordEmbedFooter Footer { get; set; }

        [JsonPropertyName("thumbnail")]
        public DiscordEmbedImage Thumbnail { get; set; }

        [JsonPropertyName("image")]
        public DiscordEmbedImage Image { get; set; }
    }
}