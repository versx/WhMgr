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
        public Dictionary<int, PokemonSubscription> Pokemon { get; set; }

        [JsonProperty("raids")]
        public Dictionary<int, RaidSubscription> Raids { get; set; }

        [JsonProperty("notifications_today")]
        public long NotificationsToday { get; set; }
    }
}