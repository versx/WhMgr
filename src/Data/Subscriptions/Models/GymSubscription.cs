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
            Alias("subscription_id"), 
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
             JsonProperty("guild_id"),
             Alias("guild_id"),
             Required
        ]
        public ulong GuildId { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"), 
            Required
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