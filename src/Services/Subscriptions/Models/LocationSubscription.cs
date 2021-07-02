namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    [Table("locations")]
    public class LocationSubscription : BaseSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("name"),
            Column("name"),
        ]
        public string Name { get; set; }

        [
            JsonPropertyName("distance"),
            Column("distance"),
            DefaultValue(0),
        ]
        public int DistanceM { get; set; }

        /// <summary>
        /// Gets or sets the latitude to use with distance checks
        /// </summary>
        [
            JsonPropertyName("latitude"),
            Column("latitude"),
            DefaultValue(0),
        ]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude to use with distance checks
        /// </summary>
        [
            JsonPropertyName("longitude"),
            Column("longitude"),
            DefaultValue(0),
        ]
        public double Longitude { get; set; }
    }
}