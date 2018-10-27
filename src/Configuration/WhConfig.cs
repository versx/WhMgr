namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models.Providers;

    public class WhConfig
    {
        private static readonly EventLogger _logger = EventLogger.GetLogger();

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("ownerId")]
        public ulong OwnerId { get; set; }

        [JsonProperty("supporterRoleId")]
        public ulong SupporterRoleId { get; set; }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("moderators")]
        public List<ulong> Moderators { get; set; }

        [JsonProperty("webhookPort")]
        public ushort WebHookPort { get; set; }

        [JsonProperty("gmapsKey")]
        public string GmapsKey { get; set; }

        [JsonProperty("mapProvider")]
        public MapProviderType MapProvider { get; set; }

        [JsonProperty("mapProviderFork")]
        public MapProviderFork MapProviderFork { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("commandPrefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("cityRoles")]
        public List<string> CityRoles { get; set; }

        public WhConfig()
        {
            CityRoles = new List<string>();
            MapProvider = MapProviderType.RealDeviceMap;
            MapProviderFork = MapProviderFork.Default;
            Moderators = new List<ulong>();
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

            var data = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<WhConfig>(data);
        }
    }
}