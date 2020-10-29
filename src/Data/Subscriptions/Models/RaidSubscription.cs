namespace WhMgr.Data.Subscriptions.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("raids"),
        Table("raids")
    ]
    public class RaidSubscription : SubscriptionItem
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
            JsonProperty("pokemon_id"),
            Column("pokemon_id"),
            Required
        ]
        public int PokemonId { get; set; }

        [
            JsonProperty("form"),
            Column("form")
        ]
        public string Form { get; set; }

        [
            JsonProperty("city"),
            Column("city"),
            Required
        ]
        public string City { get; set; }
    }
}