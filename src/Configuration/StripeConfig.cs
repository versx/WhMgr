namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class StripeConfig
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
    }
}