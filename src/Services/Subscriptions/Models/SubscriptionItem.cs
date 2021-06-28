namespace WhMgr.Services.Subscriptions.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

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
            Column("id"),
            Key,
            // TODO: AutoIncrement,
        ]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the guild Id for the subscription item
        /// </summary>
        [
             JsonPropertyName("guild_id"),
             Column("guild_id"),
             Required,
        ]
        public virtual ulong GuildId { get; set; }

        /// <summary>
        /// Gets or sets the user id for the subscription id
        /// </summary>
        [
            JsonPropertyName("user_id"),
            Column("user_id"),
            Required,
        ]
        public virtual ulong UserId { get; set; }
    }
}