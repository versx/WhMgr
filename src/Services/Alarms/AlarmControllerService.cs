﻿namespace WhMgr.Services.Alarms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using Microsoft.Extensions.Hosting;
    //using Microsoft.Extensions.Logging;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.HostedServices.TaskQueue;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Alarms.Filters.Models;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Discord;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Services.Webhook.Queue;

    public class AlarmControllerService : BackgroundService, IAlarmControllerService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<AlarmControllerService> _logger;
        private readonly IReadOnlyDictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly IDiscordClientService _discordService;
        private readonly ConfigHolder _config;
        private readonly IMapDataCache _mapDataCache;
        private readonly IStaticsticsService _statsService;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IWebhookQueueManager _webhookQueueManager;

        public AlarmControllerService(
            Microsoft.Extensions.Logging.ILogger<AlarmControllerService> logger,
            IReadOnlyDictionary<ulong, ChannelAlarmsManifest> alarms,
            IDiscordClientService discordService,
            ConfigHolder config,
            IMapDataCache mapDataCache,
            IStaticsticsService statsService,
            IBackgroundTaskQueue taskQueue,
            IWebhookQueueManager webhookQueueManager)
        {
            _logger = logger;
            _alarms = alarms;
            _discordService = discordService;
            _config = config;
            _mapDataCache = mapDataCache;
            _statsService = statsService;
            _taskQueue = (DefaultBackgroundTaskQueue)taskQueue;
            _webhookQueueManager = webhookQueueManager;
            _logger.Information($"Alarms {_alarms?.Keys?.Count():N0}");
        }

        public void ProcessPokemonAlarms(PokemonData pokemon)
        {
            if (pokemon == null)
                return;

            _statsService.TotalPokemonReceived++;
            if (pokemon.IsMissingStats)
                _statsService.TotalPokemonMissingStatsReceived++;
            else
                _statsService.TotalPokemonWithStatsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnablePokemon ?? false))
            {
                var pokemonAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Pokemon?.Pokemon != null && alarm.Filters.Pokemon.Enabled);
                if (pokemonAlarms == null)
                    continue;

                for (var i = 0; i < pokemonAlarms.Count; i++)
                {
                    var alarm = pokemonAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(pokemon));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokemon {pkmn.Id}: not in geofence.");
                        continue;
                    }

                    if ((alarm.Filters.Pokemon.IsEvent && !(pokemon.IsEvent.HasValue && pokemon.IsEvent.Value)) ||
                        (!alarm.Filters.Pokemon.IsEvent && pokemon.IsEvent.HasValue && pokemon.IsEvent.Value))
                    {
                        // Pokemon does not have event flag indicating it was checked with event account and event filter is set, skip.
                        // or Pokemon has event but filter is set to not include them
                        continue;
                    }

                    if (!PokemonMatchesFilter(pokemon, alarm.Filters.Pokemon))
                    {
                        // Does not match pokemon metadata filter
                        continue;
                    }

                    if (alarm.Filters.Pokemon.IgnoreMissing && pokemon.IsMissingStats)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: IgnoreMissing=true.");
                        continue;
                    }

                    var pvpPokemonFilters = alarm.Filters.Pokemon.Pvp;
                    //if (pokemon.HasPvpRankings && alarm.Filters.Pokemon.Pvp.Count > 0)
                    // Check alarm PVP filter only if set
                    if ((pvpPokemonFilters?.Count ?? 0) > 0)
                    {
                        var pvpFilterLeagues = pvpPokemonFilters?.Keys.ToList();
                        var pvpMatches = pvpFilterLeagues?.Exists(league =>
                        {
                            // Check if webhook Pokemon contains Pokemon alarm PvP league filter
                            if (!(pokemon.PvpRankings?.ContainsKey(league) ?? false))
                            {
                                return false;
                            }
                            // Check if alarm filter contains PvP league
                            if (!(alarm.Filters.Pokemon.Pvp?.ContainsKey(league) ?? false))
                            {
                                return false;
                            }
                            var filterRanking = pvpPokemonFilters[league];
                            var pokemonRankings = pokemon.PvpRankings[league];
                            // Check if any alarm filter matches Pokemon PVP rank for each available league
                            var result = pokemonRankings.Exists(rank =>
                            {
                                //var percentage = Math.Round(Convert.ToDouble(rank.Percentage) * 100.0, 2);
                                var matches =
                                (
                                    Filters.Filters.MatchesPvPRank(rank.Rank ?? 0, filterRanking.MinimumRank, filterRanking.MaximumRank)
                                    ||
                                    Filters.Filters.MatchesPvPRank(rank.CompetitionRank, filterRanking.MinimumRank, filterRanking.MaximumRank)
                                    ||
                                    Filters.Filters.MatchesPvPRank(rank.DenseRank, filterRanking.MinimumRank, filterRanking.MaximumRank)
                                    ||
                                    Filters.Filters.MatchesPvPRank(rank.OrdinalRank, filterRanking.MinimumRank, filterRanking.MaximumRank)
                                )
                                &&
                                Filters.Filters.MatchesCP((uint)rank.CP, filterRanking.MinimumCP, filterRanking.MaximumCP)
                                &&
                                Filters.Filters.MatchesGender(rank.Gender, filterRanking.Gender);
                                // TODO: Reimplement rank product stat percentage filtering (filter.MinimumPercent <= rank.Percentage && filter.MaximumPercent >= rank.Percentage);
                                return matches;
                            });
                            return result;
                        }) ?? false;

                        // Skip Pokemon if PVP filter does not match and that there are PVP filters defined
                        if (!pvpMatches)
                            continue;
                    }
                    else
                    {
                        // Otherwise check based on general Pokemon filtering

                        if (!Filters.Filters.MatchesIV(pokemon.IV, alarm.Filters.Pokemon.MinimumIV, alarm.Filters.Pokemon.MaximumIV))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumIV={alarm.Filters.Pokemon.MinimumIV} and MaximumIV={alarm.Filters.Pokemon.MaximumIV} and IV={pkmn.IV}.");
                            continue;
                        }

                        if (!Filters.Filters.MatchesCP(pokemon.CP, alarm.Filters.Pokemon.MinimumCP, alarm.Filters.Pokemon.MaximumCP))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumCP={alarm.Filters.Pokemon.MinimumCP} and MaximumCP={alarm.Filters.Pokemon.MaximumCP} and CP={pkmn.CP}.");
                            continue;
                        }

                        if (!Filters.Filters.MatchesLvl(pokemon.Level, alarm.Filters.Pokemon.MinimumLevel, alarm.Filters.Pokemon.MaximumLevel))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumLevel={alarm.Filters.Pokemon.MinimumLevel} and MaximumLevel={alarm.Filters.Pokemon.MaximumLevel} and Level={pkmn.Level}.");
                            continue;
                        }

                        if (!Filters.Filters.MatchesGender(pokemon.Gender, alarm.Filters.Pokemon.Gender))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: DesiredGender={alarm.Filters.Pokemon.Gender} and Gender={pkmn.Gender}.");
                            continue;
                        }
                    }

                    if ((alarm.Filters?.Pokemon?.IgnoreMissing ?? false) && !(pokemon.Height != null && pokemon.Weight != null && Filters.Filters.MatchesSize(pokemon.PokemonId.GetSize(pokemon.Height ?? 0, pokemon.Weight ?? 0), alarm.Filters?.Pokemon?.Size)))
                    {
                        continue;
                    }

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = pokemon,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Pokemon alarm: {alarm.Name} for Pokemon {pokemon.PokemonId} ({pokemon.EncounterId}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Pokemon Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Pokemon: {pokemon.PokemonId}, Despawn: {pokemon.DespawnTime}]");
                    }
                }
            }
        }

        public void ProcessRaidAlarms(RaidData raid)
        {
            if (raid == null)
                return;

            if (raid.IsEgg)
                _statsService.TotalEggsReceived++;
            else
                _statsService.TotalRaidsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnableRaids ?? false))
            {
                var raidAlarms = alarms?.Alarms?.FindAll(alarm => (alarm.Filters.Raids?.Enabled ?? false) || (alarm.Filters.Eggs?.Enabled ?? false));
                if (raidAlarms == null)
                    continue;

                for (var i = 0; i < raidAlarms.Count; i++)
                {
                    var alarm = raidAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(raid));
                    if (geofences == null)
                    {
                        //_logger.LogWarning($"[{alarm.Name}] Skipping raid Pokemon={raid.PokemonId}, Level={raid.Level}: not in geofence.");
                        continue;
                    }

                    if (raid.Level == 0)
                    {
                        _logger.Warning($"[{alarm.Name}] Failed to parse '{raid.Level}' as raid level.");
                        continue;
                    }

                    if (raid.IsEgg)
                    {
                        if (alarm.Filters.Eggs == null)
                            continue;

                        if (!alarm.Filters.Eggs.Enabled)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: raids filter not enabled.");
                            continue;
                        }

                        if (!(raid.Level >= alarm.Filters.Eggs.MinimumLevel && raid.Level <= alarm.Filters.Eggs.MaximumLevel))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: '{raid.Level}' does not meet the MinimumLevel={alarm.Filters.Eggs.MinimumLevel} and MaximumLevel={alarm.Filters.Eggs.MaximumLevel} filters.");
                            continue;
                        }

                        if (alarm.Filters.Eggs.OnlyEx && !raid.IsExEligible)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: only ex {alarm.Filters.Eggs.OnlyEx}.");
                            continue;
                        }

                        if (alarm.Filters.Eggs.Team != PokemonTeam.All && alarm.Filters.Eggs.Team != raid.Team)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid egg: '{raid.Team}' does not meet Team={alarm.Filters.Eggs.Team} filter.");
                            continue;
                        }

                        if (!PowerLevelMatchesFilter(raid, alarm.Filters.Eggs.PowerLevel))
                        {
                            // Power level does not match
                            continue;
                        }
                    }
                    else
                    {
                        if (alarm.Filters.Raids == null)
                            continue;

                        if (!alarm.Filters.Raids.Enabled)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: raids filter not enabled.");
                            continue;
                        }

                        if (!(raid.Level >= alarm.Filters.Raids.MinimumLevel && raid.Level <= alarm.Filters.Raids.MaximumLevel))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping level {raid.Level} raid {raid.PokemonId}: '{raid.Level}' does not meet the MinimumLevel={alarm.Filters.Raids.MinimumLevel} and MaximumLevel={alarm.Filters.Raids.MaximumLevel} filters.");
                            continue;
                        }

                        if (!PokemonMatchesFilter(raid, alarm.Filters.Raids))
                        {
                            // Does not match pokemon metadata filter
                            continue;
                        }

                        if (alarm.Filters.Raids.OnlyEx && !raid.IsExEligible)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: only ex {alarm.Filters.Raids.OnlyEx}.");
                            continue;
                        }

                        if (alarm.Filters.Raids.Team != PokemonTeam.All && alarm.Filters.Raids.Team != raid.Team)
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: '{raid.Team}' does not meet Team={alarm.Filters.Raids.Team} filter.");
                            continue;
                        }

                        if (!PowerLevelMatchesFilter(raid, alarm.Filters.Raids.PowerLevel))
                        {
                            // Power level does not match
                            continue;
                        }

                        if (alarm.Filters.Raids.IgnoreMissing && raid.IsMissingStats)
                        {
                            _logger.Information($"[{alarm.Name}] Skipping raid boss {raid.PokemonId}: IgnoreMissing=true.");
                            continue;
                        }
                    }

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = raid,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Raid alarm: {alarm.Name} for Raid {raid.PokemonId} ({raid.Level}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Raid Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Raid: {raid.PokemonId}, Level: {raid.Level}, StartTime: {raid.StartTime}]");
                    }
                }
            }
        }

        public void ProcessQuestAlarms(QuestData quest)
        {
            if (quest == null)
                return;

            _statsService.TotalQuestsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnableQuests ?? false))
            {
                var questAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Quests != null && alarm.Filters.Quests.Enabled);
                if (questAlarms == null)
                    continue;

                var rewardKeyword = quest.GetReward();
                for (var i = 0; i < questAlarms.Count; i++)
                {
                    var alarm = questAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(quest));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: not in geofence.");
                        continue;
                    }

                    var contains = alarm.Filters.Quests.RewardKeywords.Select(keyword => keyword.ToLower()).FirstOrDefault(keyword => rewardKeyword.ToLower().Contains(keyword.ToLower())) != null;
                    if (alarm.Filters.Quests.FilterType == FilterType.Exclude && contains)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: filter {alarm.Filters.Quests.FilterType}.");
                        continue;
                    }

                    if (!(alarm.Filters.Quests.FilterType == FilterType.Include && (contains || alarm.Filters.Quests?.RewardKeywords.Count == 0)))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}: filter {alarm.Filters.Quests.FilterType}.");
                        continue;
                    }

                    if (!contains && alarm.Filters?.Quests?.RewardKeywords?.Count > 0)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: rewards does not match reward keywords.");
                        continue;
                    }

                    if (alarm.Filters.Quests.IsShiny && !quest.IsShiny)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: filter IsShiny={alarm.Filters.Quests.IsShiny} Quest={quest.IsShiny}.");
                        continue;
                    }

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = quest,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Quest alarm: {alarm.Name} for Quest {quest.PokestopId} ({quest.PokestopName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Quest Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, PokestopId: {quest.PokestopId}, Name: {quest.PokestopName}, Template: {quest.Template}]");
                    }
                }
            }
        }

        public void ProcessPokestopAlarms(PokestopData pokestop)
        {
            if (pokestop == null)
                return;

            _statsService.TotalPokestopsReceived++;
            if (pokestop.HasLure)
                _statsService.TotalLuresReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnablePokestops ?? false))
            {
                var pokestopAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Pokestops != null && alarm.Filters.Pokestops.Enabled);
                if (pokestopAlarms == null)
                    continue;

                for (var i = 0; i < pokestopAlarms.Count; i++)
                {
                    var alarm = pokestopAlarms[i];

                    var hasLure = alarm.Filters.Pokestops.Lured && pokestop.HasLure;
                    var hasLureType = alarm.Filters.Pokestops.LureTypes.Select(lure => lure.ToLower()).Contains(pokestop.LureType.ToString().ToLower())
                        && alarm.Filters.Pokestops.LureTypes.Count > 0;

                    if (!(hasLure && hasLureType))
                    {
                        // Does not meet lure filtering
                        continue;
                    }
                    if (!PowerLevelMatchesFilter(pokestop, alarm.Filters.Pokestops.PowerLevel))
                    {
                        // Power level does not match
                        continue;
                    }
                        
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(pokestop));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name} because not in geofence.");
                        continue;
                    }

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = pokestop,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Pokestop alarm: {alarm.Name} for Pokestop {pokestop.FortId} ({pokestop.FortName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Pokestop Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, PokestopId: {pokestop.FortId}, Name: {pokestop.FortName}, LureType: {pokestop.LureType}");
                    }
                }
            }
        }

        public void ProcessInvasionAlarms(IncidentData incident)
        {
            if (incident == null)
                return;

            _statsService.TotalInvasionsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnableInvasions ?? false))
            {
                var invasionAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Invasions != null && alarm.Filters.Invasions.Enabled);
                if (invasionAlarms == null)
                    continue;

                for (var i = 0; i < invasionAlarms.Count; i++)
                {
                    var alarm = invasionAlarms[i];
                    var matchesInvasionType = alarm.Filters.Invasions.InvasionTypes.ContainsKey(incident.Character)
                        && alarm.Filters.Invasions.InvasionTypes[incident.Character];

                    if (!matchesInvasionType)
                        continue;

                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(incident));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name} because not in geofence.");
                        continue;
                    }

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = incident,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Invasion alarm: {alarm.Name} for Pokestop {incident.PokestopId} ({incident.PokestopName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Invasion Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, PokestopId: {incident.PokestopId}, Name: {incident.PokestopName}, GruntType: {incident.Character}");
                    }
                }
            }
        }

        public void ProcessGymAlarms(GymDetailsData gym)
        {
            if (gym == null)
                return;

            _statsService.TotalGymsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnableGyms ?? false))
            {
                var gymAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Gyms != null && alarm.Filters.Gyms.Enabled);
                if (gymAlarms == null)
                    continue;

                for (var i = 0; i < gymAlarms.Count; i++)
                {
                    var alarm = gymAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(gym));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping gym details GymId={gym.GymId}, GymName={gym.GymName}: not in geofence.");
                        continue;
                    }

                    if ((alarm.Filters?.Gyms?.UnderAttack ?? false) && !gym.InBattle)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping gym details GymId={gym.GymId}, GymName{gym.GymName}, not under attack.");
                        continue;
                    }

                    if (alarm.Filters?.Gyms?.Team != gym.Team && alarm.Filters?.Gyms?.Team != PokemonTeam.All)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping gym details GymId={gym.GymId}, GymName{gym.GymName}, not specified team {alarm.Filters.Gyms.Team}.");
                        continue;
                    }

                    if (!PowerLevelMatchesFilter(gym, alarm.Filters.Gyms?.PowerLevel))
                    {
                        // Power level does not match
                        continue;
                    }

                    var oldGym = _mapDataCache.GetGym(gym.FortId).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (oldGym != null)
                    {
                        var changed = oldGym.Team != gym.Team
                            || gym.InBattle
                            || oldGym.SlotsAvailable != gym.SlotsAvailable;
                        if (!changed)
                            continue;
                    }

                    /*
                    var oldGym = _gyms[gym.GymId];
                    var changed = oldGym.Team != gym.Team || gym.InBattle;
                    if (!changed)
                        return;
                    */

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = gym,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Gym alarm: {alarm.Name} for Gym {gym.FortId} ({gym.FortName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Gym Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, GymId: {gym.FortId}, Name: {gym.FortName}, Team: {gym.Team}, InBattle: {gym.InBattle}");
                    }

                    // Update map data cache with gym
                    _mapDataCache.UpdateGym(gym);
                }
            }
        }

        public void ProcessWeatherAlarms(WeatherData weather)
        {
            if (weather == null)
                return;

            _statsService.TotalWeatherReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(alarm => alarm.Value?.EnableWeather ?? false))
            {
                var weatherAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Weather != null && alarm.Filters.Weather.Enabled);
                if (weatherAlarms == null)
                    continue;

                for (var i = 0; i < weatherAlarms.Count; i++)
                {
                    var alarm = weatherAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(weather));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping gym details GymId={gymDetails.GymId}, GymName={gymDetails.GymName}: not in geofence.");
                        continue;
                    }

                    if (!alarm.Filters.Weather.WeatherTypes.Contains(weather.GameplayCondition))
                    {
                        // Weather is not in list of accepted ones to send alarms for
                        continue;
                    }

                    var oldWeather = _mapDataCache.GetWeather(weather.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                    var changed = oldWeather.GameplayCondition != weather.GameplayCondition
                        || oldWeather.Severity != weather.Severity;
                    if (!changed)
                        continue;

                    /*
                    if (!_weather.ContainsKey(weather.Id))
                    {
                        _weather.Add(weather.Id, weather.GameplayCondition);
                        OnWeatherAlarmTriggered(weather, alarm, guildId);
                        continue;
                    }

                    var oldWeather = _weather[weather.Id];
                    // If previous weather and current weather are the same then don't report it.
                    if (oldWeather == weather.GameplayCondition)
                        continue;
                    */

                    foreach (var geofence in geofences)
                    {
                        var taskItem = new AlarmTaskItem
                        {
                            GuildId = guildId,
                            Alarm = alarm,
                            Data = weather,
                            City = geofence.Name,
                        };
                        if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                        {
                            _logger.Error($"Failed to queue Weather alarm: {alarm.Name} for Gym {weather.Id} ({weather.GameplayCondition}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.Information($"Weather Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Id: {weather.Id}, Name: {weather.GameplayCondition}, Severity: {weather.Severity}");
                    }

                    // Update map data cache with weather
                    _mapDataCache.UpdateWeather(weather);
                }
            }
        }

        public void ProcessAccountAlarms(AccountData account)
        {
            if (account == null)
                return;

            //_statsService.TotalAccountsReceived++;
            /*
            foreach (var (guildId, alarms) in _alarms)//.Where(alarm => alarm.Value?.EnableAccounts ?? false))
            {
                var accountAlarms = alarms?.Alarms?.FindAll(alarm => alarm.Filters?.Weather != null && alarm.Filters.Accounts.Enabled);
                if (accountAlarms == null)
                    continue;

                for (var i = 0; i < accountAlarms.Count; i++)
                {
                    var alarm = accountAlarms[i];
                    if (!alarm.Filters.Account.WeatherTypes.Contains(account.Username))
                    {
                        // Weather is not in list of accepted ones to send alarms for
                        continue;
                    }

                    var taskItem = new AlarmTaskItem
                    {
                        GuildId = guildId,
                        Alarm = alarm,
                        Data = account,
                    };
                    if (!ThreadPool.QueueUserWorkItem(async _ => await EnqueueEmbedAsync(taskItem)))
                    {
                        _logger.Error($"Failed to queue Account alarm: {alarm.Name} for Account {account.Username}");
                        continue;
                    }
                    _logger.Information($"Account Found [Alarm: {alarm.Name}, Username: {account.Username}, Warning: {account.IsWarned}, Banned: {account.IsBanned}");
                }
            }
            */
        }

        private static bool PokemonMatchesFilter(IWebhookPokemon pkmn, IWebhookFilterPokemonDetails details)
        {
            if (details.FilterType == FilterType.Exclude && details.Pokemon.Contains(pkmn.PokemonId))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            if (details.FilterType == FilterType.Include && (!details.Pokemon.Contains(pkmn.PokemonId) && details.Pokemon?.Count > 0))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            var formName = Translator.Instance.GetFormName(pkmn.FormId)?.ToLower();
            if (details.FilterType == FilterType.Exclude && details.Forms.Select(form => form.ToLower()).Contains(formName))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with form {raid.Form} ({formName}): filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            if (details.FilterType == FilterType.Include && details.Forms?.Count > 0 && !details.Forms.Select(form => form.ToLower()).Contains(formName))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with form {raid.Form} ({formName}): filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            var costumeName = Translator.Instance.GetCostumeName(pkmn.CostumeId)?.ToLower();
            if (details.FilterType == FilterType.Exclude && details.Costumes.Select(costume => costume.ToLower()).Contains(costumeName))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with costume {raid.Costume} ({costumeName}): filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            if (details.FilterType == FilterType.Include && details.Costumes?.Count > 0 && !details.Costumes.Select(costume => costume.ToLower()).Contains(costumeName))
            {
                //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with costume {raid.Costume} ({costumeName}): filter {alarm.Filters.Raids.FilterType}.");
                return false;
            }

            return true;
        }

        private static bool PowerLevelMatchesFilter(IWebhookPowerLevel fort, WebhookFilterGymLevel powerLevelFilter)
        {
            if (powerLevelFilter != null)
            {
                if (!Filters.Filters.MatchesGymPowerLevel(fort.PowerUpLevel, powerLevelFilter?.MinimumLevel ?? 0, powerLevelFilter?.MaximumLevel ?? 0))
                {
                    return false;
                }

                if (!Filters.Filters.MatchesGymPowerPoints(fort.PowerUpPoints, powerLevelFilter?.MinimumPoints ?? 0, powerLevelFilter?.MaximumPoints ?? 0))
                {
                    return false;
                }
            }

            return true;
        }

        #region Background Service

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Information(
                $"{nameof(AlarmControllerService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information(
                $"{nameof(AlarmControllerService)} is now running in the background.");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                    //await workItem(stoppingToken);
                    var workItems = await _taskQueue.DequeueMultipleAsync(Strings.Defaults.MaximumQueueBatchSize, stoppingToken);
                    foreach (var workItem in workItems)
                    {
                        await workItem(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error occurred executing task work item.");
                }
                Thread.Sleep(10);
            }

            _logger.Error("Exited background processing...");
        }

        private async Task EnqueueEmbedAsync(AlarmTaskItem taskItem)
        {
            //CheckQueueLength();

            await _taskQueue.EnqueueAsync(async token =>
                await ProcessWorkItemAsync(taskItem, token));
        }

        private async Task<CancellationToken> ProcessWorkItemAsync(
            AlarmTaskItem taskItem,
            CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(taskItem.Alarm.Webhook))
                return stoppingToken;

            if (!_discordService.DiscordClients.ContainsKey(taskItem.GuildId))
                return stoppingToken;

            if (!_config.Instance.Servers.ContainsKey(taskItem.GuildId))
                return stoppingToken;

            CheckQueueLength();

            // Queue embed
            //_logger.Information($"[{taskItem.City}] Found {taskItem.Data.GetType().Name} [Alarm={taskItem.Alarm.Name}, GuildId={taskItem.GuildId}]");

            try
            {
                var client = _discordService.DiscordClients[taskItem.GuildId];
                var eb = await taskItem.Data.GenerateEmbedMessageAsync(new AlarmMessageSettings
                {
                    GuildId = taskItem.GuildId,
                    Client = client,
                    Config = _config,
                    Alarm = taskItem.Alarm,
                    City = taskItem.City,
                    MapDataCache = _mapDataCache,
                }).ConfigureAwait(false);
                var json = eb.Build();
                if (json == null)
                {
                    _logger.Error($"Failed to convert embed notification to JSON string, skipping");
                    return stoppingToken;
                }
                //WhMgr.Utilities.NetUtils.SendWebhook(taskItem.Alarm.Webhook, json);
                await _webhookQueueManager.SendWebhook(taskItem.Alarm.Webhook, json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error: {ex}");
            }

            return stoppingToken;
        }

        private void CheckQueueLength()
        {
            if (_taskQueue.Count > Strings.Defaults.MaximumQueueSizeWarning)
            {
                _logger.Warning($"Alarm controller queue is {_taskQueue.Count:N0} items long.");
            }
            else if (_taskQueue.Count >= Strings.Defaults.MaximumQueueCapacity)
            {
                _logger.Error($"Queue has filled to maximum capacity '{Strings.Defaults.MaximumQueueCapacity}', oldest queued items will start to drop off to make room.");
                _taskQueue.ClearQueue();
            }
        }

        #endregion
    }
}