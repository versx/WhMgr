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
    public class SubscriptionObject : SubscriptionItem
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
            JsonProperty("pvp"),
            Alias("pvp"),
            Reference
        ]
        public List<PvPSubscription> PvP { get; set; }

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
            JsonProperty("lures"),
            Alias("lures"),
            Reference
        ]
        public List<LureSubscription> Lures { get; set; }

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

        [
            JsonProperty("phone_number"),
            Alias("phone_number")
        ]
        public string PhoneNumber { get; set; }

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
            PvP = new List<PvPSubscription>();
            Raids = new List<RaidSubscription>();
            Gyms = new List<GymSubscription>();
            Quests = new List<QuestSubscription>();
            Invasions = new List<InvasionSubscription>();
            Limiter = new NotificationLimiter();
            DistanceM = 0;
            Latitude = 0;
            Longitude = 0;
            IconStyle = "Default";
            PhoneNumber = string.Empty;
        }
    }
}