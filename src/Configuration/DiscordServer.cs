namespace WhMgr.Configuration
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class DiscordServer
    {
        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("ownerId")]
        public ulong OwnerId { get; set; }

        public ulong DonorRoleId { get; set; }

        [JsonProperty("moderatorIds")]
        public List<ulong> Moderators { get; set; }

        [JsonProperty("enableSubscriptions")]
        public bool EnableSubscriptions { get; set; }

        [JsonProperty("enableCities")]
        public bool EnableCities { get; set; }

        [JsonProperty("cityRoles")]
        public string CityRoles { get; set; }

        [JsonProperty("pruneQuestChannels")]
        public bool PruneQuestChannels { get; set; }

        [JsonProperty("questChannelIds")]
        public List<ulong> QuestChannelIds { get; set; }

        public DiscordServer()
        {
            Moderators = new List<ulong>();
            QuestChannelIds = new List<ulong>();
        }
    }
}