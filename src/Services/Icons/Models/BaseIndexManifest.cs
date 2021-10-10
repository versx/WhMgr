namespace WhMgr.Services.Icons.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class BaseIndexManifest
    {
        [JsonPropertyName("gym")]
        public HashSet<string> Gyms { get; set; } = new();

        [JsonPropertyName("invasion")]
        public HashSet<string> Invasions { get; set; } = new();

        [JsonPropertyName("misc")]
        public HashSet<string> Miscellaneous { get; set; } = new();

        [JsonPropertyName("nest")]
        public HashSet<string> Nests { get; set; } = new();

        [JsonPropertyName("pokemon")]
        public HashSet<string> Pokemon { get; set; } = new();

        [JsonPropertyName("pokestop")]
        public HashSet<string> Pokestops { get; set; } = new();

        [JsonPropertyName("raid")]
        public BaseIndexRaidManifest Raids { get; set; } = new();

        [JsonPropertyName("reward")]
        public Dictionary<string, List<string>> Rewards { get; set; } = new();

        [JsonPropertyName("team")]
        public HashSet<string> Teams { get; set; } = new();

        [JsonPropertyName("type")]
        public HashSet<string> Types { get; set; } = new();

        [JsonPropertyName("weather")]
        public HashSet<string> Weather { get; set; } = new();

        [JsonPropertyName("0"), JsonIgnore]
        public string IndexFile { get; set; } // home icons fix >.>

        [JsonPropertyName("1"), JsonIgnore]
        public string Overview { get; set; } // home/pmsf icons fix >.>
    }
}