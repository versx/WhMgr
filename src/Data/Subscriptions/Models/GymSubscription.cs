namespace WhMgr.Data.Subscriptions.Models
{
    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    [
        JsonObject("gyms"),
        Alias("gyms")
    ]
    public class GymSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("name"),
            Alias("name"),
            Unique
        ]
        public string Name { get; set; }
    }
}