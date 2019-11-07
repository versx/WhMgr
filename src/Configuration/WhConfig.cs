namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Data;

    //public class SubscriptionsConfiguration
    //{
    //    [JsonProperty("enableSubscriptions")]
    //    public bool EnableSubscriptions { get; set; }

    //    [JsonProperty("enableCities")]
    //    public bool EnableCities { get; set; }

    //    [JsonProperty("citiesRequireSupporterRole")]
    //    public bool CitiesRequireSupporterRole { get; set; }

    //    [JsonProperty("iconStyle")]
    //    public string IconStyle { get; set; }

    //    [JsonProperty("iconStyles")]
    //    public Dictionary<string, string> IconStyles { get; set; }
    //}

    public class DiscordServerConfiguration
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("ownerId")]
        public ulong OwnerId { get; set; }

        [JsonProperty("donorRoleIds")]
        public List<ulong> DonorRoleIds { get; set; }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("emojiGuildId")]
        public ulong EmojiGuildId { get; set; }

        [JsonProperty("botChannelIds")]
        public List<ulong> BotChannelIds { get; set; }

        [JsonProperty("moderators")]
        public List<ulong> Moderators { get; set; }

        [JsonProperty("enableSubscriptions")]
        public bool EnableSubscriptions { get; set; }

        [JsonProperty("enableCities")]
        public bool EnableCities { get; set; }

        [JsonProperty("citiesRequireSupporterRole")]
        public bool CitiesRequireSupporterRole { get; set; }

        [JsonProperty("cityRoles")]
        public List<string> CityRoles { get; set; }

        [JsonProperty("questChannelIds")]
        public List<ulong> QuestChannelIds { get; set; }

        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; set; }

        public DiscordServerConfiguration()
        {
            BotChannelIds = new List<ulong>();
            CityRoles = new List<string>();
            DonorRoleIds = new List<ulong>();
            Moderators = new List<ulong>();
            QuestChannelIds = new List<ulong>();
        }
    }

    public class WhConfig
    {
        [JsonProperty("servers")]
        public Dictionary<ulong, DiscordServer> Servers { get; set; }

        [JsonProperty("discord")]
        public DiscordServerConfiguration Discord { get; set; }

        [JsonProperty("webhookPort")]
        public ushort WebhookPort { get; set; }

        [JsonProperty("connectionStrings")]
        public ConnectionStringsConfiguration ConnectionStrings { get; set; }

        [JsonProperty("eventPokemonIds")]
        public List<int> EventPokemonIds { get; set; }

        [JsonProperty("shortUrlApiUrl")]
        public string ShortUrlApiUrl { get; set; }

        [JsonProperty("shinyStats")]
        public ShinyStatsConfiguration ShinyStats { get; set; }

        [JsonProperty("urls")]
        public UrlConfiguration Urls { get; set; }

        [JsonProperty("staticMap")]
        public StaticMapConfiguration StaticMap { get; set; }

        [JsonProperty("iconStyle")]
        public string IconStyle { get; set; }

        [JsonProperty("iconStyles")]
        public Dictionary<string, string> IconStyles { get; set; }

        [JsonProperty("stripeApiKey")]
        public string StripeApiKey { get; set; }

        [JsonProperty("nestsChannelId")]
        public ulong NestsChannelId { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public WhConfig()
        {
            Servers = new Dictionary<ulong, DiscordServer>();
            Discord = new DiscordServerConfiguration();
            ConnectionStrings = new ConnectionStringsConfiguration();
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