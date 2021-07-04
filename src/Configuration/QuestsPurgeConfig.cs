namespace WhMgr.Configuration
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class QuestsPurgeConfig
    {
        /// <summary>
        /// Gets or sets whether to prune previous field research quest channels 
        /// at midnight
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a list of field research quest channel ID(s) to reset
        /// </summary>
        [JsonPropertyName("channels")]
        public Dictionary<string, List<ulong>> ChannelIds { get; set; } = new();
    }
}