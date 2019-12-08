namespace WhMgr.Data.Subscriptions.Models
{
    using Newtonsoft.Json;

    using ServiceStack.DataAnnotations;

    public abstract class SubscriptionItem
    {
        [
            JsonIgnore,//JsonProperty("id"),
            Alias("id"),
            PrimaryKey,
            AutoIncrement
        ]
        public int Id { get; set; }

        [
             JsonProperty("guild_id"),
             Alias("guild_id"),
             Required
         ]
        public virtual ulong GuildId { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"),
            Required
        ]
        public virtual ulong UserId { get; set; }
    }
}