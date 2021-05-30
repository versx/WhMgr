namespace WhMgr.Data.Subscriptions.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    [
        JsonObject("gyms"),
        Alias("gyms"),
    ]
    public class GymSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject)),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("name"),
            Alias("name"),
            Unique,
        ]
        public string Name { get; set; }

        [
            JsonProperty("min_level"),
            Alias("min_level"),
        ]
        public ushort MinimumLevel { get; set; }

        [
            JsonProperty("max_level"),
            Alias("max_level"),
        ]
        public ushort MaximumLevel { get; set; }

        [
            JsonProperty("pokemon_ids"),
            Alias("pokemon_ids"),
        ]
        public List<uint> PokemonIDs { get; set; }

        public GymSubscription()
        {
            PokemonIDs = new List<uint>();
        }
    }
}