namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// MySQL connection strings configuration class
    /// </summary>
    public class ConnectionStringsConfig
    {
        /// <summary>
        /// Gets or sets the main database options for subscriptions
        /// </summary>
        [JsonPropertyName("main")]
        public DatabaseConfig Main { get; set; }

        /// <summary>
        /// Gets or sets the scanner database options
        /// </summary>
        [JsonPropertyName("scanner")]
        public DatabaseConfig Scanner { get; set; }

        /// <summary>
        /// Gets or sets the nests database options
        /// </summary>
        [JsonPropertyName("nests")]
        public DatabaseConfig Nests { get; set; }
    }
}