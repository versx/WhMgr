namespace WhMgr.Data.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    [Alias("pokemon_stats")]
    public class PokemonStats
    {
        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("pokemon_id")]
        public int PokemonId { get; set; }

        [Alias("count")]
        public long Count { get; set; }
    }
}