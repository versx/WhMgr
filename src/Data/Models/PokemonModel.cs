namespace T.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class PokemonModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("spawn_rate")]
        public string SpawnRate { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("types")]
        public List<PokemonType> Types { get; set; }

        [JsonProperty("base_stats")]
        public BaseStats BaseStats { get ;set; }
    }

    public class BaseStats
    {
        [JsonProperty("attack")]
        public int Attack { get; set; }

        [JsonProperty("defense")]
        public int Defense { get; set; }

        [JsonProperty("stamina")]
        public int Stamina { get; set; }

        [JsonProperty("legendary")]
        public bool Legendary { get; set; }

        [JsonProperty("generation")]
        public int Generation { get; set; }
    }

    public class PokemonType
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}