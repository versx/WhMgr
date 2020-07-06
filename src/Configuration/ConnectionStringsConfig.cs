namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// MySQL connection strings configuration class
    /// </summary>
    public class ConnectionStringsConfig
    {
        /// <summary>
        /// Gets or sets the main database options for subscriptions
        /// </summary>
        [JsonProperty("main")]
        public DatabaseConfig Main { get; set; }

        /// <summary>
        /// Gets or sets the scanner database options
        /// </summary>
        [JsonProperty("scanner")]
        public DatabaseConfig Scanner { get; set; }

        /// <summary>
        /// Gets or sets the nests database options
        /// </summary>
        [JsonProperty("nests")]
        public DatabaseConfig Nests { get; set; }
    }
}