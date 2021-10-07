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
        private readonly ISubscriptionProcessorService _subscriptionService;
        private readonly IMapDataCache _mapDataCache;

        private readonly Dictionary<string, ScannedPokemon> _processedPokemon;
        private readonly Dictionary<string, ScannedRaid> _processedRaids;
        private readonly Dictionary<string, ScannedQuest> _processedQuests;
        private readonly Dictionary<string, ScannedPokestop> _processedPokestops;
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
            ISubscriptionProcessorService subscriptionService,
            IMapDataCache mapDataCache)
        {
            _logger = logger;
            _config = config;
            _alarmsService = alarmsService;
            _subscriptionService = subscriptionService;
            _mapDataCache = mapDataCache;

            _processedPokemon = new Dictionary<string, ScannedPokemon>();
            _processedRaids = new Dictionary<string, ScannedRaid>();
            _processedQuests = new Dictionary<string, ScannedQuest>();
            _processedPokestops = new Dictionary<string, ScannedPokestop>();
            _processedGyms = new Dictionary<string, ScannedGym>();
            _processedWeather = new Dictionary<long, ScannedWeather>();

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

        public async Task ParseData(List<WebhookPayload> payloads)
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
                        await ProcessQuestAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Invasion:
                    case WebhookTypes.Pokestop:
                        await ProcessPokestopAsync(payload.Message).ConfigureAwait(false);
                        break;
                    case WebhookTypes.Gym:
                    case WebhookTypes.GymDetails:
                        ProcessGym(payload.Message);
                        break;
                    case WebhookTypes.Weather:
                        ProcessWeather(payload.Message);
                        break;
                    default:
                        _logger.Warning($"Unhandled webhook type: {payload.Type}");
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

            if (pokemon.SecondsLeft.TotalMinutes < DespawnTimerMinimumMinutes)
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
            await _subscriptionService.ProcessPokemonSubscriptionAsync(pokemon).ConfigureAwait(false);
            await _subscriptionService.ProcessPvpSubscriptionAsync(pokemon).ConfigureAwait(false);
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
                            && _processedRaids[raid.GymId].FormId == raid.Form
                            && _processedRaids[raid.GymId].CostumeId == raid.Costume
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
            await _subscriptionService.ProcessRaidSubscriptionAsync(raid).ConfigureAwait(false);
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
            await _subscriptionService.ProcessQuestSubscriptionAsync(quest).ConfigureAwait(false);
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
                    if (_processedPokestops.ContainsKey(pokestop.PokestopId))
                    {
                        var processedLureAlready = _processedPokestops[pokestop.PokestopId].LureType == pokestop.LureType
                            && _processedPokestops[pokestop.PokestopId].LureExpireTime == pokestop.LureExpireTime;
                        var processedInvasionAlready = _processedPokestops[pokestop.PokestopId].GruntType == pokestop.GruntType
                            && _processedPokestops[pokestop.PokestopId].InvasionExpireTime == pokestop.InvasionExpireTime;

                        if ((processedLureAlready || processedInvasionAlready) &&
                            !(processedLureAlready && processedInvasionAlready))
                        {
                            // Processed pokestop lure or invasion already and not both
                            return;
                        }

                        _processedPokestops[pokestop.PokestopId] = new ScannedPokestop(pokestop);
                    }
                    else
                    {
                        _processedPokestops.Add(pokestop.PokestopId, new ScannedPokestop(pokestop));
                    }
                }
            }

            // Process pokestop alarms
            _alarmsService.ProcessPokestopAlarms(pokestop);

            // Process invasion subscriptions
            if (pokestop.HasInvasion)
            {
                await _subscriptionService.ProcessInvasionSubscriptionAsync(pokestop).ConfigureAwait(false);
            }

            // Process lure subscriptions
            if (pokestop.HasLure)
            {
                await _subscriptionService.ProcessLureSubscriptionAsync(pokestop).ConfigureAwait(false);
            }
        }

        private void ProcessGym(dynamic message)
        {
            string json = Convert.ToString(message);
            var gym = json.FromJson<GymDetailsData>();
            if (gym == null)
            {
                _logger.Warning($"Failed to deserialize gym {message}, skipping...");
            }

            if (CheckForDuplicates)
            {
                // Lock process gyms, check for duplicates of incoming gym
                lock (_processedGyms)
                {
                    if (_processedGyms.ContainsKey(gym.GymId))
                    {
                        if (gym.Team == gym.Team
                            && gym.SlotsAvailable == gym.SlotsAvailable
                            && gym.InBattle == gym.InBattle)
                        {
                            // Gym already processed
                            return;
                        }

                        _processedGyms[gym.GymId] = new ScannedGym(gym);
                    }
                    else
                    {
                        _processedGyms.Add(gym.GymId, new ScannedGym(gym));
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
                    // Pokestop lure or invasion expired, remove from cache
                    _processedPokestops.Remove(pokestopId);
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
    }
}