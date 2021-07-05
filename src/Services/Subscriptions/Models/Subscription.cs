namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    
    /// <summary>
    /// User subscription class
    /// </summary>
    [Table("subscriptions")]
    public class Subscription : BaseSubscription
    {
        /// <summary>
        /// Gets or sets a value determining whether the associated users
        /// subscriptions are enabled or not
        /// </summary>
        [
            JsonPropertyName("status"),
            Column("status"),
            DefaultValue((int)NotificationStatusType.All),
        ]
        public NotificationStatusType Status { get; set; }

        public bool IsEnabled(NotificationStatusType status)
        {
            return (Status & status) == status;
        }

        public void EnableNotificationType(NotificationStatusType status)
        {
            Status |= status;
        }

        public void DisableNotificationType(NotificationStatusType status)
        {
            Status &= (~status);
        }

        /// <summary>
        /// Gets or sets the Pokemon subscriptions
        /// </summary>
        [
            JsonPropertyName("pokemon"),
            Column("pokemon"),
            //Reference,
        ]
        public ICollection<PokemonSubscription> Pokemon { get; set; }

        /// <summary>
        /// Gets or sets the PvP Pokemon subscriptions
        /// </summary>
        [
            JsonPropertyName("pvp"),
            Column("pvp"),
            //Reference,
        ]
        public ICollection<PvpSubscription> PvP { get; set; }

        /// <summary>
        /// Gets or sets the Raid subscriptions
        /// </summary>
        [
            JsonPropertyName("raids"),
            Column("raids"),
            //Reference,
        ]
        public ICollection<RaidSubscription> Raids { get; set; }

        /// <summary>
        /// Gets or sets the Gym subscriptions to use with Raid subscriptions
        /// </summary>
        [
            JsonPropertyName("gyms"),
            Column("gyms"),
            //Reference,
        ]
        public ICollection<GymSubscription> Gyms { get; set; }

        /// <summary>
        /// Gets or sets the Quest subscriptions
        /// </summary>
        [
            JsonPropertyName("quests"),
            Column("quests"),
            //Reference,
        ]
        public ICollection<QuestSubscription> Quests { get; set; }

        /// <summary>
        /// Gets or sets the Team Rocket Invasion subscriptions
        /// </summary>
        [
            JsonPropertyName("invasions"),
            Column("invasions"),
            //Reference,
        ]
        public ICollection<InvasionSubscription> Invasions { get; set; }

        /// <summary>
        /// Gets or sets the distance in meters a subscription should be within
        /// to trigger
        /// </summary>
        [
            JsonPropertyName("lures"),
            Column("lures"),
            //Reference,
        ]
        public ICollection<LureSubscription> Lures { get; set; }

        [
            JsonPropertyName("locations"),
            Column("locations"),
            //Reference,
        ]
        public ICollection<LocationSubscription> Locations { get; set; }

        [
            JsonPropertyName("location"),
            Column("location"),
            DefaultValue(null),
        ]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the icon style to use for the subscription notification
        /// </summary>
        [
            JsonPropertyName("icon_style"),
            Column("icon_style"),
            DefaultValue("Default"),
        ]
        public string IconStyle { get; set; }

        /// <summary>
        /// Gets or sets the phone number to send ultra rare Pokemon notifications to
        /// </summary>
        [
            JsonPropertyName("phone_number"),
            Column("phone_number"),
        ]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets the <seealso cref="NotificationLimiter"/> class associated with the subscription
        /// </summary>
        [
            JsonIgnore,
            NotMapped,
        ]
        public NotificationLimiter Limiter { get; }

        /// <summary>
        /// Gets or sets a value determining whether the rate limit notification
        /// has been sent to the user already
        /// </summary>
        [
            JsonIgnore,
            NotMapped,
        ]
        public bool RateLimitNotificationSent { get; set; }

        /// <summary>
        /// Instantiates a new subscription object
        /// </summary>
        public Subscription()
        {
            Status = NotificationStatusType.All;
            Limiter = new NotificationLimiter();
            IconStyle = "Default";
            PhoneNumber = string.Empty;
        }
    }
}