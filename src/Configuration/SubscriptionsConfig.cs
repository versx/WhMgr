namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    public class SubscriptionsConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("maxPokemonSubscriptions")]
        public int MaxPokemonSubscriptions { get; set; }

        [JsonPropertyName("maxPvPSubscriptions")]
        public int MaxPvPSubscriptions { get; set; }

        [JsonPropertyName("maxRaidSubscriptions")]
        public int MaxRaidSubscriptions { get; set; }

        [JsonPropertyName("maxQuestSubscriptions")]
        public int MaxQuestSubscriptions { get; set; }

        [JsonPropertyName("maxInvasionSubscriptions")]
        public int MaxInvasionSubscriptions { get; set; }

        [JsonPropertyName("maxLureSubscriptions")]
        public int MaxLureSubscriptions { get; set; }

        [JsonPropertyName("maxGymSubscriptions")]
        public int MaxGymSubscriptions { get; set; }

        [JsonPropertyName("maxNotificationsPerMinute")]
        public int MaxNotificationsPerMinute { get; set; }

        public SubscriptionsConfig()
        {
            Enabled = false;
            MaxPokemonSubscriptions = 0;
            MaxPvPSubscriptions = 0;
            MaxRaidSubscriptions = 0;
            MaxQuestSubscriptions = 0;
            MaxInvasionSubscriptions = 0;
            MaxLureSubscriptions = 0;
            MaxGymSubscriptions = 0;
            MaxNotificationsPerMinute = 15;
        }
    }
}