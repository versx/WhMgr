namespace WhMgr.Services.Webhook
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    //using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook.Cache;
    using WhMgr.Services.Webhook.Models;

    /// <summary>
    /// Parses incoming webhook data and distributes to alarm and subscription processors
    /// </summary>
    public class WebhookProcessorService : IWebhookProcessorService
    {
        private const uint ClearCacheInterval = 60000 * 15; // Every 15 minutes

        private readonly Microsoft.Extensions.Logging.ILogger<WebhookProcessorService> _logger;
        private readonly ConfigHolder _config;
        private readonly IAlarmControllerService _alarmsService;
        private readonly ISubscriptionProcessorService _subscriptionsService;
        private readonly IMapDataCache _mapDataCache;

        private readonly Dictionary<string, ScannedPokemon> _processedPokemon;
        private readonly Dictionary<string, ScannedRaid> _processedRaids;
        private readonly Dictionary<string, ScannedQuest> _processedQuests;
        private readonly Dictionary<string, ScannedPokestop> _processedPokestops;
        private readonly Dictionary<string, ScannedIncident> _processedInvasions;
        private readonly Dictionary<string, ScannedGym> _processedGyms;
        private readonly Dictionary<long, ScannedWeather> _processedWeather;
        private readonly System.Timers.Timer _clearCache;

        #region Properties

        public bool Enabled { get; private set; }

        public bool CheckForDuplicates { get; set; }

        public ushort DespawnTimerMinimumMinutes { get; set; }

        public bool Debug { get; set; }

        #endregion

        public WebhookProcessorService(
            Microsoft.Extensions.Logging.ILogger<WebhookProcessorService> logger,
            ConfigHolder config,
            IAlarmControllerService alarmsService,
            ISubscriptionProcessorService subscriptionsService,
            IMapDataCache mapDataCache)
        {
            _logger = logger;
            _config = config;
            _alarmsService = alarmsService;
            _subscriptionsService = subscriptionsService;
            _mapDataCache = mapDataCache;

            _processedPokemon = new();
            _processedRaids = new();
            _processedQuests = new();
            _processedPokestops = new();
            _processedInvasions = new();
            _processedGyms = new();
            _processedWeather = new();

            _clearCache = new System.Timers.Timer
            {
                Interval = ClearCacheInterval,
            };
            _clearCache.Elapsed += (sender, e) => OnClearCache();

            CheckForDuplicates = _config.Instance.CheckForDuplicates;
            Debug = _config.Instance.Debug;
            DespawnTimerMinimumMinutes = _config.Instance.DespawnTimeMinimumMinutes;
        }

        #region Public Methods

        public void Start()
        {
            Enabled = true;

            // Start cache cleaning timer
            if (!_clearCache.Enabled)
            {
                _clearCache.Start();
            }
        }

        public void Stop()
        {
            Enabled = false;

            // Stop cache cleaning timer
            if (_clearCache?.Enabled ?? false)
            {
                _clearCache.Stop();
            }
        }

        public async Task ParseDataAsync(List<WebhookPayload> payloads)
        {
            if (!Enabled) return;

            if (Debug)
            {
                var json = payloads?.ToJson();
                if (!string.IsNullOrEmpty(json))
                {
                    var path = Path.Combine(Strings.BasePath, Strings.DebugLogFileName);
                    using var sw = new StreamWriter(path, true, Encoding.UTF8);
                    sw.WriteLine(json);
                }
            }

            _logger.Information($"Received {payloads.Count:N0} webhook payloads");
            for (var i = 0; i < payloads.Count; i++)
            {
                var payload = payloads[i];
                switch (payload.Type)
                {
                    case WebhookTypes.Pokemon:
                        await ProcessPokemonAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Raid:
                        await ProcessRaidAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Quest:
                    case WebhookTypes.AlternativeQuest:
                        await ProcessQuestAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Invasion:
                        await ProcessInvasionAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Pokestop:
                        await ProcessPokestopAsync(payload.Message).ConfigureAwait(false);
                        break;
                    // TODO: Do not parse `gym` webhook type as GymDetailsData, property keys do not match
                    //case WebhookTypes.Gym:
                    case WebhookTypes.GymDetails:
                        ProcessGym(payload.Message);
                        break;
                    case WebhookTypes.Weather:
                        ProcessWeather(payload.Message);
                        break;
                    case WebhookTypes.Account:
                        // TODO: ProcessAccount(payload.Message);
                        break;
                    default:
                        _logger.Warning($"Unhandled webhook type: {payload.Type}: {payload.Message}");
                        break;
                }
            }
        }

        #endregion

        #region Processing Methods

        private async Task ProcessPokemonAsync(dynamic message)
        {
            string json = Convert.ToString(message);
            var pokemon = json.FromJson<PokemonData>();
            if (pokemon == null)
            {
                _logger.Warning($"Failed to deserialize pokemon {message}, skipping...");
                return;
            }
            pokemon.SetTimes();

            // Check if Pokemon despawn timer has at least the specified minimum minutes
            // remaining, otherwise skip...
            if (pokemon.SecondsLeft.TotalMinutes < DespawnTimerMinimumMinutes)
                return;

            // Check if event Pokemon filtering enabled and event Pokemon list is set
            if (!CanProceed(pokemon))
                return;

            if (CheckForDuplicates)
            {
                // Lock processed pokemon, check for duplicates of incoming pokemon
                lock (_processedPokemon)
                {
                    // If we have already processed pokemon, previously did not have stats, and currently does
                    // not have stats, skip.
                    if (_processedPokemon.ContainsKey(pokemon.EncounterId)
                        && (pokemon.IsMissingStats
                        || (!pokemon.IsMissingStats && !_processedPokemon[pokemon.EncounterId].IsMissingStats)))
                        return;

                    // Check if we have not processed this encounter before, is so then add
                    if (!_processedPokemon.ContainsKey(pokemon.EncounterId))
                        _processedPokemon.Add(pokemon.EncounterId, new ScannedPokemon(pokemon));

                    // Check if incoming pokemon has stats but previously processed pokemon did not and update it
                    if (!pokemon.IsMissingStats && _processedPokemon[pokemon.EncounterId].IsMissingStats)
                        _processedPokemon[pokemon.EncounterId] = new ScannedPokemon(pokemon);
                }
            }

            // Process pokemon alarms
            _alarmsService.ProcessPokemonAlarms(pokemon);

            // Process pokemon subscriptions
            await _subscriptionsService.ProcessPokemonSubscriptionAsync(pokemon).ConfigureAwait(false);

            // Only process pvp subscriptions if great or ultra league ranks set
            if (pokemon.HasPvpRankings)
            {
                await _subscriptionsService.ProcessPvpSubscriptionAsync(pokemon).ConfigureAwait(false);
            }
        }

        private async Task ProcessRaidAsync(dynamic message)
        {
            string json = Convert.ToString(message);
            var raid = json.FromJson<RaidData>();
            if (raid == null)
            {
                _logger.Warning($"Failed to deserialize raid {message}, skipping...");
                return;
            }
            raid.SetTimes();

            if (CheckForDuplicates)
            {
                // Lock processed raids, check for duplicates of incoming raid
                lock (_processedRaids)
                {
                    if (_processedRaids.ContainsKey(raid.GymId))
                    {
                        // Check if raid data matches existing scanned raids with
                        // pokemon_id, form_id, costume_id, and not expired
                        if (_processedRaids[raid.GymId].PokemonId == raid.PokemonId
                            && _processedRaids[raid.GymId].FormId == raid.FormId
                            && _processedRaids[raid.GymId].CostumeId == raid.CostumeId
                            && _processedRaids[raid.GymId].Level == raid.Level
                            && !_processedRaids[raid.GymId].IsExpired)
                        {
                            // Processed raid already
                            return;
                        }

                        _processedRaids[raid.GymId] = new ScannedRaid(raid);
                    }
                    else
                    {
                        _processedRaids.Add(raid.GymId, new ScannedRaid(raid));
                    }
                }
            }

            // Process raid alarms
            _alarmsService.ProcessRaidAlarms(raid);
            await _subscriptionsService.ProcessRaidSubscriptionAsync(raid).ConfigureAwait(false);
        }

        private async Task ProcessQuestAsync(dynamic message)
        {
            string json = Convert.ToString(message);
            var quest = json.FromJson<QuestData>();
            if (quest == null)
            {
                _logger.Warning($"Failed to deserialize quest {message}, skipping...");
                return;
            }

            if (CheckForDuplicates)
            {
                // Lock processed quests, check for duplicates of incoming quest
                lock (_processedQuests)
                {
                    if (_processedQuests.ContainsKey(quest.PokestopId))
                    {
                        if (_processedQuests[quest.PokestopId].Type == quest.Type
                            && !_processedQuests[quest.PokestopId].IsExpired)
                        {
                            // Processed quest already
                            return;
                        }

                        _processedQuests[quest.PokestopId] = new ScannedQuest(quest);
                    }
                    else
                    {
                        _processedQuests.Add(quest.PokestopId, new ScannedQuest(quest));
                    }
                }
            }

            // Process quest alarms
            _alarmsService.ProcessQuestAlarms(quest);
            await _subscriptionsService.ProcessQuestSubscriptionAsync(quest).ConfigureAwait(false);
        }

        private async Task ProcessPokestopAsync(dynamic message)
        {
            string json = Convert.ToString(message);
            var pokestop = json.FromJson<PokestopData>();
            if (pokestop == null)
            {
                _logger.Warning($"Failed to deserialize pokestop {message}, skipping...");
                return;
            }
            pokestop.SetTimes();

            if (CheckForDuplicates)
            {
                // Lock processed pokestops, check for duplicates of incoming pokestop
                lock (_processedPokestops)
                {
                    if (_processedPokestops.ContainsKey(pokestop.FortId))
                    {
                        var processedLureAlready = _processedPokestops[pokestop.FortId].LureType == pokestop.LureType
                            && _processedPokestops[pokestop.FortId].LureExpireTime == pokestop.LureExpireTime;

                        if (processedLureAlready)
                        {
                            // Processed pokestop lure already
                            return;
                        }

                        _processedPokestops[pokestop.FortId] = new ScannedPokestop(pokestop);
                    }
                    else
                    {
                        _processedPokestops.Add(pokestop.FortId, new ScannedPokestop(pokestop));
                    }
                }
            }

            // Process pokestop alarms
            _alarmsService.ProcessPokestopAlarms(pokestop);

            // Process lure subscriptions
            if (pokestop.HasLure)
            {
                await _subscriptionsService.ProcessLureSubscriptionAsync(pokestop).ConfigureAwait(false);
            }
        }

        private async Task ProcessInvasionAsync(dynamic message)
        {
            string json = Convert.ToString(message);
            var invasion = json.FromJson<IncidentData>();
            if (invasion == null)
            {
                _logger.Warning($"Failed to deserialize incident {message}, skipping...");
                return;
            }
            invasion.SetTimes();

            if (CheckForDuplicates)
            {
                // Lock processed pokestops, check for duplicates of incoming incident
                lock (_processedInvasions)
                {
                    if (_processedInvasions.ContainsKey(invasion.Id))
                    {
                        if (_processedInvasions[invasion.Id].Character == invasion.Character &&
                            _processedInvasions[invasion.Id].ExpireTime == invasion.ExpirationTime)
                        {
                            // Processed pokestop invasion already
                            return;
                        }

                        _processedInvasions[invasion.Id] = new ScannedIncident(invasion);
                    }
                    else
                    {
                        _processedInvasions.Add(invasion.Id, new ScannedIncident(invasion));
                    }
                }
            }

            // Process invasion alarms
            _alarmsService.ProcessInvasionAlarms(invasion);

            // Process invasion subscriptions
            await _subscriptionsService.ProcessInvasionSubscriptionAsync(invasion).ConfigureAwait(false);
        }

        private void ProcessGym(dynamic message)
        {
            string json = Convert.ToString(message);
            var gym = json.FromJson<GymDetailsData>();
            if (gym == null)
            {
                _logger.Warning($"Failed to deserialize gym {message}, skipping...");
            }
            gym.SetTimes();

            if (CheckForDuplicates)
            {
                // Lock process gyms, check for duplicates of incoming gym
                lock (_processedGyms)
                {
                    if (string.IsNullOrEmpty(gym?.FortId)) {
                        // Skip gyms with no ID set
                        return;
                    }
                    if (_processedGyms.ContainsKey(gym.FortId))
                    {
                        if (_processedGyms[gym.FortId].Team == gym.Team
                            && _processedGyms[gym.FortId].SlotsAvailable == gym.SlotsAvailable
                            && _processedGyms[gym.FortId].InBattle == gym.InBattle)
                        {
                            // Gym already processed
                            return;
                        }

                        _processedGyms[gym.FortId] = new ScannedGym(gym);
                    }
                    else
                    {
                        _processedGyms.Add(gym.FortId, new ScannedGym(gym));
                    }
                }
            }

            // Process gym alarms
            _alarmsService.ProcessGymAlarms(gym);
        }

        private void ProcessWeather(dynamic message)
        {
            string json = Convert.ToString(message);
            var weather = json.FromJson<WeatherData>();
            if (weather == null)
            {
                _logger.Warning($"Failed to deserialize weather {message}, skipping...");
            }
            weather.SetTimes();

            if (CheckForDuplicates)
            {
                lock (_processedWeather)
                {
                    if (_processedWeather.ContainsKey(weather.Id))
                    {
                        if (_processedWeather[weather.Id].Condition == weather.GameplayCondition &&
                            !_processedWeather[weather.Id].IsExpired)
                        {
                            // Processed weather already
                            return;
                        }

                        _processedWeather[weather.Id] = new ScannedWeather(weather);
                    }
                    else
                    {
                        _processedWeather.Add(weather.Id, new ScannedWeather(weather));
                    }
                }
            }

            // Process weather alarms
            _alarmsService.ProcessWeatherAlarms(weather);
        }

        private void ProcessAccount(dynamic message)
        {
            string json = Convert.ToString(message);
            var account = json.FromJson<AccountData>();
            if (account == null)
            {
                _logger.Warning($"Failed to deserialize account {message}, skipping...");
            }
            account.SetTimes();

            if (CheckForDuplicates)
            {
            }

            // Process account alarms
            _alarmsService.ProcessAccountAlarms(account);
        }

        #endregion

        private void OnClearCache()
        {
            lock (_processedPokemon)
            {
                var expiredEncounters = _processedPokemon.Where(pair => pair.Value.IsExpired)
                                                         .Select(pair => pair.Key)
                                                         .ToList();
                foreach (var encounterId in expiredEncounters)
                {
                    // Spawn expired, remove from cache
                    _processedPokemon.Remove(encounterId);
                }
            }

            lock (_processedRaids)
            {
                var expiredRaids = _processedRaids.Where(pair => pair.Value.IsExpired)
                                                  .Select(pair => pair.Key)
                                                  .ToList();
                foreach (var gymId in expiredRaids)
                {
                    // Gym expired, remove from cache
                    _processedRaids.Remove(gymId);
                }
            }

            lock (_processedQuests)
            {
                var expiredQuests = _processedQuests.Where(pair => pair.Value.IsExpired)
                                                    .Select(pair => pair.Key)
                                                    .ToList();
                foreach (var pokestopId in expiredQuests)
                {
                    // Quest expired, remove from cache
                    _processedQuests.Remove(pokestopId);
                }
            }

            lock (_processedPokestops)
            {
                var expiredPokestops = _processedPokestops.Where(pair => pair.Value.IsExpired)
                                                          .Select(pair => pair.Key)
                                                          .ToList();
                foreach (var pokestopId in expiredPokestops)
                {
                    // Pokestop lure expired, remove from cache
                    _processedPokestops.Remove(pokestopId);
                }
            }

            lock (_processedInvasions)
            {
                var expiredInvasions = _processedInvasions.Where(pair => pair.Value.IsExpired)
                                                          .Select(pair => pair.Key)
                                                          .ToList();
                foreach (var invasionId in expiredInvasions)
                {
                    // Pokestop invasion expired, remove from cache
                    _processedInvasions.Remove(invasionId);
                }
            }

            lock (_processedWeather)
            {
                var expiredWeather = _processedWeather.Where(pair => pair.Value.IsExpired)
                                                      .Select(pair => pair.Key)
                                                      .ToList();
                foreach (var weatherId in expiredWeather)
                {
                    // Weather expired, from from cache
                    _processedWeather.Remove(weatherId);
                }
            }
        }

        private bool CanProceed(PokemonData pokemon)
        {
            // Check if event Pokemon filtering enabled and event Pokemon list is set
            if ((_config.Instance.EventPokemon?.Enabled ?? false) &&
                (_config.Instance.EventPokemon?.PokemonIds?.Count ?? 0) > 0)
            {
                // Only process Pokemon if IV is 0%, greater than or equal to minimum IV set, or has PvP league rankings.
                var allowPokemon = pokemon.IVReal == 0
                    || pokemon.IVReal >= _config.Instance.EventPokemon.MinimumIV
                    || pokemon.HasPvpRankings;

                var filterType = _config.Instance.EventPokemon?.FilterType;
                var ignoreMissingStats = _config.Instance.EventPokemon?.IgnoreMissingStats ?? true;

                /*
                 * Set to `Include` if you do not want the Pokemon reported unless
                   it meets the minimum IV value set (or is 0% or has PvP ranks).
                 * Set to `Exclude` if you only want the Pokemon reported if it meets
                   the minimum IV value set. No other Pokemon will be reported other
                   than those in the event list.
                */

                // Check if Pokemon is in event Pokemon list
                if (_config.Instance.EventPokemon.PokemonIds.Contains(pokemon.PokemonId))
                {
                    // Pokemon is in event Pokemon list
                    switch (filterType)
                    {
                        case FilterType.Exclude:
                            // Skip Pokemon if no IV stats.
                            if (ignoreMissingStats && pokemon.IsMissingStats) return false;
                            // Only allow Pokemon if meets IV/PvP criteria
                            if (!allowPokemon) return false;
                            break;
                        case FilterType.Include:
                            if (ignoreMissingStats && pokemon.IsMissingStats) return false;
                            // Only allow Pokemon if meets IV/PvP criteria
                            if (!allowPokemon) return false;
                            break;
                    }
                }
                else
                {
                    // Pokemon not in event Pokemon list
                    switch (filterType)
                    {
                        case FilterType.Exclude:
                            // Skip any Pokemon that is not in the event list, skip regardless
                            // if criteria matches
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
