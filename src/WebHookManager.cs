namespace T
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;

    using T.Alarms;
    using T.Alarms.Filters;
    using T.Alarms.Models;
    using T.Diagnostics;
    using T.Geofence;
    using T.Net;
    using T.Net.Models;

    public class WebHookManager
    {
        #region Constants

        const string AlarmsFilePath = "alarms.json";
        const string GeofencesFolder = "Geofences";

        #endregion

        #region Variables

        private readonly HttpServer _http;
        private readonly GeofenceService _geofenceSvc;
        private AlarmList _alarms;
        private readonly Filters _filters;
        private readonly Dictionary<string, WebHookObject> _webhooks;
        private readonly IEventLogger _logger;

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, WebHookObject> WebHooks => _webhooks;

        #endregion

        #region Events

        public event EventHandler<PokemonAlarmTriggeredEventArgs> PokemonAlarmTriggered;

        private void OnPokemonAlarmTriggered(PokemonData pkmn, AlarmObject alarm)
        {
            PokemonAlarmTriggered?.Invoke(this, new PokemonAlarmTriggeredEventArgs(pkmn, alarm));
        }

        public event EventHandler<RaidAlarmTriggeredEventArgs> RaidAlarmTriggered;

        private void OnRaidAlarmTriggered(RaidData raid, AlarmObject alarm)
        {
            RaidAlarmTriggered?.Invoke(this, new RaidAlarmTriggeredEventArgs(raid, alarm));
        }

        #endregion

        #region Constructor

        public WebHookManager(ushort port)
        {
            _logger = EventLogger.GetLogger();
            _logger.Trace($"WebHookManager::WebHookManager [Port={port}]");

            _http = new HttpServer(port);
            _http.PokemonReceived += Http_PokemonReceived;
            _http.RaidReceived += Http_RaidReceived;

            _geofenceSvc = new GeofenceService();
            _filters = new Filters(_logger);
            _alarms = LoadAlarms(AlarmsFilePath);
            _webhooks = new Dictionary<string, WebHookObject>();

            LoadWebHooks();
            LoadAlarmsOnChange();
        }

        #endregion

        #region HttpServer Events

        private void Http_PokemonReceived(object sender, PokemonDataEventArgs e) => ProcessPokemon(e.Pokemon);

        private void Http_RaidReceived(object sender, RaidDataEventArgs e) => ProcessRaid(e.Raid);

        #endregion

        #region Private Methods

        private AlarmList LoadAlarms(string alarmsFilePath)
        {
            _logger.Trace($"WebHookManager::LoadAlarms [AlarmsFilePath={alarmsFilePath}]");

            if (!File.Exists(alarmsFilePath))
            {
                _logger.Error($"Failed to load file alarms file '{alarmsFilePath}'...");
                return null;
            }

            var alarmData = File.ReadAllText(alarmsFilePath);
            if (string.IsNullOrEmpty(alarmData))
            {
                _logger.Error($"Failed to load '{alarmsFilePath}', file is empty...");
                return null;
            }

            var alarms = JsonConvert.DeserializeObject<AlarmList>(alarmData);
            if (alarms == null)
            {
                _logger.Error($"Failed to deserialize the alarms file '{alarmsFilePath}', make sure you don't have any json syntax errors.");
                return null;
            }

            _logger.Info($"Alarms file {alarmsFilePath} was loaded successfully.");

            foreach (var item in alarms)
            {
                item.Value.ForEach(x => x.LoadGeofence());
                _logger.Debug($"Geofence file loaded for {item.Key}...");
            }

            return alarms;
        }

        private void LoadAlarmsOnChange()
        {
            var offset = 0L;

            var fsw = new FileSystemWatcher
            {
                Path = Environment.CurrentDirectory,
                Filter = AlarmsFilePath
            };

            var file = File.Open(
                AlarmsFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            var sr = new StreamReader(file);
            while (true)
            {
                fsw.WaitForChanged(WatcherChangeTypes.Changed);

                file.Seek(offset, SeekOrigin.Begin);
                if (!sr.EndOfStream)
                {
                    do
                    {
                        Console.WriteLine(sr.ReadLine());
                    } while (!sr.EndOfStream);

                    offset = file.Position;
                }
                else
                {
                    _logger.Info($"Alarms file {AlarmsFilePath} has changed, reloading...");
                    _alarms = LoadAlarms(AlarmsFilePath);
                }
            }
        }

        private void LoadWebHooks()
        {
            foreach (var alarm in _alarms)
            {
                foreach (var item in alarm.Value)
                {
                    if (string.IsNullOrEmpty(item.Webhook))
                        continue;

                    var wh = GetWebHookData(item.Webhook);
                    if (wh == null)
                    {
                        _logger.Error($"Failed to download webhook data from {item.Webhook}.");
                        continue;
                    }

                    if (!_webhooks.ContainsKey(item.Name))
                    {
                        _webhooks.Add(item.Name, wh);
                    }
                }
            }
        }

        private void ProcessPokemon(PokemonData pkmn)
        {
            _logger.Trace($"WebHookManager::ProcessPokemon [Pkmn={pkmn.Id}]");

            if (pkmn == null)
                return;

            if (_alarms?.Count == 0)
                return;

            pkmn.SetDespawnTime();

            foreach (var item in _alarms)
            {
                foreach (var alarm in item.Value)
                {
                    if (!InGeofence(alarm.Geofence, new Location(pkmn.Latitude, pkmn.Longitude)))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because not in geofence.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon == null)
                        continue;

                    if (!alarm.Filters.Pokemon.Enabled)
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because Pokemon filter not enabled.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Pokemon.Contains(pkmn.Id))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because of filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (!(alarm.Filters.Pokemon.FilterType == FilterType.Include && (alarm.Filters.Pokemon.Pokemon.Contains(pkmn.Id) || alarm.Filters.Pokemon?.Pokemon.Count == 0)))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because of filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.IgnoreMissing && (pkmn.Attack == "?" || pkmn.Defense == "?" || pkmn.Stamina == "?"))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because IgnoreMissing=true.");
                        continue;
                    }

                    if (!_filters.MatchesIV(pkmn.IV, Convert.ToInt32(alarm.Filters.Pokemon.MinimumIV), Convert.ToInt32(alarm.Filters.Pokemon.MaximumIV)))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because MinimumIV={alarm.Filters.Pokemon.MinimumIV} and MaximumIV={alarm.Filters.Pokemon.MaximumIV} and IV={pkmn.IV}.");
                        continue;
                    }

                     _logger.Info($"[{alarm.Geofence.Name}] Notification triggered for pokemon {pkmn.Id}.");
                     OnPokemonAlarmTriggered(pkmn, alarm);
                 }
            }
        }

        private void ProcessRaid(RaidData raid)
        {
            _logger.Trace($"WebHookManager::ProcessRaid [Raid={raid.PokemonId}]");

            if (raid == null)
                return;

            if (_alarms?.Count == 0)
                return;

            foreach (var item in _alarms)
            {
                foreach (var alarm in item.Value)
                {
                    if (!InGeofence(alarm.Geofence, new Location(raid.Latitude, raid.Longitude)))
                    {
                        _logger.Info($"[{alarm.Geofence.Name}] Skipping raid Pokemon={raid.PokemonId}, Level={raid.Level} because not in geofence.");
                        continue;
                    }

                    if (raid.IsEgg)
                    {
                        if (alarm.Filters.Raids == null)
                            continue;

                        if (!alarm.Filters.Raids.Enabled)
                        {
                            _logger.Info($"[{alarm.Geofence.Name}] Skipping level {raid.Level} raid egg because raids filter not enabled.");
                            continue;
                        }

                        if (!int.TryParse(raid.Level, out var level))
                        {
                            _logger.Error($"Failed to parse '{raid.Level}' as raid level.");
                            continue;
                        }

                        if (!(level >= alarm.Filters.Eggs.MinimumLevel && level <= alarm.Filters.Eggs.MaximumLevel))
                        {
                            _logger.Info($"[{alarm.Geofence.Name}] Skipping level {raid.Level} raid egg because '{raid.Level}' does not meet the MinimumLevel={alarm.Filters.Eggs.MinimumLevel} and MaximumLevel={alarm.Filters.Eggs.MaximumLevel} filters.");
                            continue;
                        }

                        _logger.Info($"[{alarm.Geofence.Name}] Notification triggered for level {raid.Level} raid egg.");
                        OnRaidAlarmTriggered(raid, alarm);
                    }
                    else
                    {
                        if (alarm.Filters.Raids == null)
                            continue;

                        if (!alarm.Filters.Raids.Enabled)
                        {
                            _logger.Info($"[{alarm.Geofence.Name}] Skipping raid boss {raid.PokemonId} because raids filter not enabled.");
                            continue;
                        }

                        if (!alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId))
                        {
                            _logger.Info($"[{alarm.Geofence.Name}] Skipping raid boss {raid.PokemonId} because raid boss not in include list.");
                            continue;
                        }

                        if (alarm.Filters.Raids.IgnoreMissing && (string.IsNullOrEmpty(raid.FastMove) || raid.FastMove == "?"))
                        {
                            _logger.Info($"[{alarm.Geofence.Name}] Skipping raid boss {raid.PokemonId} because IgnoreMissing=true.");
                            continue;
                        }

                        _logger.Info($"[{alarm.Geofence.Name}] Notification triggered for raid boss {raid.PokemonId}.");
                        OnRaidAlarmTriggered(raid, alarm);
                    }
                }
            }
        }

        private bool InGeofence(GeofenceItem geofence, Location location)
        {
            return _geofenceSvc.Contains(geofence, location);
        }

        #endregion

        public static WebHookObject GetWebHookData(string webHook)
        {
            /**Example:
             * {
             *   "name": "Pogo", 
             *   "channel_id": "352137087182416026", 
             *   "token": "fCdHsCZWeGB_vTkdPRqnB4_7fXil5tutXDLAZQYDurkXWQOqzSptiSQHbiCOBGlsg8J8", 
             *   "avatar": null, 
             *   "guild_id": "322025055510854680", 
             *   "id": "352156775101032439"
             * }
             */

            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                var json = wc.DownloadString(webHook);
                var data = JsonConvert.DeserializeObject<WebHookObject>(json);
                return data;
            }
        }
    }

    public class PokemonAlarmTriggeredEventArgs : EventArgs
    {
        public AlarmObject Alarm { get; }

        public PokemonData Pokemon { get; }

        public PokemonAlarmTriggeredEventArgs(PokemonData pkmn, AlarmObject alarm)
        {
            Pokemon = pkmn;
            Alarm = alarm;
        }
    }

    public class RaidAlarmTriggeredEventArgs : EventArgs
    {
        public AlarmObject Alarm { get; }

        public RaidData Raid { get; }

        public RaidAlarmTriggeredEventArgs(RaidData raid, AlarmObject alarm)
        {
            Raid = raid;
            Alarm = alarm;
        }
    }
}