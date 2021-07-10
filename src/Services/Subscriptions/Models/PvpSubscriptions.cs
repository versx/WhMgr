namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    [Table("pvp")]
    public class PvpSubscription : BaseSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription))
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            Required,
        ]
        public List<uint> PokemonId { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public List<string> Forms => FormsString?.Split(',').ToList();

        [
            JsonPropertyName("form"),
            Column("form"),
        ]
        public string FormsString { get; set; }

        [
            JsonPropertyName("league"),
            Column("league"),
            //Required,
        ]
        public PvpLeague League { get; set; }

        [
            JsonPropertyName("min_rank"),
            Column("min_rank"),
            DefaultValue(25),
        ]
        public int MinimumRank { get; set; }

        [
            JsonPropertyName("min_percent"),
            Column("min_percent"),
            DefaultValue(90.0),
        ]
        public double MinimumPercent { get; set; }

        [
            JsonPropertyName("city"),
            Column("city"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }

        public PvpSubscription()
        {
            FormsString = null;
            League = PvpLeague.Great;
            MinimumRank = 25;
            MinimumPercent = 95;
        }
    }
}