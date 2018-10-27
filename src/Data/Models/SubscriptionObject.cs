namespace WhMgr.Data.Models
{
    using System;
    using System.Collections.Generic;

    using ServiceStack.DataAnnotations;

    [Alias("subscription")]
    public class SubscriptionObject
    {
        [Alias("id"), AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), PrimaryKey]
        public ulong UserId { get; set; }

        [Alias("enabled"), Default(1)]
        public bool Enabled { get; set; }

        [Alias("pokemon"), Reference]
        public List<PokemonSubscription> Pokemon { get; set; }

        [Alias("raids"), Reference]
        public List<RaidSubscription> Raids { get; set; }

        [Alias("notifications_today")]
        public long NotificationsToday { get; set; }

        [Ignore]
        public NotificationLimiter Limiter { get; set; }

        public SubscriptionObject()
        {
            Enabled = true;
            Pokemon = new List<PokemonSubscription>();
            Raids = new List<RaidSubscription>();
            Limiter = new NotificationLimiter();
        }
    }
}