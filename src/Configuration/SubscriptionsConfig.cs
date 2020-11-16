namespace WhMgr.Configuration
{
    using Newtonsoft.Json;

    public class SubscriptionsConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("maxPokemonSubscriptions")]
        public int MaxPokemonSubscriptions { get; set; }

        [JsonProperty("maxPvPSubscriptions")]
        public int MaxPvPSubscriptions { get; set; }

        [JsonProperty("maxRaidSubscriptions")]
        public int MaxRaidSubscriptions { get; set; }

        [JsonProperty("maxQuestSubscriptions")]
        public int MaxQuestSubscriptions { get; set; }

        [JsonProperty("maxInvasionSubscriptions")]
        public int MaxInvasionSubscriptions { get; set; }

        [JsonProperty("maxGymSubscriptions")]
        public int MaxGymSubscriptions { get; set; }

        [JsonProperty("maxNotificationsPerMinute")]
        public int MaxNotificationsPerMinute { get; set; }

        public SubscriptionsConfig()
        {
            Enabled = false;
            MaxPokemonSubscriptions = 0;
            MaxPvPSubscriptions = 0;
            MaxRaidSubscriptions = 0;
            MaxQuestSubscriptions = 0;
            MaxInvasionSubscriptions = 0;
            MaxGymSubscriptions = 0;
            MaxNotificationsPerMinute = 15;
        }
    }
}