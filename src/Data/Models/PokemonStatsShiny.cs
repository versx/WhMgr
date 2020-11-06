namespace WhMgr.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pokemon_shiny_stats")]
    internal class PokemonStatsShiny
    {
        [
            Column("date"),
            Key
        ]
        public DateTime Date { get; set; }

        [Column("pokemon_id")]
        public uint PokemonId { get; set; }

        [Column("count")]
        public ulong Count { get; set; }
    }
}