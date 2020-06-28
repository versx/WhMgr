namespace WhMgr.Alarms.Alerts
{
    using Newtonsoft.Json;

    /// <summary>
    /// Discord alert message settings
    /// </summary>
    public class AlertMessageSettings
    {
        /// <summary>
        /// Discord message content.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Discord message icon (left side)
        /// </summary>
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        /// <summary>
        /// Discord message author icon avatar url
        /// </summary>
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Discord message title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Discord message title url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Discord author username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Discord message image (bottom preview)
        /// </summary>
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }
}