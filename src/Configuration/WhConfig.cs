namespace T.Configuration
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    using T.Diagnostics;

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

        [JsonProperty("guidId")]
        public ulong GuidId { get; set; }

        [JsonProperty("webhookPort")]
        public ushort WebHookPort { get; set; }

        [JsonProperty("gmapsKey")]
        public string GmapsKey { get; set; }

        public WhConfig()
        {
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