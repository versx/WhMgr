namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    using WhMgr.Services.StaticMap;

    public class StaticMapConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public StaticMapTemplateType Type { get; set; } = StaticMapTemplateType.StaticMap;

        [JsonPropertyName("includeGyms")]
        public bool IncludeNearbyGyms { get; set; }

        [JsonPropertyName("includePokestops")]
        public bool IncludeNearbyPokestops { get; set; }

        [JsonPropertyName("pregenerate")]
        public bool Pregenerate { get; set; } = true;
    }
}