namespace WhMgr.Data.Models.Discord
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class DiscordEmbedMessage
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("author")]
        public DiscordEmbedAuthor Author { get; set; }

        [JsonProperty("fields")]
        public List<DiscordField> Fields { get; set; }

        [JsonProperty("footer")]
        public DiscordEmbedFooter Footer { get; set; }

        [JsonProperty("thumbnail")]
        public DiscordEmbedImage Thumbnail { get; set; }

        [JsonProperty("image")]
        public DiscordEmbedImage Image { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public DiscordEmbedMessage()
        {
            Fields = new List<DiscordField>();
        }
    }
}