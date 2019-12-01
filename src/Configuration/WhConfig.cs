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

    public class WhConfig
    {
        [JsonProperty("port")]
        public ushort WebhookPort { get; set; }

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

        [JsonProperty("staticMap")]
        public StaticMapConfig StaticMap { get; set; }

        [JsonProperty("eventPokemonIds")]
        public List<int> EventPokemonIds { get; set; }

        [JsonProperty("iconStyles")]
        public Dictionary<string, string> IconStyles { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public WhConfig()
        {
            Servers = new Dictionary<ulong, DiscordServerConfig>();
            Database = new ConnectionStringsConfig();
            EventPokemonIds = new List<int>();
            Urls = new UrlConfig();
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

            return Data.Database.LoadInit<WhConfig>(filePath, typeof(WhConfig));
        }
    }
}