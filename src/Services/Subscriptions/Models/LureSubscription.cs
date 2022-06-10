﻿namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
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

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Column("pokestop_name"),
            DefaultValue(null),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("lure_type"),
            Column("lure_type"),
            Required,
        ]
        public List<PokestopLureType> LureType { get; set; } = new();

        [
            JsonPropertyName("areas"),
            Column("areas"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
            DefaultValue(null),
        ]
        public string Location { get; set; }
    }
}