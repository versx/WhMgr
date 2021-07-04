namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    [Table("lures")]
    public class LureSubscription : BaseSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Column("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("lure_type"),
            Column("lure_type"),
            //Required,
        ]
        public PokestopLureType LureType { get; set; } = PokestopLureType.None;

        [
            JsonPropertyName("city"),
            Column("city"),
            //Required,
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}