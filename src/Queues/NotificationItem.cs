namespace WhMgr.Queues
{
    using DSharpPlus.Entities;

    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Services.Webhook.Models;

    /// <summary>
    /// Notification queue item class
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// Gets or sets the subscription associated with the notification
        /// </summary>
        public Subscription Subscription { get; set; }

        /// <summary>
        /// Gets or sets the Discord member to receive the notification
        /// </summary>
        public DiscordMember Member { get; set; }

        /// <summary>
        /// Gets or sets the Discord embed message to send
        /// </summary>
        public DiscordEmbed Embed { get; set; }

        /// <summary>
        /// Gets or sets the optional description of the message
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the geofence city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the Pokemon data object to read from for text message alerts
        /// </summary>
        public PokemonData Pokemon { get; set; }
    }
}