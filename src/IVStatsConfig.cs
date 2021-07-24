namespace WhMgr
{
    using System.Text.Json.Serialization;

    public class IVStatsConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("clearMessages")]
        public bool ClearMessages { get; set; }

        [JsonPropertyName("channelId")]
        public ulong ChannelId { get; set; }
    }
}