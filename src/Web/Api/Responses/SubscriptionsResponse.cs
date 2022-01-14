namespace WhMgr.Web.Api.Responses
{
    using System.Text.Json.Serialization;

    public class SubscriptionsResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}