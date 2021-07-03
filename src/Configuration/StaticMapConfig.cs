namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class StaticMapConfig
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("template")]
        public string TemplateName { get; set; }
    }
}