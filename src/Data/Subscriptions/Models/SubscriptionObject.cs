namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using ServiceStack.DataAnnotations;

    [Alias("subscriptions")]
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

        [Alias("gyms"), Reference]
        public List<GymSubscription> Gyms { get; set; }

        [Alias("quests"), Reference]
        public List<QuestSubscription> Quests { get; set; }

        [Alias("distance"), Default(0)]
        public int DistanceM { get; set; }

        [Alias("latitude"), Default(0)]
        public double Latitude { get; set; }

        [Alias("longitude"), Default(0)]
        public double Longitude { get; set; }

        [Ignore]
        public NotificationLimiter Limiter { get; set; }

        public SubscriptionObject()
        {
            Enabled = true;
            Pokemon = new List<PokemonSubscription>();
            Raids = new List<RaidSubscription>();
            Gyms = new List<GymSubscription>();
            Quests = new List<QuestSubscription>();
            Limiter = new NotificationLimiter();
        }
    }
}