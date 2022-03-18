namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    using Microsoft.Extensions.Logging;

    using WhMgr.Services.Geofence;

    /// <summary>
    /// Discord server configuration class
    /// </summary>
    public class DiscordServerConfig
    {
        /// <summary>
        /// Gets or sets the bot configuration to use
        /// </summary>
        [JsonPropertyName("bot")]
        public BotConfig Bot { get; set; }

        //[JsonProperty("locale")]
        //public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the donor role ID(s)
        /// </summary>
        [JsonPropertyName("donorRoleIds")]
        public Dictionary<ulong, IEnumerable<SubscriptionAccessType>> DonorRoleIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the free donor role name to assign by non-donors to get
        /// free donor access
        /// </summary>
        [JsonPropertyName("freeRoleName")]
        public string FreeRoleName { get; set; }

        /// <summary>
        /// Gets or sets the moderators of the Discord server
        /// </summary>
        [JsonPropertyName("moderatorRoleIds")]
        public List<ulong> ModeratorRoleIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the associated alarms file for the Discord server
        /// </summary>
        [JsonPropertyName("alarms")]
        public string AlarmsFile { get; set; }

        /// <summary>
        /// Gets or sets the list of Geofence files to use for the Discord server
        /// (in addition to the common ones)
        /// </summary>
        [JsonPropertyName("geofences")]
        public string[] GeofenceFiles { get; set; }

        [JsonIgnore]
        public List<Geofence> Geofences { get; } = new();

        /// <summary>
        /// Gets or sets whether to enable custom direct message subscriptions
        /// </summary>
        [JsonPropertyName("subscriptions")]
        public SubscriptionsConfig Subscriptions { get; set; } = new();

        /// <summary>
        /// Gets or sets the GeofenceRoles config to use with assigning geofence 
        /// roles to see different sections
        /// </summary>
        [JsonPropertyName("geofenceRoles")]
        public GeofenceRolesConfig GeofenceRoles { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("questsPurge")]
        public QuestsPurgeConfig QuestsPurge { get; set; } = new();

        /// <summary>
        /// Gets or sets the nests config to use with reporting current nests
        /// </summary>
        [JsonPropertyName("nests")]
        public NestsConfig Nests { get; set; } = new();

        /// <summary>
        /// Gets or sets the daily stats configuration for nightly channel postings
        /// </summary>
        [JsonPropertyName("dailyStats")]
        public DailyStatsConfig DailyStats { get; set; } = new();

        /// <summary>
        /// Gets or sets the icon style for messages on the Discord server
        /// </summary>
        [JsonPropertyName("iconStyle")]
        public string IconStyle { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the DiscordClient minimum log level to use for the DSharpPlus
        /// internal logger (separate from the main logs)
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="DiscordServerConfig"/> class
        /// </summary>
        public DiscordServerConfig()
        {
            //Locale = "en";
            LogLevel = LogLevel.Error;
        }

        public void LoadGeofences()
        {
            Geofences.Clear();

            var geofenceFiles = GeofenceFiles;
            var geofences = new List<Geofence>();

            if (geofenceFiles != null && geofenceFiles.Any())
            {
                foreach (var file in geofenceFiles)
                {
                    var filePath = Path.Combine(Strings.GeofencesFolder, file);

                    try
                    {
                        var fileGeofences = Geofence.FromFile(filePath);
                        geofences.AddRange(fileGeofences);
                        Console.WriteLine($"Successfully loaded {fileGeofences.Count} geofences from {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not load Geofence file {file}");// (for server {serverId}):");
                        Console.WriteLine(ex);
                    }
                }
            }

            Geofences.AddRange(geofences);
        }
    }
}