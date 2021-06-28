namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    [Table("quests")]
    public class QuestSubscription : SubscriptionItem
    {
        [
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey(""),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Column("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("reward"),
            Column("reward"),
            //Required,
        ]
        public string RewardKeyword { get; set; }

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