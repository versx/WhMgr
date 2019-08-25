namespace WhMgr.Data.Models
{
    using Newtonsoft.Json;

    public class PokemonPvP
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("level")]
        public double Level { get; set; }

        [JsonProperty("iv")]
        public Stats IVs { get; set; }

        [JsonProperty("cp")]
        public int CP { get; set; }

        [JsonProperty("atk")]
        public double Attack { get; set; }

        [JsonProperty("def")]
        public double Defense { get; set; }

        [JsonProperty("sta")]
        public int Stamina { get; set; }

        [JsonProperty("stat_product")]
        public long StatProduct { get; set; }

        [JsonProperty("max_stat")]
        public double MaxStat { get; set; }

        public PokemonPvP()
        {
            IVs = new Stats();
        }
    }

    public class Stats
    {
        [JsonProperty("atk")]
        public int Attack { get; set; }

        [JsonProperty("def")]
        public int Defense { get; set; }

        [JsonProperty("sta")]
        public int Stamina { get; set; }
    }
}
