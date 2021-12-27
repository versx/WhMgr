namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class DailyStatsConfig
    {
        [JsonPropertyName("shiny")]
        public StatsConfig ShinyStats { get; set; }

        [JsonPropertyName("iv")]
        public StatsConfig IVStats { get; set; }
    }
}