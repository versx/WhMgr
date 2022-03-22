namespace WhMgr.Configuration
{
    using System;
    using System.Text.Json.Serialization;

    public class PvpLeagueConfig
    {
        [JsonPropertyName("minCP")]
        public ushort MinimumCP { get; set; }

        [JsonPropertyName("maxCP")]
        public ushort MaximumCP { get; set; }

        [JsonPropertyName("minRank")]
        public ushort MinimumRank { get; set; }

        [JsonPropertyName("maxRank")]
        public ushort MaximumRank { get; set; }
    }
}