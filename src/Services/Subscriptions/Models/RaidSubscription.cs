namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    [Table("raids")]
    public class RaidSubscription : BaseSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            Required,
        ]
        public List<uint> PokemonId { get; set; } = new();

        /*
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
        */

        [
            JsonPropertyName("forms"),
            Column("forms"),
        ]
        public List<string> Forms { get; set; } = new();

        [
            JsonPropertyName("ex_eligible"),
            Column("ex_eligible"),
            DefaultValue(false),
        ]
        public bool IsExEligible { get; set; }

        [
            JsonPropertyName("areas"),
            Column("areas"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}