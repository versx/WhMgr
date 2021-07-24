namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class DailyStatsConfig
    {
        [JsonPropertyName("shiny")]
        public ShinyStatsConfig ShinyStats { get; set; }

        [JsonPropertyName("iv")]
        public IVStatsConfig IVStats { get; set; }
    }
}