namespace WhMgr.Alarms.Alerts
{
    using Newtonsoft.Json;

    /// <summary>
    /// Discord alert message settings
    /// </summary>
    public class AlertMessageSettings
    {
        /// <summary>
        /// Gets or sets the Discord message content outside of the embed message. (above it)
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Discord message content within the embed message.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the Discord message icon url (left side)
        /// </summary>
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message author icon avatar url
        /// </summary>
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Discord message title url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Discord author username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Discord message image url (bottom preview)
        /// </summary>
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the Discord message footer text and icon url
        /// </summary>
        [JsonProperty("footer")]
        public AlertMessageFooter Footer { get; set; }
    }

    /// <summary>
    /// Discord alert message footer
    /// </summary>
    public class AlertMessageFooter
    {
        /// <summary>
        /// Gets or sets the Discord message footer text
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the Discord message footer icon url
        /// </summary>
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }
    }
}