namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    [Table("gyms")]
    public class GymSubscription : BaseSubscription
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
            JsonPropertyName("name"),
            Column("name"),
        ]
        public string Name { get; set; }

        [
            JsonPropertyName("min_level"),
            Column("min_level"),
            DefaultValue(0),
        ]
        public ushort MinimumLevel { get; set; }

        [
            JsonPropertyName("max_level"),
            Column("max_level"),
            DefaultValue(8),
        ]
        public ushort MaximumLevel { get; set; }

        [
            JsonPropertyName("pokemon_ids"),
            Column("pokemon_ids"),
        ]
        public List<uint> PokemonIDs { get; set; } = new();

        [
            JsonPropertyName("ex_eligible"),
            Column("ex_eligible"),
            DefaultValue(false),
        ]
        public bool IsExEligible { get; set; }

        [
            JsonPropertyName("location"),
            Column("location"),
            DefaultValue(null),
        ]
        public string Location { get; set; }
    }
}