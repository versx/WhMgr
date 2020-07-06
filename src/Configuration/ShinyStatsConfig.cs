namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// Shiny stats Pokemon configuration class
    /// </summary>
    public class ShinyStatsConfig
    {
        /// <summary>
        /// Gets or sets whether to enable shiny stats posting
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets whether to clear the previous shiny stats messages
        /// </summary>
        [JsonProperty("clearMessages")]
        public bool ClearMessages { get; set; }

        /// <summary>
        /// Gets or sets the channel ID to post the shiny stats to
        /// </summary>
        [JsonProperty("channelId")]
        public ulong ChannelId { get; set; }
    }
}