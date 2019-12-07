namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("subscriptions"),
        Alias("subscriptions")
    ]
    public class SubscriptionObject : SubscriptionItem<SubscriptionObject>
    {
        [
            JsonProperty("enabled"),
            Alias("enabled"), 
            Default(1)
        ]
        public bool Enabled { get; set; }

        [
            JsonProperty("pokemon"),
            Alias("pokemon"), 
            Reference
        ]
        public List<PokemonSubscription> Pokemon { get; set; }

        [
            JsonProperty("raids"),
            Alias("raids"), 
            Reference]
        public List<RaidSubscription> Raids { get; set; }

        [
            JsonProperty("gyms"),
            Alias("gyms"),
            Reference
        ]
        public List<GymSubscription> Gyms { get; set; }

        [
            JsonProperty("quests"),
            Alias("quests"),
            Reference
        ]
        public List<QuestSubscription> Quests { get; set; }

        [
            JsonProperty("invasions"),
            Alias("invasions"),
            Reference
        ]
        public List<InvasionSubscription> Invasions { get; set; }

        [
            JsonProperty("distance"),
            Alias("distance"),
            Default(0)
        ]
        public int DistanceM { get; set; }

        [
            JsonProperty("latitude"),
            Alias("latitude"),
            Default(0)
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Alias("longitude"), 
            Default(0)
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("icon_style"),
            Alias("icon_style"),
            Default("Default")
        ]
        public string IconStyle { get; set; }

        //[Alias("pokemon_stats"), Reference]
        //public List<PokemonStatistics> PokemonStatistics { get; set; }

        //[Alias("raid_stats"), Reference]
        //public List<RaidStatistics> RaidStatistics { get; set; }

        //[Alias("quest_stats"), Reference]
        //public List<QuestStatistics> QuestStatistics { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public NotificationLimiter Limiter { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public bool RateLimitNotificationSent { get; set; }

        public SubscriptionObject()
        {
            Enabled = true;
            Pokemon = new List<PokemonSubscription>();
            Raids = new List<RaidSubscription>();
            Gyms = new List<GymSubscription>();
            Quests = new List<QuestSubscription>();
            Invasions = new List<InvasionSubscription>();
            //PokemonStatistics = new List<PokemonStatistics>();
            //RaidStatistics = new List<RaidStatistics>();
            //QuestStatistics = new List<QuestStatistics>();
            Limiter = new NotificationLimiter();
        }
    }
}