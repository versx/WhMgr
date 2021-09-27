namespace WhMgr.Services.Pvp
{
    using System.Text.Json.Serialization;

    public class PvpRank
    {
        [JsonPropertyName("cp")]
        public uint CP { get; set; }

        [JsonPropertyName("rank")]
        public ushort Rank { get; set; }

        [JsonPropertyName("pokemon")]
        public ushort Pokemon { get; set; }

        [JsonPropertyName("form")]
        public ushort Form { get; set; }

        [JsonPropertyName("evolution")]
        public ushort Evolution { get; set; }

        [JsonPropertyName("level")]
        public double Level { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }

        [JsonPropertyName("cap")]
        public ushort LevelCap { get; set; }

        [JsonIgnore]
        public bool IsCapped { get; set; }
    }
}