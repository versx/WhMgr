﻿namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Serialization;

    using Microsoft.Extensions.Logging;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Services.Icons;

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
        /// Gets or sets the short url API config (yourls.org)
        /// </summary>
        [JsonPropertyName("shortUrlApi")]
        public UrlShortenerConfig ShortUrlApi { get; set; }

        /// <summary>
        /// Gets or sets the Stripe API config
        /// </summary>
        [JsonPropertyName("stripeApi")]
        public StripeConfig StripeApi { get; set; }

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
        /// Gets or sets the Twilio configuration
        /// </summary>
        [JsonPropertyName("twilio")]
        public TwilioConfig Twilio { get; set; } = new();

        /// <summary>
        /// Gets or sets the event specified Pokemon and filtering
        /// </summary>
        [JsonPropertyName("eventPokemon")]
        public EventPokemonConfig EventPokemon { get; set; } = new();

        /// <summary>
        /// Gets or sets the icon styles
        /// </summary>
        [JsonPropertyName("iconStyles")]
        public IconStyleCollection IconStyles { get; set; } = new();

        /// <summary>
        /// Gets or sets the static map template files to use per type
        /// </summary>
        [JsonPropertyName("staticMaps")]
        public StaticMapConfig StaticMaps { get; set; } = new();

        /// <summary>
        /// Gets or sets the reverse geocoding location lookup configuration
        /// </summary>
        [JsonPropertyName("reverseGeocoding")]
        public ReverseGeocodingConfig ReverseGeocoding { get; set; } = new();

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
        /// Gets or sets a value determining whether to check for duplicate webhook messages or not
        /// </summary>
        [JsonPropertyName("checkForDuplicates")]
        public bool CheckForDuplicates { get; set; }

        /// <summary>
        /// Gets or sets whether to log incoming webhook data to a file
        /// </summary>
        [JsonPropertyName("debug")]
        public bool Debug { get; set; }

        /// <summary>
        /// Gets a value determining the maximum Pokemon ID to support
        /// </summary>
        [JsonIgnore]
        public uint MaxPokemonId => (uint)GameMaster.Instance.Pokedex.Count;

        /// <summary>
        /// Gets or sets the event logging level to set
        /// </summary>
        [JsonPropertyName("logLevel")]
        public LogLevel LogLevel { get; set; }
        /*
         * Trace: 0
         * Debug: 1
         * Info: 2
         * Warning: 3
         * Error: 4
         * Critical: 5
         * None: 6
         */

        /// <summary>
        /// Gets or sets a value determining whether to enable Sentry tracking or not
        /// </summary>
        [JsonPropertyName("sentry")]
        public bool EnableSentry { get; set; }

        /// <summary>
        /// Gets or sets the allowed PvP league rankings to show and filter by received Pokemon
        /// </summary>
        [JsonPropertyName("pvpLeagues")]
        public Dictionary<PvpLeague, PvpLeagueConfig> PvpLeagues { get; set; }

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
            LogLevel = LogLevel.Trace;
            DespawnTimeMinimumMinutes = 5;
            CheckForDuplicates = true;
            PvpLeagues = new Dictionary<PvpLeague, PvpLeagueConfig>();
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
            foreach (var (_, serverConfig) in servers)
            {
                serverConfig.LoadGeofences();
                /*
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
                */
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
            var config = filePath.LoadFromFile<Config>();
            config.LoadDiscordServers();
            return config;
        }
    }
}