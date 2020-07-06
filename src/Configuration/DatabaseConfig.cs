namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// MySQL database configuration class.
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// MySQL host address
        /// </summary>
        [JsonProperty("host")]
        public string Host { get; set; }

        /// <summary>
        /// MySQL listening port
        /// </summary>
        [JsonProperty("port")]
        public ushort Port { get; set; }

        /// <summary>
        /// MySQL username
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// MySQL password
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// MySQL database name
        /// </summary>
        [JsonProperty("database")]
        public string Database { get; set; }

        /// <summary>
        /// Returns the string representation of the <see cref="DatabaseConfig"/> class.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Uid={Username};Password={Password};Server={Host};Port={Port};Database={Database};old guids=true;";
        }
    }
}