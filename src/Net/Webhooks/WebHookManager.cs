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
    using WhMgr.Diagnostics;
    using WhMgr.Geofence;
    using WhMgr.Net;
    using WhMgr.Net.Models;
    using WhMgr.Utilities;

    public class WebhookManager
    {
        #region Variables

        private readonly HttpServer _http;
        private AlarmList _alarms;
        private readonly Dictionary<string, WebHookObject> _webhooks;
        private readonly IEventLogger _logger;

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, WebHookObject> WebHooks => _webhooks;

        public GeofenceService GeofenceService { get; }

        public Dictionary<string, GeofenceItem> Geofences { get; private set; }

        public Filters Filters { get; }

        #endregion

        #region Events

        #region Alarms

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

        public event EventHandler<QuestAlarmTriggeredEventArgs> QuestAlarmTriggered;
        private void OnQuestAlarmTriggered(QuestData quest, AlarmObject alarm)
        {
            QuestAlarmTriggered?.Invoke(this, new QuestAlarmTriggeredEventArgs(quest, alarm));
        }

        public event EventHandler<GymAlarmTriggeredEventArgs> GymAlarmTriggered;
        private void OnGymAlarmTriggered(GymData gym, AlarmObject alarm)
        {
            GymAlarmTriggered?.Invoke(this, new GymAlarmTriggeredEventArgs(gym, alarm));
        }

        public event EventHandler<GymDetailsAlarmTriggeredEventArgs> GymDetailsAlarmTriggered;
        private void OnGymDetailsAlarmTriggered(GymDetailsData gymDetails, AlarmObject alarm)
        {
            GymDetailsAlarmTriggered?.Invoke(this, new GymDetailsAlarmTriggeredEventArgs(gymDetails, alarm));
        }

        public event EventHandler<PokestopAlarmTriggeredEventArgs> PokestopAlarmTriggered;
        private void OnPokestopAlarmTriggered(PokestopData pokestop, AlarmObject alarm)
        {
            PokestopAlarmTriggered?.Invoke(this, new PokestopAlarmTriggeredEventArgs(pokestop, alarm));
        }

        #endregion

        #region Subscriptions

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

        public event EventHandler<QuestData> QuestSubscriptionTriggered;
        private void OnQuestSubscriptionTriggered(QuestData quest)
        {
            QuestSubscriptionTriggered?.Invoke(this, quest);
        }

        #endregion

        #endregion

        #region Constructor

        public WebhookManager(ushort port)
        {
            Filters = new Filters();
            Geofences = new Dictionary<string, GeofenceItem>();

            _logger = EventLogger.GetLogger();
            _logger.Trace($"WebHookManager::WebHookManager [Port={port}]");

            _webhooks = new Dictionary<string, WebHookObject>();
            GeofenceService = new GeofenceService();
            _alarms = LoadAlarms(Strings.AlarmsFileName);

            LoadWebHooks();

            _http = new HttpServer(port);
            _http.PokemonReceived += Http_PokemonReceived;
            _http.RaidReceived += Http_RaidReceived;
            _http.QuestReceived += Http_QuestReceived;
            _http.PokestopReceived += Http_PokestopReceived;
            _http.GymReceived += Http_GymReceived;
            _http.GymDetailsReceived += Http_GymDetailsReceived;
            _http.IsDebug = true;
            _http.Start();

            new System.Threading.Thread(LoadAlarmsOnChange).Start();
        }

        #endregion

        #region HttpServer Events

        private void Http_PokemonReceived(object sender, PokemonDataEventArgs e)
        {
            if (DateTime.Now > e.Pokemon.DespawnTime)
            {
                _logger.Debug($"Pokemon {e.Pokemon.Id} already despawned at {e.Pokemon.DespawnTime}");
                return;
            }

            ProcessPokemon(e.Pokemon);
            OnPokemonSubscriptionTriggered(e.Pokemon);
        }

        private void Http_RaidReceived(object sender, RaidDataEventArgs e)
        {
            if (DateTime.Now > e.Raid.EndTime)
            {
                _logger.Debug($"Raid boss {e.Raid.PokemonId} already despawned at {e.Raid.EndTime}");
                return;
            }

            ProcessRaid(e.Raid);
            OnRaidSubscriptionTriggered(e.Raid);
        }

        private void Http_QuestReceived(object sender, QuestDataEventArgs e)
        {
            ProcessQuest(e.Quest);
            OnQuestSubscriptionTriggered(e.Quest);
        }

        private void Http_PokestopReceived(object sender, PokestopDataEventArgs e)
        {
        }

        private void Http_GymReceived(object sender, GymDataEventArgs e)
        {
        }

        private void Http_GymDetailsReceived(object sender, GymDetailsDataEventArgs e)
        {
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

            alarms.Alarms.ForEach(x =>
            {
                var geofences = x.LoadGeofence();
                for (var i = 0; i < geofences.Count; i++)
                {
                    var geofence = geofences[i];
                    if (!Geofences.ContainsKey(geofence.Name))
                    {
                        Geofences.Add(geofence.Name, geofence);
                        _logger.Debug($"Geofence file loaded for {x.Name}...");
                    }
                }
            });

            return alarms;
        }

        private void LoadAlarmsOnChange()
        {
            _logger.Trace($"WebHookManager::LoadAlarmsOnChange");

            var path = Path.Combine(Directory.GetCurrentDirectory(), Strings.AlarmsFileName);
            var fileWatcher = new FileWatcher(path);
            fileWatcher.FileChanged += (sender, e) => _alarms = LoadAlarms(path);
            fileWatcher.Start();
        }

        private void LoadWebHooks()
        {
            _logger.Trace($"WebHookManager::LoadWebHooks");

            foreach (var alarm in _alarms.Alarms)
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

            if (!_alarms.EnablePokemon)
                return;

            if (pkmn == null)
                return;

            if (_alarms.Alarms?.Count == 0)
                return;

            for (var i = 0; i < _alarms.Alarms.Count; i++)
            {
                var alarm = _alarms.Alarms[i];
                if (alarm.Filters.Pokemon == null)
                    continue;

                if (!alarm.Filters.Pokemon.Enabled)
                {
                    _logger.Info($"[{alarm.Name}] Skipping pokemon {pkmn.Id} because Pokemon filter not enabled.");
                    continue;
                }

                var geofence = InGeofence(alarm.Geofences, new Location(pkmn.Latitude, pkmn.Longitude));
                if (geofence == null)
                {
                    _logger.Info($"[{alarm.Name}] Skipping pokemon {pkmn.Id} because not in geofence.");
                    continue;
                }

                if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Pokemon.Contains(pkmn.Id))
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                    continue;
                }

                if (!(alarm.Filters.Pokemon.FilterType == FilterType.Include && (alarm.Filters.Pokemon.Pokemon.Contains(pkmn.Id) || alarm.Filters.Pokemon.Pokemon.Count == 0)))
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                    continue;
                }

                if (alarm.Filters.Pokemon.IgnoreMissing && pkmn.IsMissingStats)
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: IgnoreMissing=true.");
                    continue;
                }

                if (!Filters.MatchesIV(pkmn.IV, Convert.ToInt32(alarm.Filters.Pokemon.MinimumIV), Convert.ToInt32(alarm.Filters.Pokemon.MaximumIV)))
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumIV={alarm.Filters.Pokemon.MinimumIV} and MaximumIV={alarm.Filters.Pokemon.MaximumIV} and IV={pkmn.IV}.");
                    continue;
                }

                OnPokemonAlarmTriggered(pkmn, alarm);
                _logger.Info($"[{alarm.Name}] [{geofence.Name}] Notification triggered for pokemon {pkmn.Id}.");
            }
        }

        private void ProcessRaid(RaidData raid)
        {
            _logger.Trace($"WebHookManager::ProcessRaid [Raid={raid.PokemonId}]");

            if (!_alarms.EnableRaids)
                return;

            if (raid == null)
                return;

            if (_alarms.Alarms?.Count == 0)
                return;

            for (var i = 0; i < _alarms.Alarms.Count; i++)
            {
                var alarm = _alarms.Alarms[i];
                var geofence = InGeofence(alarm.Geofences, new Location(raid.Latitude, raid.Longitude));
                if (geofence == null)
                {
                    _logger.Info($"[{alarm.Name}] Skipping raid Pokemon={raid.PokemonId}, Level={raid.Level} because not in geofence.");
                    continue;
                }

                if (raid.IsEgg)
                {
                    if (alarm.Filters.Eggs == null)
                        continue;

                    if (!alarm.Filters.Eggs.Enabled)
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: raids filter not enabled.");
                        continue;
                    }

                    if (!int.TryParse(raid.Level, out var level))
                    {
                        _logger.Error($"[{alarm.Name}] [{geofence.Name}] Failed to parse '{raid.Level}' as raid level.");
                        continue;
                    }

                    if (!(level >= alarm.Filters.Eggs.MinimumLevel && level <= alarm.Filters.Eggs.MaximumLevel))
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: '{raid.Level}' does not meet the MinimumLevel={alarm.Filters.Eggs.MinimumLevel} and MaximumLevel={alarm.Filters.Eggs.MaximumLevel} filters.");
                        continue;
                    }

                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Notification triggered for level {raid.Level} raid egg.");
                    OnRaidAlarmTriggered(raid, alarm);
                }
                else
                {
                    if (alarm.Filters.Raids == null)
                        continue;

                    if (!alarm.Filters.Raids.Enabled)
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: raids filter not enabled.");
                        continue;
                    }

                    if (alarm.Filters.Raids.FilterType == FilterType.Exclude && alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId))
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                        continue;
                    }

                    //if (alarm.Filters.Raids.FilterType == FilterType.Exclude && alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId) && alarm.Filters.Raids.Pokemon?.Count > 0)
                    //{
                    //    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                    //    continue;
                    //}

                    if (alarm.Filters.Raids.FilterType == FilterType.Include && (!alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId) && alarm.Filters.Raids.Pokemon?.Count > 0))
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Raids.IgnoreMissing && (string.IsNullOrEmpty(raid.FastMove) || raid.FastMove == "?"))
                    {
                        _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: IgnoreMissing=true.");
                        continue;
                    }

                    OnRaidAlarmTriggered(raid, alarm);
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Notification triggered for raid boss {raid.PokemonId}.");
                }
            }
        }

        private void ProcessQuest(QuestData quest)
        {
            _logger.Trace($"WebhookManager::ProcessQuest [Quest={quest.PokestopId}]");

            if (!_alarms.EnableQuests)
                return;

            if (quest == null)
                return;

            if (_alarms.Alarms?.Count == 0)
                return;

            var rewardKeyword = quest.GetRewardString();

            for (var i = 0; i < _alarms.Alarms.Count; i++)
            {
                var alarm = _alarms.Alarms[i];
                if (alarm.Filters.Quests == null)
                    continue;

                if (!alarm.Filters.Quests.Enabled)
                {
                    _logger.Info($"[{alarm.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: quests filter not enabled.");
                    continue;
                }

                var geofence = InGeofence(alarm.Geofences, new Location(quest.Latitude, quest.Longitude));
                if (geofence == null)
                {
                    _logger.Info($"[{alarm.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: not in geofence.");
                    continue;
                }

                var contains = alarm.Filters.Quests.RewardKeywords.Select(x => x.ToLower()).Contains(rewardKeyword.ToLower());
                if (alarm.Filters.Quests.FilterType == FilterType.Exclude && contains)
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: filter {alarm.Filters.Quests.FilterType}.");
                    continue;
                }

                if (!(alarm.Filters.Quests.FilterType == FilterType.Include && (contains || alarm.Filters.Quests?.RewardKeywords.Count == 0)))
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}: filter {alarm.Filters.Quests.FilterType}.");
                    continue;
                }

                if (!contains && alarm.Filters?.Quests?.RewardKeywords?.Count > 0)
                {
                    _logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: rewards does not match reward keywords.");
                    continue;
                }

                _logger.Info($"[{alarm.Name}] [{geofence.Name}] Notification triggered for PokestopId={quest.PokestopId}, Type={quest.Type}.");
                OnQuestAlarmTriggered(quest, alarm);
            }
        }

        private GeofenceItem InGeofence(List<GeofenceItem> geofences, Location location)
        {
            for (var i = 0; i < geofences.Count; i++)
            {
                var geofence = geofences[i];
                if (!GeofenceService.Contains(geofence, location))
                    continue;

                return geofence;
            }

            return null;
        }

        #endregion

        #region Static Methods

        public static WebHookObject GetWebHookData(string webHook)
        {
            /**Example:
             * {
             *   "name": "", 
             *   "channel_id": "", 
             *   "token": "", 
             *   "avatar": null, 
             *   "guild_id": "", 
             *   "id": ""
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
}