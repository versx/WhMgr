namespace WhMgr.Services.Webhook.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// PVP Pokemon rank class.
    /// </summary>
    public sealed class PvpRankData
    {
        [JsonPropertyName("rank")]
        public int? Rank { get; set; }

        [JsonPropertyName("percentage")]
        public double? Percentage { get; set; }

        [JsonPropertyName("pokemon")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("form")]
        public int FormId { get; set; }

        [JsonPropertyName("level")]
        public double? Level { get; set; }

        [JsonPropertyName("cp")]
        public int? CP { get; set; }
    }
}