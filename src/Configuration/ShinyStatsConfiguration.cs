namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class ShinyStatsConfiguration
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("clearMessages")]
        public bool ClearMessages { get; set; }

        [JsonProperty("channelId")]
        public ulong ChannelId { get; set; }
    }
}