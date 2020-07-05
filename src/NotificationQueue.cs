namespace WhMgr
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Net.Models;

    /// <summary>
    /// Notification queue wrapper class
    /// </summary>
    public sealed class NotificationQueue : Queue<NotificationItem>
    {
    }

    /// <summary>
    /// Notification queue item class
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// Gets or sets the subscription associated with the notification
        /// </summary>
        public SubscriptionObject Subscription { get; set; }

        /// <summary>
        /// Gets or sets the Discord member to receive the notification
        /// </summary>
        public DiscordMember Member { get; }

        /// <summary>
        /// Gets or sets the Discord embed message to send
        /// </summary>
        public DiscordEmbed Embed { get; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="member"></param>
        /// <param name="embed"></param>
        /// <param name="description"></param>
        public NotificationItem(SubscriptionObject subscription, DiscordMember member, DiscordEmbed embed, string description, string city, PokemonData pokemon = null)
        {
            Subscription = subscription;
            Member = member;
            Embed = embed;
            Description = description;
            City = city;
            Pokemon = pokemon;
        }
    }
}