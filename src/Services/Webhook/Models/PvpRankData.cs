namespace WhMgr.Services.Webhook.Models
{
    using System.Text.Json.Serialization;

    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;

    /// <summary>
    /// PVP Pokemon rank class.
    /// </summary>
    public sealed class PvpRankData
    {
        [JsonPropertyName("rank")]
        public int? Rank { get; set; }

        [JsonPropertyName("dense_rank")]
        public ushort DenseRank { get; set; }

        [JsonPropertyName("ordinal_rank")]
        public ushort OrdinalRank { get; set; }

        [JsonPropertyName("competition_rank")]
        public ushort CompetitionRank { get; set; }

        [JsonPropertyName("percentage")]
        public double? Percentage { get; set; }

        [JsonPropertyName("pokemon")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("form")]
        public uint FormId { get; set; }

        [JsonPropertyName("level")]
        public double? Level { get; set; }

        [JsonPropertyName("gender")]
        public Gender Gender { get; set; }

        [JsonPropertyName("cp")]
        public int? CP { get; set; }
    }
}