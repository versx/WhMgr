namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("gyms"),
        Alias("gyms")
    ]
    public class GymSubscription : SubscriptionItem<GymSubscription>
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