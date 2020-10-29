namespace WhMgr.Data.Subscriptions.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("invasions"),
        Table("invasions")
    ]
    public class InvasionSubscription : SubscriptionItem
    {
        [
            JsonProperty("subscription_id"),
            Column("subscription_id"),
            ForeignKey("subscription_id"),
            Required
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("subscription"),
        ]
        public SubscriptionObject Subscription { get; set; }

        [
            JsonProperty("reward_pokemon_id"),
            Column("reward_pokemon_id"),
            Required
        ]
        public int RewardPokemonId { get; set; }

        [
            JsonProperty("city"),
            Column("city"),
            Required
        ]
        public string City { get; set; }
    }
}