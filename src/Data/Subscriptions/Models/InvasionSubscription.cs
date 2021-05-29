namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using ServiceStack.DataAnnotations;

    [
        //JsonPropertyName("invasions"),
        Alias("invasions"),
    ]
    public class InvasionSubscription : SubscriptionItem
    {
        [
            JsonPropertyName("subscription_id"),
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject)),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Alias("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("grunt_type"),
            Alias("grunt_type"),
        ]
        public InvasionCharacter InvasionType { get; set; }

        [
            JsonPropertyName("reward_pokemon_id"),
            Alias("reward_pokemon_id"),
        ]
        public int RewardPokemonId { get; set; }

        [
            JsonPropertyName("city"),
            Alias("city"),
        ]
        public List<string> Areas { get; set; }

        [
            JsonPropertyName("location"),
            Alias("location"),
        ]
        public string Location { get; set; }

        public InvasionSubscription()
        {
            Areas = new List<string>();
        }
    }
}