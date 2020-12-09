namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Net.Models;

    [
        JsonObject("lures"),
        Alias("lures")
    ]
    public class LureSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("lure_type"),
            Alias("lure_type"),
            Required
        ]
        public PokestopLureType LureType { get; set; }

        [
            JsonProperty("city"),
            Alias("city"),
            Required
        ]
        public List<string> Areas { get; set; }

        public LureSubscription()
        {
            LureType = PokestopLureType.None;
            Areas = new List<string>();
        }
    }
}