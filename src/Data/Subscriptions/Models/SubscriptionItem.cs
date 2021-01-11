namespace WhMgr.Data.Subscriptions.Models
{
    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    /// <summary>
    /// Base subscription object all subscription items inherit from
    /// </summary>
    public abstract class SubscriptionItem
    {
        /// <summary>
        /// Gets or sets the unique primary key Id for the subscription item
        /// </summary>
        [
            JsonIgnore,//JsonProperty("id"),
            Alias("id"),
            PrimaryKey,
            AutoIncrement
        ]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the guild Id for the subscription item
        /// </summary>
        [
             JsonProperty("guild_id"),
             Alias("guild_id"),
             Required
        ]
        public virtual ulong GuildId { get; set; }

        /// <summary>
        /// Gets or sets the user id for the subscription id
        /// </summary>
        [
            JsonProperty("user_id"),
            Alias("user_id"),
            Required
        ]
        public virtual ulong UserId { get; set; }
    }
}