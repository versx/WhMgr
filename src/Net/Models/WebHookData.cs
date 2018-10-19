namespace T.Net.Models
{
    using Newtonsoft.Json;

    public class WebHookData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public dynamic Message { get; set; }
    }
}