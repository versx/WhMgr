namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    [Table("gyms")]
    public class GymSubscription : SubscriptionItem
    {
        [
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("FK_gym_subscriptions_subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("name"),
            Column("name"),
            //Unique,
        ]
        public string Name { get; set; }

        [
            JsonPropertyName("min_level"),
            Column("min_level"),
        ]
        public ushort MinimumLevel { get; set; }

        [
            JsonPropertyName("max_level"),
            Column("max_level"),
        ]
        public ushort MaximumLevel { get; set; }

        [
            JsonPropertyName("pokemon_ids"),
            Column("pokemon_ids"),
        ]
        public List<uint> PokemonIDs { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}