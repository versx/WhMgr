namespace WhMgr.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using WhMgr.Extensions;

    [Table("nests")]
    public class Nest
    {
        [
            Column("nest_id"),
            Key
        ]
        public string NestId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("pokemon_avg")]
        public int Average { get; set; }

        [Column("pokemon_count")]
        public int Count { get; set; }

        [Column("pokemon_id")]
        public int PokemonId { get; set; }

        [Column("lat")]
        public double Latitude { get; set; }

        [Column("lon")]
        public double Longitude { get; set; }

        [Column("updated")]
        public long LastUpdatedUnix { get; set; }

        [NotMapped]
        public DateTime LastUpdated => LastUpdatedUnix.FromUnix();
    }
}