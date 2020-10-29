namespace WhMgr.Data.Subscriptions.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("gyms"),
        Table("gyms")
    ]
    public class GymSubscription : SubscriptionItem
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
            JsonProperty("name"),
            Column("name"),
            Required
        ]
        public string Name { get; set; }
    }
}