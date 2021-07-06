namespace WhMgr.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pokemon_iv_stats")]
    public class PokemonStatsIV
    {
        [
            Column("date", TypeName = "date"),
            Key,
        ]
        public DateTime Date { get; set; }

        [
            Column("pokemon_id"),
            Key,
        ]
        public uint PokemonId { get; set; }

        [Column("count")]
        public ulong Count { get; set; }
    }
}