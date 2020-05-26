namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class DatabaseConfig
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("database")]
        public string Database { get; set; }

        public override string ToString()
        {
            return $"Uid={Username};Password={Password};Server={Host};Port={Port};Database={Database};";
        }
    }
}