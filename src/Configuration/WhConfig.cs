namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Data;

    public class WhConfig
    {
        [JsonProperty("port")]
        public ushort WebhookPort { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("shortUrlApiUrl")]
        public string ShortUrlApiUrl { get; set; }

        [JsonProperty("stripeApiKey")]
        public string StripeApiKey { get; set; }

        [JsonProperty("servers")]
        public Dictionary<ulong, DiscordServerConfig> Servers { get; set; }

        [JsonProperty("database")]
        public ConnectionStringsConfig Database { get; set; }

        [JsonProperty("urls")]
        public UrlConfig Urls { get; set; }

        //[JsonProperty("staticMap")]
        //public StaticMapConfig StaticMap { get; set; }

        [JsonProperty("eventPokemonIds")]
        public List<int> EventPokemonIds { get; set; }

        [JsonProperty("iconStyles")]
        public Dictionary<string, string> IconStyles { get; set; }

        [JsonProperty("enableDST")]
        public bool EnableDST { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public WhConfig()
        {
            Locale = "en";
            Servers = new Dictionary<ulong, DiscordServerConfig>();
            Database = new ConnectionStringsConfig();
            Urls = new UrlConfig();
            EventPokemonIds = new List<int>();
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

            return MasterFile.LoadInit<WhConfig>(filePath, typeof(WhConfig));
        }
    }
}