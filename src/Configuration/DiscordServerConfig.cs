namespace WhMgr.Configuration
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class DiscordServerConfig
    {
        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("emojiGuildId")]
        public ulong EmojiGuildId { get; set; }

        [JsonProperty("ownerId")]
        public ulong OwnerId { get; set; }

        [JsonProperty("donorRoleIds")]
        public List<ulong> DonorRoleIds { get; set; }

        [JsonProperty("moderatorIds")]
        public List<ulong> Moderators { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("alarms")]
        public string AlarmsFile { get; set; }

        [JsonProperty("enableSubscriptions")]
        public bool EnableSubscriptions { get; set; }

        [JsonProperty("enableCities")]
        public bool EnableCities { get; set; }

        [JsonProperty("cityRoles")]
        public List<string> CityRoles { get; set; }

        [JsonProperty("citiesRequireSupporterRole")]
        public bool CitiesRequireSupporterRole { get; set; }

        [JsonProperty("pruneQuestChannels")]
        public bool PruneQuestChannels { get; set; }

        [JsonProperty("questChannelIds")]
        public List<ulong> QuestChannelIds { get; set; }

        [JsonProperty("nestsChannelId")]
        public ulong NestsChannelId { get; set; }

        [JsonProperty("shinyStats")]
        public ShinyStatsConfig ShinyStats { get; set; }

        [JsonProperty("iconStyle")]
        public string IconStyle { get; set; }

        [JsonProperty("botChannelIds")]
        public List<ulong> BotChannelIds { get; set; }

        public DiscordServerConfig()
        {
            Moderators = new List<ulong>();
            CityRoles = new List<string>();
            IconStyle = "Default";
            QuestChannelIds = new List<ulong>();
            ShinyStats = new ShinyStatsConfig();
        }
    }

    public class SubscriptionsConfiguration
    {
        [JsonProperty("enableSubscriptions")]
        public bool EnableSubscriptions { get; set; }

        [JsonProperty("enableCities")]
        public bool EnableCities { get; set; }

        [JsonProperty("citiesRequireSupporterRole")]
        public bool CitiesRequireSupporterRole { get; set; }

        [JsonProperty("iconStyle")]
        public string IconStyle { get; set; }
    }
}