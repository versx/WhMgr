namespace WhMgr.Configuration
{
    using System;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Icons.Models;

    public class IconStyleConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonIgnore]
        //public HashSet<string> IndexList { get; set; } = new();
        public dynamic IndexList { get; set; }

        [JsonIgnore]
        public BaseIndexManifest BaseIndexList { get; set; } = new();
    }
}