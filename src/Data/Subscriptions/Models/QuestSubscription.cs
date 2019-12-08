namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("quests"),
        Alias("quests")
    ]
    public class QuestSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"), 
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("reward"),
            Alias("reward"), 
            Required
        ]
        public string RewardKeyword { get; set; }

        [
            JsonProperty("city"),
            Alias("city"), 
            Required
        ]
        public string City { get; set; }
    }
}