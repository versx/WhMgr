namespace WhMgr.Net.Webhooks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json;

    using WhMgr.Alarms;
    using WhMgr.Alarms.Filters;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Diagnostics;
    using WhMgr.Geofence;
    using WhMgr.Net;
    using WhMgr.Net.Models;
    using WhMgr.Net.Models.Providers;

    public class WebhookManager
    {
        #region Variables

        private readonly HttpServer _http;
        private readonly GeofenceService _geofenceSvc;
        private AlarmList _alarms;
        private readonly Dictionary<string, WebHookObject> _webhooks;
        private readonly IEventLogger _logger;

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, WebHookObject> WebHooks => _webhooks;

        public GeofenceService GeofenceService => _geofenceSvc;

        public List<GeofenceItem> Geofences { get; private set; }

        public Filters Filters { get; }

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

        public event EventHandler<PokemonData> PokemonSubscriptionTriggered;

        private void OnPokemonSubscriptionTriggered(PokemonData pkmn)
        {
            PokemonSubscriptionTriggered?.Invoke(this, pkmn);
        }

        public event EventHandler<RaidData> RaidSubscriptionTriggered;
        private void OnRaidSubscriptionTriggered(RaidData raid)
        {
            RaidSubscriptionTriggered?.Invoke(this, raid);
        }

        #endregion

        #region Constructor

        public WebhookManager(ushort port, MapProviderType provider, MapProviderFork fork)
        {
            Filters = new Filters();
            Geofences = new List<GeofenceItem>();

            _logger = EventLogger.GetLogger();
            _logger.Trace($"WebHookManager::WebHookManager [Port={port}]");

            _webhooks = new Dictionary<string, WebHookObject>();
            _geofenceSvc = new GeofenceService();
            _alarms = LoadAlarms(Strings.AlarmsFileName);

            LoadWebHooks();

            _http = new HttpServer(port, provider, fork);
            _http.PokemonReceived += Http_PokemonReceived;
            _http.RaidReceived += Http_RaidReceived;
            _http.Start();

            new System.Threading.Thread(LoadAlarmsOnChange).Start();
        }

        #endregion

        #region HttpServer Events

        private void Http_PokemonReceived(object sender, PokemonDataEventArgs e)
        {
            ProcessPokemon(e.Pokemon);
            OnPokemonSubscriptionTriggered(e.Pokemon);
        }

        private void Http_RaidReceived(object sender, RaidDataEventArgs e)
        {
            ProcessRaid(e.Raid);
            OnRaidSubscriptionTriggered(e.Raid);
        }

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

            alarms.ForEach(x =>
            {
                var geofence = x.LoadGeofence();
                //TODO: Fix geofence reload issue, find distinct way.
                if (!Geofences.Contains(geofence))
                {
                    Geofences.Add(geofence);
                }

                _logger.Debug($"Geofence file loaded for {x.Name}...");
            });

            return alarms;
        }

        private void LoadAlarmsOnChange()
        {
            _logger.Trace($"WebHookManager::LoadAlarmsOnChange");

            var path = Path.Combine(Directory.GetCurrentDirectory(), Strings.AlarmsFileName);
            var fileWatcher = new FileWatcher(path);
            fileWatcher.FileChanged += (sender, e) => _alarms = LoadAlarms(Strings.AlarmsFileName);
            fileWatcher.Start();
        }

        private void LoadWebHooks()
        {
            _logger.Trace($"WebHookManager::LoadWebHooks");

            foreach (var alarm in _alarms)
            {
                if (string.IsNullOrEmpty(alarm.Webhook))
                    continue;

                var wh = GetWebHookData(alarm.Webhook);
                if (wh == null)
                {
                    _logger.Error($"Failed to download webhook data from {alarm.Webhook}.");
                    continue;
                }

                if (!_webhooks.ContainsKey(alarm.Name))
                {
                    _webhooks.Add(alarm.Name, wh);
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

            for (var i = 0; i < _alarms.Count; i++)
            {
                var alarm = _alarms[i];
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

                if (!Filters.MatchesIV(pkmn.IV, Convert.ToInt32(alarm.Filters.Pokemon.MinimumIV), Convert.ToInt32(alarm.Filters.Pokemon.MaximumIV)))
                {
                    _logger.Info($"[{alarm.Geofence.Name}] Skipping pokemon {pkmn.Id} because MinimumIV={alarm.Filters.Pokemon.MinimumIV} and MaximumIV={alarm.Filters.Pokemon.MaximumIV} and IV={pkmn.IV}.");
                    continue;
                }

                OnPokemonAlarmTriggered(pkmn, alarm);
                _logger.Info($"[{alarm.Geofence.Name}] Notification triggered for pokemon {pkmn.Id}.");
            }
        }

        private void ProcessRaid(RaidData raid)
        {
            _logger.Trace($"WebHookManager::ProcessRaid [Raid={raid.PokemonId}]");

            if (raid == null)
                return;

            if (_alarms?.Count == 0)
                return;

            for (var i = 0; i < _alarms.Count; i++)
            {
                var alarm = _alarms[i];
                if (!InGeofence(alarm.Geofence, new Location(raid.Latitude, raid.Longitude)))
                {
                    _logger.Info($"[{alarm.Geofence.Name}] Skipping raid Pokemon={raid.PokemonId}, Level={raid.Level} because not in geofence.");
                    continue;
                }

                if (raid.IsEgg)
                {
                    if (alarm.Filters.Eggs == null)
                        continue;

                    if (!alarm.Filters.Eggs.Enabled)
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

                    OnRaidAlarmTriggered(raid, alarm);
                    _logger.Info($"[{alarm.Geofence.Name}] Notification triggered for raid boss {raid.PokemonId}.");
                }
            }
        }

        private bool InGeofence(GeofenceItem geofence, Location location)
        {
            return _geofenceSvc.Contains(geofence, location);
        }

        #endregion

        #region Static Methods

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

        #endregion
    }

    public class FileWatcher
    {
        private readonly FileSystemWatcher _fsw;

        public string FilePath { get; }

        public event EventHandler<FileChangeEventArgs> FileChanged;

        private void OnFileChanged(string filePath)
        {
            FileChanged?.Invoke(this, new FileChangeEventArgs(filePath));
        }

        public FileWatcher(string filePath)
        {
            FilePath = filePath;

            _fsw = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(FilePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = Path.GetFileName(FilePath)
            };
            _fsw.Changed += (sender, e) => OnFileChanged(e.FullPath);
        }

        public void Start()
        {
            _fsw.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _fsw.EnableRaisingEvents = false;
        }
    }

    public class FileChangeEventArgs
    {
        public string FilePath { get; set; }

        public FileChangeEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}