namespace WhMgr.Services.Webhook.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    /// <summary>
    /// PVP Pokemon rank class.
    /// </summary>
    public sealed class PvpRankData
    {
        [
            JsonPropertyName("rank"),
            Column("rank"),
        ]
        public int? Rank { get; set; }

        [
            JsonPropertyName("percentage"),
            Column("percentage"),
        ]
        public double? Percentage { get; set; }

        [
            JsonPropertyName("pokemon"),
            Column("pokemon"),
        ]
        public uint PokemonId { get; set; }

        [
            JsonPropertyName("form"),
            Column("form"),
        ]
        public uint FormId { get; set; }

        [
            JsonPropertyName("level"),
            Column("level"),
        ]
        public double? Level { get; set; }

        [
            JsonPropertyName("cp"),
            Column("cp"),
        ]
        public int? CP { get; set; }

        [
            JsonPropertyName("pokemon_name"),
            NotMapped,
        ]
        public string PokemonName { get; set; }
    }
}