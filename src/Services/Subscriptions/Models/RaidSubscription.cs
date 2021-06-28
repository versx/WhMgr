namespace WhMgr.Services.Subscriptions.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    [Table("raids")]
    public class RaidSubscription : SubscriptionItem
    {
        [
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("FK_raids_subscriptions_subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            //Required,
        ]
        public uint PokemonId { get; set; }

        [
            JsonPropertyName("form"),
            Column("form"),
        ]
        public string Form { get; set; }

        [
            JsonPropertyName("city"),
            Column("city"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}