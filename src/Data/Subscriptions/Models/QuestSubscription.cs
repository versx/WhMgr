namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    [
        JsonObject("quests"),
        Alias("quests"),
    ]
    public class QuestSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"), 
            ForeignKey(typeof(SubscriptionObject)),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("pokestop_name"),
            Alias("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonProperty("reward"),
            Alias("reward"), 
            Required,
        ]
        public string RewardKeyword { get; set; }

        [
            JsonProperty("city"),
            Alias("city"), 
        ]
        public List<string> Areas { get; set; }

        [
            JsonProperty("location"),
            Alias("location"),
        ]
        public string Location { get; set; }

        public QuestSubscription()
        {
            Areas = new List<string>();
        }
    }
}