namespace WhMgr.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pokemon_iv_stats")]
    internal class PokemonStatsIV
    {
        [Column("date")]
        public DateTime Date { get; set; }

        [Column("pokemon_id")]
        public uint PokemonId { get; set; }

        [Column("count")]
        public ulong Count { get; set; }
    }
}