namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    [
        JsonObject("invasions"),
        Alias("invasions")
    ]
    public class InvasionSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("reward_pokemon_id"),
            Alias("reward_pokemon_id"),
            Required
        ]
        public int RewardPokemonId { get; set; }

        [
            JsonProperty("city"),
            Alias("city"), 
            Required
        ]
        public List<string> Areas { get; set; }

        public InvasionSubscription()
        {
            Areas = new List<string>();
        }
    }
}