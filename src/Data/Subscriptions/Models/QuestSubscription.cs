namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("quests"),
        Alias("quests")
    ]
    public class QuestSubscription
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