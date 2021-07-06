namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;
    using WhMgr.Utilities;

    /// <summary>
    /// Configuration file class
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the HTTP listening interface/host address
        /// </summary>
        [JsonPropertyName("host")]
        public string ListeningHost { get; set; }

        /// <summary>
        /// Gets or sets the HTTP listening port
        /// </summary>
        [JsonPropertyName("port")]
        public ushort WebhookPort { get; set; }

        /// <summary>
        /// Gets or sets the locale translation file to use
        /// </summary>
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the short url API url (yourls.org)
        /// </summary>
        [JsonPropertyName("shortUrlApiUrl")]
        public string ShortUrlApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the Stripe API key
        /// </summary>
        [JsonPropertyName("stripeApiKey")]
        public string StripeApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Discord servers configuration
        /// </summary>
        [JsonIgnore]
        public Dictionary<ulong, DiscordServerConfig> Servers { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("servers")]
        public Dictionary<string, string> ServerConfigFiles { get; set; } = new();

        /// <summary>
        /// Gets or sets the Database configuration
        /// </summary>
        [JsonPropertyName("database")]
        public ConnectionStringsConfig Database { get; set; } = new();

        /// <summary>
        /// Gets or sets the Urls configuration
        /// </summary>
        [JsonPropertyName("urls")]
        public UrlConfig Urls { get; set; } = new();

        /// <summary>
        /// Gets or sets the event Pokemon IDs list
        /// </summary>
        [JsonPropertyName("eventPokemonIds")]
        public List<int> EventPokemonIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the minimum IV value for an event Pokemon to be to process
        /// for channel alarms or direct message subscriptions
        /// </summary>
        [JsonPropertyName("eventMinimumIV")]
        public int EventMinimumIV { get; set; }

        /// <summary>
        /// Gets or sets the icon styles
        /// </summary>
        [JsonPropertyName("iconStyles")]
        public Dictionary<string, string> IconStyles { get; set; } = new();

        /// <summary>
        /// Gets or sets the static map template files to use per type
        /// </summary>
        [JsonPropertyName("staticMaps")]
        public Dictionary<StaticMapType, StaticMapConfig> StaticMaps { get; set; } = new();

        /// <summary>
        /// Gets or sets the Google maps key for location lookup
        /// </summary>
        [JsonPropertyName("gmapsKey")]
        public string GoogleMapsKey { get; set; }

        /// <summary>
        /// Gets or sets the OpenStreetMaps Nominatim endpoint to use for reverse location lookup
        /// </summary>
        [JsonPropertyName("nominatim")]
        public string NominatimEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the OpenStreetMaps Nominatim location string schema
        /// </summary>
        [JsonPropertyName("nominatimSchema")]
        public string NominatimSchema { get; set; }

        /// <summary>
        /// Gets or sets the minimum despawn time in minutes a Pokemon must have in order to send the alarm
        /// </summary>
        [JsonPropertyName("despawnTimeMinimumMinutes")]
        public ushort DespawnTimeMinimumMinutes { get; set; }

        /// <summary>
        /// Gets or sets the interval in minutes to reload subscriptions to accomodate the UI changes
        /// </summary>
        [JsonPropertyName("reloadSubscriptionChangesMinutes")]
        public ushort ReloadSubscriptionChangesMinutes { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of notifications a user can receive per minute per server before being rate limited
        /// </summary>
        [JsonPropertyName("maxNotificationsPerMinute")]
        public ushort MaxNotificationsPerMinute { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to check for duplicate webhook messages or not
        /// </summary>
        [JsonPropertyName("checkForDuplicates")]
        public bool CheckForDuplicates { get; set; }

        /// <summary>
        /// Gets or sets whether to log incoming webhook data to a file
        /// </summary>
        [JsonPropertyName("debug")]
        public bool Debug { get; set; }

        [JsonPropertyName("maxPokemonId")]
        public uint MaxPokemonId { get; set; }

        /// <summary>
        /// Gets or sets the event logging level to set
        /// </summary>
        //[JsonPropertyName("logLevel")]
        // TODO: public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the configuration file path
        /// </summary>
        [JsonIgnore]
        public string FileName { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="Config"/> class
        /// </summary>
        public Config()
        {
            ListeningHost = "127.0.0.1";
            WebhookPort = 8008;
            Locale = "en";
            MaxPokemonId = 800;
            //LogLevel = LogLevel.Trace;
            EventMinimumIV = 90;
            DespawnTimeMinimumMinutes = 5;
            ReloadSubscriptionChangesMinutes = 1;
            MaxNotificationsPerMinute = 10;
            CheckForDuplicates = true;
        }

        /// <summary>
        /// Load Discords from the `/discords` folder
        /// </summary>
        /// <returns>Returns parsed alert message</returns>
        public void LoadDiscordServers()
        {
            if (!Directory.Exists(Strings.DiscordsFolder))
            {
                // Discords folder does not exist
                return;
            }

            var dict = new Dictionary<ulong, DiscordServerConfig>();
            foreach (var (guildId, fileName) in ServerConfigFiles)
            {
                var id = ulong.Parse(guildId);
                var path = Path.Combine(Strings.DiscordsFolder, fileName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Discord server config file {path} not found.", path);
                }
                if (!dict.ContainsKey(id))
                {
                    var json = File.ReadAllText(path);
                    var config = json.FromJson<DiscordServerConfig>();
                    dict.Add(id, config);
                }
            }
            Servers = dict;
            LoadGeofences(Servers);
        }

        private static void LoadGeofences(Dictionary<ulong, DiscordServerConfig> servers)
        {
            foreach (var (serverId, serverConfig) in servers)
            {
                serverConfig.Geofences.Clear();

                var geofenceFiles = serverConfig.GeofenceFiles;
                var geofences = new List<Geofence>();

                if (geofenceFiles != null && geofenceFiles.Any())
                {
                    foreach (var file in geofenceFiles)
                    {
                        var filePath = Path.Combine(Strings.GeofenceFolder, file);

                        try
                        {
                            var fileGeofences = Geofence.FromFile(filePath);
                            geofences.AddRange(fileGeofences);
                            Console.WriteLine($"Successfully loaded {fileGeofences.Count} geofences from {file}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not load Geofence file {file} (for server {serverId}):");
                            Console.WriteLine(ex);
                        }
                    }
                }

                serverConfig.Geofences.AddRange(geofences);
            }
        }

        /// <summary>
        /// Save the current configuration object
        /// </summary>
        /// <param name="filePath">Path to save the configuration file</param>
        public void Save(string filePath)
        {
            var data = this.ToJson();
            File.WriteAllText(filePath, data);
        }

        /// <summary>
        /// Load the configuration from a file
        /// </summary>
        /// <param name="filePath">Path to load the configuration file from</param>
        /// <returns>Returns the deserialized configuration object</returns>
        public static Config Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Config not loaded because file not found.", filePath);
            }
            var config = LoadInit<Config>(filePath);
            config.LoadDiscordServers();
            return config;
        }

        public static T LoadInit<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} file not found.", filePath);
            }

            var data = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(data))
            {
                Console.WriteLine($"{filePath} masterfile is empty.");
                return default;
            }

            return data.FromJson<T>();
        }
    }
}