﻿namespace WhMgr.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using WhMgr.Services.Webhook.Models;

    [Table("nests")]
    public class Nest : IWebhookPoint
    {
        [
            Column("nest_id"),
            Key,
        ]
        public long NestId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("pokemon_avg")]
        public double Average { get; set; }

        [Column("pokemon_count")]
        public double Count { get; set; }

        [Column("pokemon_id")]
        public uint PokemonId { get; set; }

        [Column("lat")]
        public double Latitude { get; set; }

        [Column("lon")]
        public double Longitude { get; set; }

        [Column("updated")]
        public ulong LastUpdated { get; set; }

        // TODO: Check if updated within the last 2 weeks before posting?
        [NotMapped]
        public bool IsExpired { get; set; }
    }
}