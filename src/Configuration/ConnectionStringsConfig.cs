namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class ConnectionStringsConfig
    {
        [JsonProperty("main")]
        public DatabaseConfig Main { get; set; }

        [JsonProperty("scanner")]
        public DatabaseConfig Scanner { get; set; }

        [JsonProperty("nests")]
        public DatabaseConfig Nests { get; set; }
    }
}