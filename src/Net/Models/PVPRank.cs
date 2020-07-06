namespace WhMgr.Net.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// PVP Pokemon rank class.
    /// </summary>
    public sealed class PVPRank
    {
        [JsonProperty("rank")]
        public int? Rank { get; set; }

        [JsonProperty("percentage")]
        public double? Percentage { get; set; }

        [JsonProperty("pokemon")]
        public int PokemonId { get; set; }

        [JsonProperty("form")]
        public int FormId { get; set; }

        [JsonProperty("level")]
        public ushort? Level { get; set; }

        [JsonProperty("cp")]
        public int? CP { get; set; }
    }
}