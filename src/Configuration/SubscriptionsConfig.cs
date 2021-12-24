namespace WhMgr.Configuration
{
    using System.IO;
    using System.Text.Json.Serialization;

    using WhMgr.Extensions;
    using WhMgr.Services.Alarms.Embeds;

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

        /// <summary>
        /// Gets or sets the maximum amount of notifications a user can receive per minute per server before being rate limited
        /// </summary>
        [JsonPropertyName("maxNotificationsPerMinute")]
        public ushort MaxNotificationsPerMinute { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the embeds file to use with direct message subscriptions
        /// </summary>
        [JsonPropertyName("dmEmbedsFile")]
        public string DmEmbedsFile { get; set; } = "default.json";

        /// <summary>
        /// Gets or sets the direct message embeds class to use for subscriptions
        /// </summary>
        [JsonIgnore]
        public EmbedMessage DmEmbeds { get; set; }

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
            MaxNotificationsPerMinute = 10;

            LoadDmEmbeds();
        }

        public void LoadDmEmbeds()
        {
            var path = Path.Combine(Strings.EmbedsFolder, DmEmbedsFile);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found at location {path}", path);
            }
            DmEmbeds = path.LoadFromFile<EmbedMessage>();
        }
    }
}