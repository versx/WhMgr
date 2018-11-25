namespace WhMgr.Data.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    [Alias("raid_stats")]
    public class RaidStats
    {
        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("pokemon_id")]
        public int PokemonId { get; set; }

        [Alias("count")]
        public long Count { get; set; }

        [Alias("level")]
        public int Level { get; set; }
    }
}