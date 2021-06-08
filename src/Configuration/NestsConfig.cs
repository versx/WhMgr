namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class NestsConfig
    {
        /// <summary>
        /// Gets or sets the nests channel ID to report nests
        /// </summary>
        [JsonPropertyName("channelId")]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the minimum nest spawns per hour to limit nest posts by
        /// </summary>
        [JsonPropertyName("minimumPerHour")]
        public int MinimumPerHour { get; set; } = 1;
    }
}