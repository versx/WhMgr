namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class ConnectionStringsConfiguration
    {
        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("scanner")]
        public string Scanner { get; set; }

        [JsonProperty("nests")]
        public string Nests { get; set; }
    }
}