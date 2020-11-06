namespace WhMgr.Data.Subscriptions.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("quests"),
        Table("quests")
    ]
    public class QuestSubscription : SubscriptionItem
    {
        [
            JsonProperty("subscription_id"),
            Column("subscription_id"),
            ForeignKey("subscription_id"),
            Required
        ]
        public int SubscriptionId { get; set; }

        //[
        //    JsonProperty("subscription"),
        //]
        //public SubscriptionObject Subscription { get; set; }

        [
            JsonProperty("reward"),
            Column("reward"),
            Required
        ]
        public string RewardKeyword { get; set; }

        [
            JsonProperty("city"),
            Column("city"),
            Required
        ]
        public string City { get; set; }
    }
}