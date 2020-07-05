namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class TwilioConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("accountSid")]
        public string AccountSid { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("from")]
        public string FromNumber { get; set; }
    }
}