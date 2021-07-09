namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class StaticMapConfig
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("template")]
        public string TemplateName { get; set; }

        [JsonPropertyName("includeGyms")]
        public bool IncludeNearbyGyms { get; set; }

        [JsonPropertyName("includePokestops")]
        public bool IncludeNearbyPokestops { get; set; }
    }
}