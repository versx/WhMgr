namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("gyms"),
        Alias("gyms")
    ]
    public class GymSubscription
    {
        [
            JsonIgnore,
            Alias("id"), 
            PrimaryKey, 
            AutoIncrement
        ]
        public int Id { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"), 
            ForeignKey(typeof(SubscriptionObject))
        ]
        public ulong UserId { get; set; }

        [
            JsonProperty("name"),
            Alias("name"), 
            Unique
        ]
        public string Name { get; set; }
    }
}