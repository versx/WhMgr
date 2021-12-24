namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class UrlShortenerConfig
    {
        /// <summary>
        /// Gets or sets a value determining whether the url shortener api is enabled or not
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the base API url for yourls.org
        /// </summary>
        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the url shortener signature
        /// </summary>
        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets the default response action, only 'shorturl' is currently supported
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; } = "shorturl";

        /// <summary>
        /// Gets or sets the default response format, only 'json' is currently supported
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = "json";
    }
}