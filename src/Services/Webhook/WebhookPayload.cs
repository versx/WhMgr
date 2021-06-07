namespace WhMgr.Services.Webhook
{
    using System.Text.Json.Serialization;

    public class WebhookPayload : IWebhookPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("message")]
        public dynamic Message { get; set; }
    }
}