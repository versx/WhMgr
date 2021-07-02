namespace WhMgr.Services.Alarms.Embeds
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Discord alert message settings
    /// </summary>
    public class EmbedMessageSettings
    {
        /// <summary>
        /// Gets or sets the Discord message content within the embed message.
        /// </summary>
        [JsonPropertyName("content")]
        public List<string> ContentList { get; set; }

        [JsonIgnore]
        public string Content => string.Join("\n", ContentList);

        /// <summary>
        /// Gets or sets the Discord message icon url (left side)
        /// </summary>
        [JsonPropertyName("iconUrl")]
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message author icon avatar url
        /// </summary>
        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Discord message title url
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Discord author username
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Discord message image url (bottom preview)
        /// </summary>
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message footer text and icon url
        /// </summary>
        [JsonPropertyName("footer")]
        public EmbedMessageFooter Footer { get; set; }
    }
}