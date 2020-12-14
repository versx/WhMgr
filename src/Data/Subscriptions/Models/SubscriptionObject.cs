namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    /// <summary>
    /// User subscription class
    /// </summary>
    [
        JsonObject("subscriptions"),
        Alias("subscriptions")
    ]
    public class SubscriptionObject : SubscriptionItem
    {
        /// <summary>
        /// Gets or sets a value determining whether the associated users
        /// subscriptions are enabled or not
        /// </summary>
        [
            JsonProperty("enabled"),
            Alias("enabled"), 
            Default(1)
        ]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Pokemon subscriptions
        /// </summary>
        [
            JsonProperty("pokemon"),
            Alias("pokemon"), 
            Reference
        ]
        public List<PokemonSubscription> Pokemon { get; set; }

        /// <summary>
        /// Gets or sets the PvP Pokemon subscriptions
        /// </summary>
        [
            JsonProperty("pvp"),
            Alias("pvp"),
            Reference
        ]
        public List<PvPSubscription> PvP { get; set; }

        /// <summary>
        /// Gets or sets the Raid subscriptions
        /// </summary>
        [
            JsonProperty("raids"),
            Alias("raids"), 
            Reference]
        public List<RaidSubscription> Raids { get; set; }

        /// <summary>
        /// Gets or sets the Gym subscriptions to use with Raid subscriptions
        /// </summary>
        [
            JsonProperty("gyms"),
            Alias("gyms"),
            Reference
        ]
        public List<GymSubscription> Gyms { get; set; }

        /// <summary>
        /// Gets or sets the Quest subscriptions
        /// </summary>
        [
            JsonProperty("quests"),
            Alias("quests"),
            Reference
        ]
        public List<QuestSubscription> Quests { get; set; }

        /// <summary>
        /// Gets or sets the Team Rocket Invasion subscriptions
        /// </summary>
        [
            JsonProperty("invasions"),
            Alias("invasions"),
            Reference
        ]
        public List<InvasionSubscription> Invasions { get; set; }

        /// <summary>
        /// Gets or sets the distance in meters a subscription should be within
        /// to trigger
        /// </summary>
        [
            JsonProperty("distance"),
            Alias("distance"),
            Default(0)
        ]
        public int DistanceM { get; set; }

        /// <summary>
        /// Gets or sets the latitude to use with distance checks
        /// </summary>
        [
            JsonProperty("latitude"),
            Alias("latitude"),
            Default(0)
        ]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude to use with distance checks
        /// </summary>
        [
            JsonProperty("longitude"),
            Alias("longitude"), 
            Default(0)
        ]
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the icon style to use for the subscription notification
        /// </summary>
        [
            JsonProperty("icon_style"),
            Alias("icon_style"),
            Default("Default")
        ]
        public string IconStyle { get; set; }

        /// <summary>
        /// Gets or sets the phone number to send ultra rare Pokemon notifications to
        /// </summary>
        [
            JsonProperty("phone_number"),
            Alias("phone_number")
        ]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets the <seealso cref="NotificationLimiter"/> class associated with the subscription
        /// </summary>
        [
            JsonIgnore,
            Ignore
        ]
        public NotificationLimiter Limiter { get; }

        /// <summary>
        /// Gets or sets a value determining whether the rate limit notification
        /// has been sent to the user already
        /// </summary>
        [
            JsonIgnore,
            Ignore
        ]
        public bool RateLimitNotificationSent { get; set; }

        /// <summary>
        /// Instantiates a new subscription object
        /// </summary>
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