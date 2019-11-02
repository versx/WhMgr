namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Data;

    public class WhConfig
    {
        [JsonProperty("servers")]
        public Dictionary<ulong, DiscordServer> Servers { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("ownerId")]
        public ulong OwnerId { get; set; }

        [JsonProperty("donorRoleIds")]
        public List<ulong> DonorRoleIds { get; set; }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("botChannelIds")]
        public List<ulong> BotChannelIds { get; set; }

        [JsonProperty("moderators")]
        public List<ulong> Moderators { get; set; }

        [JsonProperty("webhookPort")]
        public ushort WebhookPort { get; set; }

        [JsonProperty("enableSubscriptions")]
        public bool EnableSubscriptions { get; set; }

        [JsonProperty("enableCities")]
        public bool EnableCities { get; set; }

        [JsonProperty("citiesRequireSupporterRole")]
        public bool CitiesRequireSupporterRole { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("scannerConnectionString")]
        public string ScannerConnectionString { get; set; }

        [JsonProperty("nestsConnectionString")]
        public string NestsConnectionString { get; set; }

        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("cityRoles")]
        public List<string> CityRoles { get; set; }

        [JsonProperty("questChannelIds")]
        public List<ulong> QuestChannelIds { get; set; }

        [JsonProperty("eventPokemonIds")]
        public List<int> EventPokemonIds { get; set; }

        [JsonProperty("shortUrlApiUrl")]
        public string ShortUrlApiUrl { get; set; }

        [JsonProperty("shinyStats")]
        public ShinyStatsConfiguration ShinyStats { get; set; }

        [JsonProperty("urls")]
        public UrlConfiguration Urls { get; set; }

        [JsonProperty("iconStyle")]
        public string IconStyle { get; set; }

        [JsonProperty("iconStyles")]
        public Dictionary<string, string> IconStyles { get; set; }

        [JsonProperty("stripeApiKey")]
        public string StripeApiKey { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public WhConfig()
        {
            Servers = new Dictionary<ulong, DiscordServer>();
            BotChannelIds = new List<ulong>();
            CityRoles = new List<string>();
            Moderators = new List<ulong>();
            QuestChannelIds = new List<ulong>();
            EventPokemonIds = new List<int>();
            ShinyStats = new ShinyStatsConfiguration();
            Urls = new UrlConfiguration();
            IconStyle = "Default";
            IconStyles = new Dictionary<string, string>();
        }

        public void Save(string filePath)
        {
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, data);
        }

        public static WhConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Config not loaded because file not found.", filePath);
            }

            return Database.LoadInit<WhConfig>(filePath, typeof(WhConfig));
        }
    }
}