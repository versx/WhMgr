namespace WhMgr
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;
    using WhMgr.Data.Subscriptions.Models;

    public sealed class NotificationQueue : Queue<NotificationItem>
    {
    }

    public class NotificationItem
    {
        public SubscriptionObject Subscription { get; set; }

        public DiscordMember Member { get; }

        public DiscordEmbed Embed { get; }

        public string Description { get; set; }

        public NotificationItem(SubscriptionObject subscription, DiscordMember member, DiscordEmbed embed, string description)
        {
            Subscription = subscription;
            Member = member;
            Embed = embed;
            Description = description;
        }
    }
}