namespace WhMgr.Services.Alarms.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;

    public class ChannelAlarmsManifest
    {
        //private readonly ILogger<ChannelAlarmsManifest> _logger;
        private static readonly object _geofencesLock = new();

        [JsonPropertyName("enablePokemon")]
        public bool EnablePokemon { get; set; }

        [JsonPropertyName("enableRaids")]
        public bool EnableRaids { get; set; }

        [JsonPropertyName("enableQuests")]
        public bool EnableQuests { get; set; }

        [JsonPropertyName("enablePokestops")]
        public bool EnablePokestops { get; set; }

        [JsonPropertyName("enableGyms")]
        public bool EnableGyms { get; set; }

        [JsonPropertyName("enableWeather")]
        public bool EnableWeather { get; set; }

        [JsonPropertyName("alarms")]
        public List<ChannelAlarm> Alarms { get; set; }

        public ChannelAlarmsManifest()
        {
            //_logger = LoggerFactory.Create(configure => configure.AddConsole())
            //    .CreateLogger<ChannelAlarmsManifest>();
            Alarms = new List<ChannelAlarm>();
        }

        public static Dictionary<ulong, ChannelAlarmsManifest> LoadAlarms(Dictionary<ulong, DiscordServerConfig> servers)
        {
            var alarms = new Dictionary<ulong, ChannelAlarmsManifest>();
            foreach (var (serverId, serverConfig) in servers)
            {
                var serverAlarms = LoadAlarms(serverId, serverConfig.AlarmsFile, servers);
                alarms.Add(serverId, serverAlarms);
            }
            return alarms;
        }

        private static ChannelAlarmsManifest LoadAlarms(ulong forGuildId, string alarmsFilePath, Dictionary<ulong, DiscordServerConfig> servers)
        {
            Console.WriteLine($"ChannelAlarms::LoadAlarms [ForGuildId={forGuildId}, AlarmsFilePath={alarmsFilePath}]");

            var alarmsFolder = Path.Combine(Directory.GetCurrentDirectory(), Strings.AlarmsFolder);
            var alarmPath = Path.Combine(alarmsFolder, alarmsFilePath);
            if (!File.Exists(alarmPath))
            {
                Console.WriteLine($"Failed to load file alarms file '{alarmPath}' file does not exist...");
                return null;
            }

            var alarmData = File.ReadAllText(alarmPath);
            if (string.IsNullOrEmpty(alarmData))
            {
                Console.WriteLine($"Failed to load '{alarmPath}', file is empty...");
                return null;
            }

            var alarms = alarmData.FromJson<ChannelAlarmsManifest>();
            if (alarms == null)
            {
                Console.WriteLine($"Failed to deserialize the alarms file '{alarmPath}', make sure you don't have any json syntax errors.");
                return null;
            }

            Console.WriteLine($"Alarms file {alarmPath} was loaded successfully.");
            foreach (var alarm in alarms.Alarms)
            {
                if (alarm.Geofences != null)
                {
                    foreach (var geofenceName in alarm.Geofences)
                    {
                        lock (_geofencesLock)
                        {
                            // First try and find loaded geofences for this server by name or filename (so we don't have to parse already loaded files again)
                            var server = servers[forGuildId];
                            var geofences = server.Geofences.Where(g => g.Name.Equals(geofenceName, StringComparison.OrdinalIgnoreCase) ||
                                                                        g.Filename.Equals(geofenceName, StringComparison.OrdinalIgnoreCase)).ToList();

                            if (geofences.Any())
                            {
                                alarm.GeofenceItems.AddRange(geofences);
                            }
                            else
                            {
                                // Try and load from a file instead
                                var filePath = Path.Combine(Strings.GeofenceFolder, geofenceName);

                                if (!File.Exists(filePath))
                                {
                                    Console.WriteLine($"Could not find Geofence file \"{geofenceName}\" for alarm \"{alarm.Name}\"");
                                    continue;
                                }

                                var fileGeofences = Geofence.FromFile(filePath);

                                alarm.GeofenceItems.AddRange(fileGeofences);
                                Console.WriteLine($"Successfully loaded {fileGeofences.Count} geofences from {geofenceName}");
                            }
                        }
                    }
                }

                alarm.LoadEmbeds();
                alarm.LoadFilters();
            }

            return alarms;
        }
    }
}