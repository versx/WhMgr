namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("subscriptions"),
        Table("subscriptions")
    ]
    public class SubscriptionObject : SubscriptionItem
    {
        /*
        public List<PokemonSubscription> Pokemon { get; set; }

        public List<PvPSubscription> PvP { get; set; }

        public List<RaidSubscription> Raids { get; set; }

        public List<QuestSubscription> Quests { get; set; }

        public List<GymSubscription> Gyms { get; set; }

        public List<InvasionSubscription> Invasions { get; set; }
        */

        [
            JsonProperty("enabled"),
            Column("enabled"),
            DefaultValue(1)
        ]
        public bool Enabled { get; set; }

        [
            JsonProperty("distance"),
            Column("distance"),
            DefaultValue(0)
        ]
        public int DistanceM { get; set; }

        [
            JsonProperty("latitude"),
            Column("latitude"),
            DefaultValue(0)
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Column("longitude"),
            DefaultValue(0)
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("icon_style"),
            Column("icon_style"),
            DefaultValue("Default")
        ]
        public string IconStyle { get; set; }

        [
            JsonProperty("phone_number"),
            Column("phone_number")
        ]
        public string PhoneNumber { get; set; }

        [
            JsonIgnore,
            NotMapped
        ]
        public NotificationLimiter Limiter { get; set; }

        [
            JsonIgnore,
            NotMapped
        ]
        public bool RateLimitNotificationSent { get; set; }

        public SubscriptionObject()
        {
            /*
            Pokemon = new List<PokemonSubscription>();
            PvP = new List<PvPSubscription>();
            Raids = new List<RaidSubscription>();
            Quests = new List<QuestSubscription>();
            Gyms = new List<GymSubscription>();
            Invasions = new List<InvasionSubscription>();
            */
            Enabled = true;
            Limiter = new NotificationLimiter();
            DistanceM = 0;
            Latitude = 0;
            Longitude = 0;
            IconStyle = "Default";
            PhoneNumber = string.Empty;
        }
    }
}