namespace WhMgr.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class SubscriptionObject
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("pokemon")]
        public List<PokemonSubscription> Pokemon { get; set; }

        [JsonProperty("raids")]
        public List<RaidSubscription> Raids { get; set; }

        [JsonProperty("notifications_today")]
        public long NotificationsToday { get; set; }

        [JsonIgnore]
        public NotificationLimiter Limiter { get; set; }

        public SubscriptionObject()
        {
            Pokemon = new List<PokemonSubscription>();
            Raids = new List<RaidSubscription>();
            Limiter = new NotificationLimiter();
        }
    }
}