namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// Url configuration class
    /// </summary>
    public class UrlConfig
    {
        /// <summary>
        /// Gets or sets the static map image url
        /// </summary>
        [JsonProperty("staticMap")]
        public string StaticMap { get; set; }

        /// <summary>
        /// Gets or sets the scanner map url
        /// </summary>
        [JsonProperty("scannerMap")]
        public string ScannerMap { get; set; }
    }
}