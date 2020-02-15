namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

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

        /*
        [
            JsonProperty("grunt_type"),
            Alias("grunt_type"), 
            Required
        ]
        public InvasionGruntType GruntType { get; set; }
        */
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
        public string City { get; set; }
    }
}