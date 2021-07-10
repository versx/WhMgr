namespace WhMgr.Services.Alarms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DSharpPlus;
    using Microsoft.Extensions.Logging;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Utilities;

    public class AlarmControllerService : IAlarmControllerService
    {
        private readonly ILogger<AlarmControllerService> _logger;
        private readonly IReadOnlyDictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly IReadOnlyDictionary<ulong, DiscordClient> _discordClients;
        private readonly ConfigHolder _config;
        private readonly IMapDataCache _mapDataCache;
        private readonly IStaticsticsService _statsService;

        public AlarmControllerService(
            ILogger<AlarmControllerService> logger,
            IReadOnlyDictionary<ulong, ChannelAlarmsManifest> alarms,
            IReadOnlyDictionary<ulong, DiscordClient> discordClients,
            ConfigHolder config,
            IMapDataCache mapDataCache,
            IStaticsticsService statsService)
        {
            _logger = logger;
            _alarms = alarms;
            _discordClients = discordClients;
            _config = config;
            _mapDataCache = mapDataCache;
            _statsService = statsService;
            _logger.LogInformation($"Alarms {_alarms?.Keys?.Count():N0}");
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

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnablePokemon ?? false))
            {
                var pokemonAlarms = alarms?.Alarms?.FindAll(x => x.Filters?.Pokemon?.Pokemon != null && x.Filters.Pokemon.Enabled);
                if (pokemonAlarms == null)
                    continue;

                for (var i = 0; i < pokemonAlarms.Count; i++)
                {
                    var alarm = pokemonAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(pokemon.Latitude, pokemon.Longitude));
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

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Pokemon.Contains(pokemon.Id))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Pokemon?.Count > 0 && !alarm.Filters.Pokemon.Pokemon.Contains(pokemon.Id))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    var formName = Translator.Instance.GetFormName(pokemon.FormId)?.ToLower();
                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Forms.Select(x => x.ToLower()).Contains(formName))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with form {pkmn.FormId} ({formName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Forms?.Count > 0 && !alarm.Filters.Pokemon.Forms.Select(x => x.ToLower()).Contains(formName))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with form {pkmn.FormId} ({formName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    var costumeName = Translator.Instance.GetCostumeName(pokemon.Costume)?.ToLower();
                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with costume {pkmn.Costume} ({costumeName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Costumes?.Count > 0 && !alarm.Filters.Pokemon.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with costume {pkmn.Costume} ({costumeName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.IgnoreMissing && pokemon.IsMissingStats)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: IgnoreMissing=true.");
                        continue;
                    }

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

                    var skipGreatLeague = alarm.Filters.Pokemon.IsPvpGreatLeague &&
                        !(pokemon.MatchesGreatLeague && pokemon.GreatLeague.Exists(x =>
                            Filters.Filters.MatchesPvPRank(x.Rank ?? 4096, alarm.Filters.Pokemon.MinimumRank, alarm.Filters.Pokemon.MaximumRank)
                            && x.CP >= Strings.MinimumGreatLeagueCP && x.CP <= Strings.MaximumGreatLeagueCP));
                    if (skipGreatLeague)
                        continue;

                    var skipUltraLeague = alarm.Filters.Pokemon.IsPvpUltraLeague &&
                        !(pokemon.MatchesUltraLeague && pokemon.UltraLeague.Exists(x =>
                            Filters.Filters.MatchesPvPRank(x.Rank ?? 4096, alarm.Filters.Pokemon.MinimumRank, alarm.Filters.Pokemon.MaximumRank)
                            && x.CP >= Strings.MinimumUltraLeagueCP && x.CP <= Strings.MaximumUltraLeagueCP));
                    if (skipUltraLeague)
                        continue;

                    if (!Filters.Filters.MatchesGender(pokemon.Gender, alarm.Filters.Pokemon.Gender.ToString()))
                    {
                        //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: DesiredGender={alarm.Filters.Pokemon.Gender} and Gender={pkmn.Gender}.");
                        continue;
                    }

                    if ((alarm.Filters?.Pokemon?.IgnoreMissing ?? false) && !(pokemon.Height != null && pokemon.Weight != null && Filters.Filters.MatchesSize(pokemon.Id.GetSize(pokemon.Height ?? 0, pokemon.Weight ?? 0), alarm.Filters?.Pokemon?.Size)))
                    {
                        continue;
                    }

                    // TODO: Proper queue service
                    foreach (var geofence in geofences)
                    {
                        if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, pokemon, geofence.Name)))
                        {
                            _logger.LogError($"Failed to queue Pokemon alarm: {alarm.Name} for Pokemon {pokemon.Id} ({pokemon.EncounterId}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Pokemon Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Pokemon: {pokemon.Id}, Despawn: {pokemon.DespawnTime}]");
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

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnableRaids ?? false))
            {
                var raidAlarms = alarms?.Alarms?.FindAll(x => (x.Filters.Raids?.Enabled ?? false) || (x.Filters.Eggs?.Enabled ?? false));
                if (raidAlarms == null)
                    continue;

                for (var i = 0; i < raidAlarms.Count; i++)
                {
                    var alarm = raidAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(raid.Latitude, raid.Longitude));
                    if (geofences == null)
                    {
                        //_logger.LogWarning($"[{alarm.Name}] Skipping raid Pokemon={raid.PokemonId}, Level={raid.Level}: not in geofence.");
                        continue;
                    }

                    if (raid.Level == 0)
                    {
                        _logger.LogWarning($"[{alarm.Name}] Failed to parse '{raid.Level}' as raid level.");
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

                        foreach (var geofence in geofences)
                        {
                            if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, raid, geofence.Name)))
                            {
                                _logger.LogError($"Failed to queue Raid alarm: {alarm.Name} for Raid {raid.PokemonId} ({raid.Level}) from geofence {geofence.Name}");
                                continue;
                            }
                            _logger.LogInformation($"Raid Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Raid: {raid.PokemonId}, Level: {raid.Level}, StartTime: {raid.StartTime}]");
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

                        if (alarm.Filters.Raids.FilterType == FilterType.Exclude && alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                            continue;
                        }

                        if (alarm.Filters.Raids.FilterType == FilterType.Include && (!alarm.Filters.Raids.Pokemon.Contains(raid.PokemonId) && alarm.Filters.Raids.Pokemon?.Count > 0))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.PokemonId}: filter {alarm.Filters.Raids.FilterType}.");
                            continue;
                        }

                        var formName = Translator.Instance.GetFormName(raid.Form)?.ToLower();
                        if (alarm.Filters.Raids.FilterType == FilterType.Exclude && alarm.Filters.Raids.Forms.Select(x => x.ToLower()).Contains(formName))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with form {raid.Form} ({formName}): filter {alarm.Filters.Raids.FilterType}.");
                            continue;
                        }

                        if (alarm.Filters.Raids.FilterType == FilterType.Include && alarm.Filters.Raids.Forms?.Count > 0 && !alarm.Filters.Raids.Forms.Select(x => x.ToLower()).Contains(formName))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with form {raid.Form} ({formName}): filter {alarm.Filters.Raids.FilterType}.");
                            continue;
                        }

                        var costumeName = Translator.Instance.GetCostumeName(raid.Costume)?.ToLower();
                        if (alarm.Filters.Raids.FilterType == FilterType.Exclude && alarm.Filters.Raids.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with costume {raid.Costume} ({costumeName}): filter {alarm.Filters.Raids.FilterType}.");
                            continue;
                        }

                        if (alarm.Filters.Raids.FilterType == FilterType.Include && alarm.Filters.Raids.Costumes?.Count > 0 && !alarm.Filters.Raids.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                        {
                            //_logger.LogDebug($"[{alarm.Name}] [{geofence.Name}] Skipping raid boss {raid.Id} with costume {raid.Costume} ({costumeName}): filter {alarm.Filters.Raids.FilterType}.");
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

                        if (alarm.Filters.Raids.IgnoreMissing && raid.IsMissingStats)
                        {
                            _logger.LogInformation($"[{alarm.Name}] Skipping raid boss {raid.PokemonId}: IgnoreMissing=true.");
                            continue;
                        }

                        foreach (var geofence in geofences)
                        {
                            if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, raid, geofence.Name)))
                            {
                                _logger.LogError($"Failed to queue Raid alarm: {alarm.Name} for Raid {raid.PokemonId} ({raid.Level}) from geofence {geofence.Name}");
                                continue;
                            }
                            _logger.LogInformation($"Raid Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Raid: {raid.PokemonId}, Level: {raid.Level}, StartTime: {raid.StartTime}]");
                        }
                    }
                }
            }
        }

        public void ProcessQuestAlarms(QuestData quest)
        {
            if (quest == null)
                return;

            _statsService.TotalQuestsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnableQuests ?? false))
            {
                var questAlarms = alarms?.Alarms?.FindAll(x => x.Filters?.Quests != null && x.Filters.Quests.Enabled);
                if (questAlarms == null)
                    continue;

                var rewardKeyword = quest.GetReward();
                for (var i = 0; i < questAlarms.Count; i++)
                {
                    var alarm = questAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(quest.Latitude, quest.Longitude));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping quest PokestopId={quest.PokestopId}, Type={quest.Type}: not in geofence.");
                        continue;
                    }

                    var contains = alarm.Filters.Quests.RewardKeywords.Select(x => x.ToLower()).FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.ToLower())) != null;
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
                        if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, quest, geofence.Name)))
                        {
                            _logger.LogError($"Failed to queue Quest alarm: {alarm.Name} for Quest {quest.PokestopId} ({quest.PokestopName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Raid Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, PokestopId: {quest.PokestopId}, Name: {quest.PokestopName}, Template: {quest.Template}]");
                    }
                }
            }
        }

        public void ProcessPokestopAlarms(PokestopData pokestop)
        {
            if (pokestop == null)
                return;

            _statsService.TotalPokestopsReceived++;
            if (pokestop.HasInvasion)
                _statsService.TotalInvasionsReceived++;
            if (pokestop.HasLure)
                _statsService.TotalLuresReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnablePokestops ?? false))
            {
                var pokestopAlarms = alarms?.Alarms?.FindAll(x => x.Filters?.Pokestops != null && x.Filters.Pokestops.Enabled);
                if (pokestopAlarms == null)
                    continue;

                for (var i = 0; i < pokestopAlarms.Count; i++)
                {
                    var alarm = pokestopAlarms[i];

                    // TODO: Combine checks
                    if (!alarm.Filters.Pokestops.Lured && pokestop.HasLure)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name}: lure filter not enabled.");
                        continue;
                    }

                    if (!alarm.Filters.Pokestops.LureTypes.Select(x => x.ToLower()).Contains(pokestop.LureType.ToString().ToLower()) && alarm.Filters.Pokestops?.LureTypes?.Count > 0)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name}, LureType={pokestop.LureType}: lure type not included.");
                        continue;
                    }

                    if (!alarm.Filters.Pokestops.Invasions && pokestop.HasInvasion)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name}: invasion filter not enabled.");
                        continue;
                    }

                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(pokestop.Latitude, pokestop.Longitude));
                    if (geofences == null)
                    {
                        //_logger.LogDebug($"[{alarm.Name}] Skipping pokestop PokestopId={pokestop.PokestopId}, Name={pokestop.Name} because not in geofence.");
                        continue;
                    }

                    foreach (var geofence in geofences)
                    {
                        if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, pokestop, geofence.Name)))
                        {
                            _logger.LogError($"Failed to queue Pokestop alarm: {alarm.Name} for Pokestop {pokestop.PokestopId} ({pokestop.Name}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Pokestop Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, PokestopId: {pokestop.PokestopId}, Name: {pokestop.Name}, LureType: {pokestop.LureType}, GruntType: {pokestop.GruntType}");
                    }
                }
            }
        }

        public void ProcessGymAlarms(GymDetailsData gym)
        {
            if (gym == null)
                return;

            _statsService.TotalGymsReceived++;

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnableGyms ?? false))
            {
                var gymAlarms = alarms?.Alarms?.FindAll(x => x.Filters?.Gyms != null && x.Filters.Gyms.Enabled);
                if (gymAlarms == null)
                    continue;

                for (var i = 0; i < gymAlarms.Count; i++)
                {
                    var alarm = gymAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(gym.Latitude, gym.Longitude));
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

                    var oldGym = _mapDataCache.GetGym(gym.GymId).ConfigureAwait(false).GetAwaiter().GetResult();
                    var changed = oldGym.Team != gym.Team
                        || gym.InBattle
                        || oldGym.SlotsAvailable != gym.SlotsAvailable;
                    if (!changed)
                        continue;

                    /*
                    var oldGym = _gyms[gym.GymId];
                    var changed = oldGym.Team != gym.Team || gym.InBattle;
                    if (!changed)
                        return;
                    */

                    foreach (var geofence in geofences)
                    {
                        if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, gym, geofence.Name)))
                        {
                            _logger.LogError($"Failed to queue Gym alarm: {alarm.Name} for Gym {gym.GymId} ({gym.GymName}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Gym Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, GymId: {gym.GymId}, Name: {gym.GymName}, Team: {gym.Team}, InBattle: {gym.InBattle}");
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

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value?.EnableWeather ?? false))
            {
                var weatherAlarms = alarms?.Alarms?.FindAll(x => x.Filters?.Weather != null && x.Filters.Weather.Enabled);
                if (weatherAlarms == null)
                    continue;

                for (var i = 0; i < weatherAlarms.Count; i++)
                {
                    var alarm = weatherAlarms[i];
                    var geofences = GeofenceService.GetGeofences(alarm.GeofenceItems, new Coordinate(weather.Latitude, weather.Longitude));
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
                        if (!ThreadPool.QueueUserWorkItem(x => SendEmbed(guildId, alarm, weather, geofence.Name)))
                        {
                            _logger.LogError($"Failed to queue Weather alarm: {alarm.Name} for Gym {weather.Id} ({weather.GameplayCondition}) from geofence {geofence.Name}");
                            continue;
                        }
                        _logger.LogInformation($"Weather Found [Geofence: {geofence.Name} Alarm: {alarm.Name}, Id: {weather.Id}, Name: {weather.GameplayCondition}, Severity: {weather.Severity}");
                    }

                    // Update map data cache with weather
                    _mapDataCache.UpdateWeather(weather);
                }
            }
        }

        // TODO: Alarm queue service (background service or hosted service)
        private void SendEmbed(ulong guildId, ChannelAlarm alarm, IWebhookData data, string city)
        {
            if (string.IsNullOrEmpty(alarm.Webhook))
                return;

            if (!_discordClients.ContainsKey(guildId))
                return;

            if (!_config.Instance.Servers.ContainsKey(guildId))
                return;

            try
            {
                var server = _config.Instance.Servers[guildId];
                var client = _discordClients[guildId];
                var eb = data.GenerateEmbedMessage(new AlarmMessageSettings
                {
                    GuildId = guildId,
                    Client = client,
                    Config = _config,
                    Alarm = alarm,
                    City = city,
                    MapDataCache = _mapDataCache,
                });
                var json = eb.Build();
                if (json == null)
                {
                    _logger.LogError($"Failed to convert embed notification to JSON string, skipping");
                    return;
                }
                NetUtils.SendWebhook(alarm.Webhook, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
            }
        }
    }
}