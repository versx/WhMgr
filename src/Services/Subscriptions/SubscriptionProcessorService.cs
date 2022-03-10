namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    //using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.HostedServices.TaskQueue;
    using WhMgr.Localization;
    using WhMgr.Queues;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Discord;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Services.Webhook.Models;

    public class SubscriptionProcessorService : BackgroundService, ISubscriptionProcessorService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ISubscriptionProcessorService> _logger;
        private readonly ISubscriptionManagerService _subscriptionManager;
        private readonly ConfigHolder _config;
        private readonly IDiscordClientService _discordService;
        private readonly IMapDataCache _mapDataCache;
        private readonly IStaticsticsService _statsService;
        private readonly IBackgroundTaskQueue _taskQueue;

        public SubscriptionProcessorService(
            Microsoft.Extensions.Logging.ILogger<ISubscriptionProcessorService> logger,
            ISubscriptionManagerService subscriptionManager,
            ConfigHolder config,
            IDiscordClientService discordService,
            IMapDataCache mapDataCache,
            IStaticsticsService statsService,
            IBackgroundTaskQueue taskQueue)
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
            _config = config;
            _discordService = discordService;
            _mapDataCache = mapDataCache;
            _statsService = statsService;
            _taskQueue = (DefaultBackgroundTaskQueue)taskQueue;
        }

        #region Subscription Processing

        public async Task ProcessPokemonSubscriptionAsync(PokemonData pokemon)
        {
            if (!GameMaster.Instance.Pokedex.ContainsKey(pokemon.Id))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var pkmnCoord = new Coordinate(pokemon.Latitude, pokemon.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, pkmnCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var subscriptions = _subscriptionManager.GetSubscriptionsByPokemonId(pokemon.Id);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            DiscordMember member = null;
            var pkmn = GameMaster.GetPokemon(pokemon.Id, pokemon.FormId);
            var matchesIV = false;
            var matchesLvl = false;
            var matchesGender = false;
            var matchesIVList = false;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    try
                    {
                        member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error(ex.ToString());
                        continue;
                    }

                    // Check donor role access for Pokemon
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Pokemon))
                    {
                        _logger.Debug($"User {member?.Username} ({user.UserId}) is not a supporter, skipping pokemon {pkmn.Name}...");
                        continue;
                    }

                    var form = Translator.Instance.GetFormName(pokemon.FormId);
                    var pokemonSubscriptions = user.Pokemon.Where(x =>
                    {
                        var containsPokemon = x.PokemonId.Contains(pokemon.Id);
                        var isEmptyForm = /* TODO: Workaround for UI */ (x.Forms.Exists(y => string.IsNullOrEmpty(y)) && x.Forms.Count == 1);
                        var containsForm = (x.Forms?.Contains(form) ?? true) || x.Forms.Count == 0 || isEmptyForm;
                        return containsPokemon && containsForm;
                    });
                    if (pokemonSubscriptions == null)
                        continue;

                    foreach (var pkmnSub in pokemonSubscriptions)
                    {
                        matchesIV = Filters.MatchesIV(pokemon.IV, (uint)pkmnSub.MinimumIV, 100);
                        //var matchesCP = _whm.Filters.MatchesCpFilter(pkmn.CP, subscribedPokemon.MinimumCP);
                        matchesLvl = Filters.MatchesLvl(pokemon.Level, (uint)pkmnSub.MinimumLevel, (uint)pkmnSub.MaximumLevel);
                        matchesGender = Filters.MatchesGender(pokemon.Gender, pkmnSub.Gender);
                        //matchesIVList = pkmnSub.IVList?.Contains($"{pokemon.Attack}/{pokemon.Defense}/{pokemon.Stamina}") ?? false;
                        matchesIVList = IvListMatches(pkmnSub.IVList, pokemon);

                        // If no IV list specified check whole IV value, otherwise ignore whole IV value and only check IV list.
                        if (!(
                            (!pkmnSub.HasIVStats && matchesIV && matchesLvl && matchesGender) ||
                            (pkmnSub.HasIVStats && matchesIVList)
                            ))
                            continue;

                        /*
                        TODO: if (!(float.TryParse(pokemon.Height, out var height) && float.TryParse(pokemon.Weight, out var weight) && Filters.MatchesSize(pokemon.Id.GetSize(height, weight), pkmnSub.Size)))
                        {
                            // Pokemon doesn't match size
                            continue;
                        }
                        */

                        var geofence = GetGeofence(user.GuildId);
                        if (geofence == null)
                        {
                            //_logger.LogWarning($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!IsNearby(user, pkmnCoord, true, pkmnSub.Location, pkmnSub.Areas, geofence.Name.ToLower()))
                            continue;

                        var embed = await pokemon.GenerateEmbedMessageAsync(new AlarmMessageSettings
                        {
                            GuildId = user.GuildId,
                            Client = client,
                            Config = _config,
                            Alarm = null,
                            City = geofence.Name,
                            MapDataCache = _mapDataCache,
                        }).ConfigureAwait(false);

                        //var end = DateTime.Now.Subtract(start);
                        //_logger.LogDebug($"Took {end} to process Pokemon subscription for user {user.UserId}");
                        embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                        {
                            Subscription = user,
                            Member = member,
                            Embed = x.GenerateDiscordMessage(),
                            Description = pkmn.Name,
                            City = geofence.Name,
                            Pokemon = pokemon,
                        }));

                        _statsService.TotalPokemonSubscriptionsSent++;
                        Thread.Sleep(5);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            member = null;
            user = null;
            pokemon = null;

            await Task.CompletedTask;
        }

        public async Task ProcessPvpSubscriptionAsync(PokemonData pokemon)
        {
            if (!GameMaster.Instance.Pokedex.ContainsKey(pokemon.Id))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var pkmnCoord = new Coordinate(pokemon.Latitude, pokemon.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, pkmnCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var pkmn = GameMaster.GetPokemon(pokemon.Id, pokemon.FormId);
            var evolutionIds = pkmn.GetPokemonEvolutionIds();
            // PvP subscriptions support for evolutions not just base evo
            // Get evolution ids from masterfile for incoming pokemon, check if subscriptions for evo/base
            var subscriptions = _subscriptionManager.GetSubscriptionsByPvpPokemonId(evolutionIds);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            DiscordMember member = null;
            //var pkmn = MasterFile.GetPokemon(pokemon.Id, pokemon.FormId);
            var matchesGreat = false;
            var matchesUltra = false;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    try
                    {
                        member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error($"Error: {ex}");
                        continue;
                    }

                    // Check donor role access for PvP
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.PvP))
                    {
                        _logger.Debug($"User {member?.Username} ({user.UserId}) is not a supporter, skipping pvp pokemon {pkmn.Name}...");
                        continue;
                    }

                    var form = Translator.Instance.GetFormName(pokemon.FormId);
                    var pokemonSubscriptions = user.PvP.Where(x =>
                    {
                        var containsPokemon = x.PokemonId.Contains(pokemon.Id);
                        var isEmptyForm = /* TODO: Workaround for UI */ (x.Forms?.Exists(y => string.IsNullOrEmpty(y)) ?? false && x.Forms?.Count == 1);
                        var containsForm = (x.Forms?.Contains(form) ?? true) || (x.Forms?.Count ?? 0) == 0 || isEmptyForm;
                        return containsPokemon && containsForm;
                    });
                    if (pokemonSubscriptions == null)
                        continue;

                    foreach (var pkmnSub in pokemonSubscriptions)
                    {
                        var defaults = Strings.Defaults;
                        // Check if PvP ranks match any relevant great or ultra league ranks, if not skip.
                        matchesGreat = pokemon.GreatLeague != null && (pokemon.GreatLeague?.Exists(x =>
                        {
                            var cp = x.CP ?? 0;
                            var rank = x.Rank ?? 4096;
                            var matchesLeague = pkmnSub.League == PvpLeague.Great;
                            var matchesCP = cp >= defaults.MinimumGreatLeagueCP && cp <= defaults.MaximumGreatLeagueCP;
                            var matchesRank = rank <= pkmnSub.MinimumRank;
                            //var matchesPercentage = (x.Percentage ?? 0) * 100 >= pkmnSub.MinimumPercent;
                            return matchesLeague && matchesCP && matchesRank;
                        }) ?? false);
                        matchesUltra = pokemon.UltraLeague != null && (pokemon.UltraLeague?.Exists(x =>
                        {
                            var cp = x.CP ?? 0;
                            var rank = x.Rank ?? 4096;
                            var matchesLeague = pkmnSub.League == PvpLeague.Ultra;
                            var matchesCP = cp >= defaults.MinimumUltraLeagueCP && cp <= defaults.MaximumUltraLeagueCP;
                            var matchesRank = rank <= pkmnSub.MinimumRank;
                            //var matchesPercentage = (x.Percentage ?? 0) * 100 >= pkmnSub.MinimumPercent;
                            return matchesLeague && matchesCP && matchesRank;
                        }) ?? false);

                        // Skip if no relevent ranks for great and ultra league.
                        if (!matchesGreat && !matchesUltra)
                            continue;

                        var geofence = GetGeofence(user.GuildId);
                        if (geofence == null)
                        {
                            //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!IsNearby(user, pkmnCoord, true, pkmnSub.Location, pkmnSub.Areas, geofence.Name.ToLower()))
                            continue;

                        var embed = await pokemon.GenerateEmbedMessageAsync(new AlarmMessageSettings
                        {
                            GuildId = user.GuildId,
                            Client = client,
                            Config = _config,
                            Alarm = null,
                            City = geofence.Name,
                            MapDataCache = _mapDataCache,
                        }).ConfigureAwait(false);
                        //var end = DateTime.Now.Subtract(start);
                        //_logger.Debug($"Took {end} to process PvP subscription for user {user.UserId}");
                        embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                        {
                            Subscription = user,
                            Member = member,
                            Embed = x.GenerateDiscordMessage(),
                            Description = pkmn.Name,
                            City = geofence.Name,
                        }));

                        _statsService.TotalPvpSubscriptionsSent++;
                        Thread.Sleep(5);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            member = null;
            user = null;
            pokemon = null;

            await Task.CompletedTask;
        }

        public async Task ProcessRaidSubscriptionAsync(RaidData raid)
        {
            if (!GameMaster.Instance.Pokedex.ContainsKey(raid.PokemonId))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var raidCoord = new Coordinate(raid.Latitude, raid.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, raidCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var subscriptions = _subscriptionManager.GetSubscriptionsByRaidPokemonId(raid.PokemonId);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            var pokemon = GameMaster.GetPokemon(raid.PokemonId, raid.Form);
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    var member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warning($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Raids))
                    {
                        _logger.Information($"User {user.UserId} is not a supporter, skipping raid boss {pokemon.Name}...");
                        continue;
                    }

                    var form = Translator.Instance.GetFormName(raid.Form);
                    var pokemonSubscriptions = user.Raids.Where(x =>
                    {
                        var containsPokemon = x.PokemonId.Contains(raid.PokemonId);
                        var isEmptyForm = /* TODO: Workaround for UI */ (x.Forms.Exists(y => string.IsNullOrEmpty(y)) && x.Forms.Count == 1);
                        var containsForm = (x.Forms?.Contains(form) ?? true) || x.Forms.Count == 0 || isEmptyForm;
                        return containsPokemon && containsForm;
                    });
                    if (pokemonSubscriptions == null)
                        continue;

                    foreach (var subRaid in pokemonSubscriptions)
                    {
                        var geofence = GetGeofence(user.GuildId);
                        if (geofence == null)
                        {
                            //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                            continue;
                        }

                        if (!raid.IsExEligible && subRaid.IsExEligible)
                        {
                            // Skip raids that are not ex eligible when we want ex eligible raids
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!IsNearby(user, raidCoord, true, subRaid.Location, subRaid.Areas, geofence.Name.ToLower()))
                            continue;

                        var embed = await raid.GenerateEmbedMessageAsync(new AlarmMessageSettings
                        {
                            GuildId = user.GuildId,
                            Client = client,
                            Config = _config,
                            Alarm = null,
                            City = geofence.Name,
                            MapDataCache = _mapDataCache,
                        }).ConfigureAwait(false);
                        //var end = DateTime.Now;
                        //_logger.Debug($"Took {end} to process raid subscription for user {user.UserId}");
                        embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                        {
                            Subscription = user,
                            Member = member,
                            Embed = x.GenerateDiscordMessage(),
                            Description = pokemon.Name,
                            City = geofence.Name
                        }));

                        _statsService.TotalRaidSubscriptionsSent++;
                        Thread.Sleep(5);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;

            await Task.CompletedTask;
        }

        public async Task ProcessQuestSubscriptionAsync(QuestData quest)
        {
            var reward = quest.Rewards.FirstOrDefault().Info;
            var rewardKeyword = quest.GetReward();
            var questName = quest.GetQuestMessage();

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var questCoord = new Coordinate(quest.Latitude, quest.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, questCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var subscriptions = _subscriptionManager.GetSubscriptionsByQuest(quest.PokestopName, rewardKeyword);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    var member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warning($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    // Check donor role access for Quests
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Quests))
                    {
                        _logger.Information($"User {user.UserId} is not a supporter, skipping quest {questName}...");
                        continue;
                    }

                    var questSub = user.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()));
                    // Not subscribed to quest
                    if (questSub == null)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for quest {questName} because the quest is in city '{loc.Name}'.");
                        continue;
                    }

                    var geofence = GetGeofence(user.GuildId);
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                        continue;
                    }

                    var geofenceMatches = questSub.Areas.Select(x => x.ToLower()).Contains(geofence.Name.ToLower());

                    // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                    if (!IsNearby(user, questCoord, true, questSub.Location, questSub.Areas, geofence.Name.ToLower()))
                        continue;

                    var embed = await quest.GenerateEmbedMessageAsync(new AlarmMessageSettings
                    {
                        GuildId = user.GuildId,
                        Client = client,
                        Config = _config,
                        Alarm = null,
                        City = geofence.Name,
                        MapDataCache = _mapDataCache,
                    }).ConfigureAwait(false);
                    //var end = DateTime.Now.Subtract(start);
                    //_logger.Debug($"Took {end} to process quest subscription for user {user.UserId}");
                    embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                    {
                        Subscription = user,
                        Member = member,
                        Embed = x.GenerateDiscordMessage(),
                        Description = questName,
                        City = geofence.Name,
                    }));

                    _statsService.TotalQuestSubscriptionsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;

            await Task.CompletedTask;
        }

        public async Task ProcessInvasionSubscriptionAsync(PokestopData pokestop)
        {
            if (pokestop.GruntType == InvasionCharacter.CharacterUnset)
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var invasionCoord = new Coordinate(pokestop.Latitude, pokestop.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, invasionCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var invasion = GameMaster.Instance.GruntTypes?.ContainsKey(pokestop.GruntType) ?? false ? GameMaster.Instance.GruntTypes[pokestop.GruntType] : null;
            var encounters = invasion?.GetEncounterRewards();
            if (encounters == null)
                return;

            var subscriptions = _subscriptionManager.GetSubscriptionsByInvasion(pokestop?.Name, pokestop?.GruntType ?? InvasionCharacter.CharacterUnset, encounters);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            if (!GameMaster.Instance.GruntTypes.ContainsKey(pokestop.GruntType))
            {
                //_logger.Error($"Failed to parse grunt type {pokestop.GruntType}, not in `grunttype.json` list.");
                return;
            }

            Subscription user;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    var member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warning($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    // Check donor role access for Invasions
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Invasions))
                    {
                        _logger.Information($"User {user.UserId} is not a supporter, skipping Team Rocket invasion {pokestop.Name}...");
                        continue;
                    }

                    var invasionSub = user.Invasions.FirstOrDefault(x => x.RewardPokemonId.Intersects(encounters));
                    // Not subscribed to invasion
                    if (invasionSub == null)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is in city '{loc.Name}'.");
                        continue;
                    }

                    var geofence = GetGeofence(user.GuildId);
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                        continue;
                    }

                    // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                    if (!IsNearby(user, invasionCoord, true, invasionSub.Location, invasionSub.Areas, geofence.Name.ToLower()))
                        continue;

                    var embed = await pokestop.GenerateEmbedMessageAsync(new AlarmMessageSettings
                    {
                        GuildId = user.GuildId,
                        Client = client,
                        Config = _config,
                        Alarm = null,
                        City = geofence?.Name,
                        MapDataCache = _mapDataCache,
                    }).ConfigureAwait(false);
                    //var end = DateTime.Now.Subtract(start);
                    //_logger.LogDebug($"Took {end} to process invasion subscription for user {user.UserId}");
                    embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                    {
                        Subscription = user,
                        Member = member,
                        Embed = x.GenerateDiscordMessage(),
                        Description = pokestop.Name,
                        City = geofence.Name,
                    }));

                    _statsService.TotalInvasionSubscriptionsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;

            await Task.CompletedTask;
        }

        public async Task ProcessLureSubscriptionAsync(PokestopData pokestop)
        {
            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var lureCoord = new Coordinate(pokestop.Latitude, pokestop.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, lureCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var subscriptions = _subscriptionManager.GetSubscriptionsByLure(pokestop.Name, pokestop.LureType);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    var member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warning($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    // Check donor role access for Lures
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Lures))
                    {
                        _logger.Information($"User {user.UserId} is not a supporter, skipping Pokestop lure {pokestop.Name}...");
                        continue;
                    }

                    var lureSub = user.Lures.FirstOrDefault(x => x.LureType?.Contains(pokestop.LureType) ?? false);
                    // Not subscribed to lure
                    if (lureSub == null)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for Pokestop lure {pokemon.Name}, lure is in city '{loc.Name}'.");
                        continue;
                    }

                    var geofence = GetGeofence(user.GuildId);
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to lookup city from coordinates {pokestop.Latitude},{pokestop.Longitude} {pokestop.PokestopId} {pokestop.Name}, skipping...");
                        continue;
                    }

                    // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                    if (!IsNearby(user, lureCoord, true, lureSub.Location, lureSub.Areas, geofence.Name.ToLower()))
                        continue;

                    var embed = await pokestop.GenerateEmbedMessageAsync(new AlarmMessageSettings
                    {
                        GuildId = user.GuildId,
                        Client = client,
                        Config = _config,
                        Alarm = null,
                        City = geofence.Name,
                        MapDataCache = _mapDataCache,
                    }).ConfigureAwait(false);
                    //var end = DateTime.Now.Subtract(start);
                    //_logger.Debug($"Took {end} to process lure subscription for user {user.UserId}");
                    embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                    {
                        Subscription = user,
                        Member = member,
                        Embed = x.GenerateDiscordMessage(),
                        Description = pokestop.Name,
                        City = geofence.Name,
                    }));

                    _statsService.TotalLureSubscriptionsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;

            await Task.CompletedTask;
        }

        public async Task ProcessGymSubscriptionAsync(RaidData raid)
        {
            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var gymCoord = new Coordinate(raid.Latitude, raid.Longitude);
            Geofence GetGeofence(ulong guildId)
            {
                if (!locationCache.TryGetValue(guildId, out var geofence))
                {
                    var geofences = _config.Instance.Servers[guildId].Geofences;
                    geofence = GeofenceService.GetGeofence(geofences, gymCoord);
                    locationCache.Add(guildId, geofence);
                }
                return geofence;
            }

            var subscriptions = _subscriptionManager.GetSubscriptionsByGymName(raid.GymName);
            if (subscriptions == null || subscriptions?.Count == 0)
            {
                //_logger.Warning($"Failed to get subscriptions from database table.");
                return;
            }

            Subscription user;
            var pokemon = GameMaster.GetPokemon(raid.PokemonId, raid.Form);
            for (var i = 0; i < subscriptions.Count; i++)
            {
                //var start = DateTime.Now;
                try
                {
                    user = subscriptions[i];

                    // Skip if user's guild is not configured or connected
                    if (!DiscordExists(user.GuildId, _config.Instance.Servers, _discordService.DiscordClients))
                        continue;

                    var client = _discordService.DiscordClients[user.GuildId];

                    var member = await client.GetMemberByIdAsync(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warning($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    // Check donor role access for Gyms
                    if (!IsSubscriberValid(member, _config.Instance.Servers[user.GuildId].DonorRoleIds, SubscriptionAccessType.Gyms))
                    {
                        _logger.Information($"User {user.UserId} is not a supporter, skipping raid boss {pokemon.Name} for gym {raid.GymName}...");
                        continue;
                    }

                    var geofence = GetGeofence(user.GuildId);
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                        continue;
                    }

                    var gymSub = user.Gyms.FirstOrDefault(x => string.Compare(x.Name, raid.GymName, true) == 0);
                    if (gymSub == null)
                        continue;

                    var checkLevel = gymSub.MinimumLevel > 0 && gymSub.MaximumLevel > 0;
                    var containsPokemon = gymSub.PokemonIDs?.Contains(raid.PokemonId) ?? false;
                    // Skip if neither level or Pokemon matches raid
                    if (!checkLevel && !containsPokemon)
                        continue;

                    if (!raid.IsExEligible && gymSub.IsExEligible)
                    {
                        // Skip raids that are not ex eligible when we want ex eligible raids
                        continue;
                    }

                    // Skip if not nearby or within set global location or individual subscription locations
                    if (!IsNearby(user, gymCoord, checkGeofence: false))
                        continue;

                    var embed = await raid.GenerateEmbedMessageAsync(new AlarmMessageSettings
                    {
                        GuildId = user.GuildId,
                        Client = client,
                        Config = _config,
                        Alarm = null,
                        City = geofence.Name,
                        MapDataCache = _mapDataCache,
                    }).ConfigureAwait(false);
                    //var end = DateTime.Now;
                    //_logger.Debug($"Took {end} to process gym raid subscription for user {user.UserId}");
                    embed.Embeds.ForEach(async x => await EnqueueEmbedAsync(new NotificationItem
                    {
                        Subscription = user,
                        Member = member,
                        Embed = x.GenerateDiscordMessage(),
                        Description = pokemon.Name,
                        City = geofence.Name,
                    }));

                    _statsService.TotalGymSubscriptionsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error: {ex}");
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;

            await Task.CompletedTask;
        }

        #endregion

        private static bool IvListMatches(List<string> ivList, PokemonData pokemon)
        {
            var matches = ivList?.Contains($"{pokemon.Attack}/{pokemon.Defense}/{pokemon.Stamina}") ?? false;
            var matchesWildcard = ivList?.Exists(iv =>
            {
                var split = iv.Split('/');

                // Ensure user specified all IV parts required
                if (split.Length != 3)
                    return false;

                // Validate IV list entry is a valid integer and no wild cards specified.
                if (!ushort.TryParse(split[0], out var attack) && split[0] != "*")
                    return false;

                if (!ushort.TryParse(split[1], out var defense) && split[1] != "*")
                    return false;

                if (!ushort.TryParse(split[2], out var stamina) && split[2] != "*")
                    return false;

                // Check if individual values are the same or if wildcard is specified.
                var matches =
                    attack == pokemon.Attack || split[0] == "*" &&
                    defense == pokemon.Defense || split[1] == "*" &&
                    stamina == pokemon.Stamina || split[2] == "*";
                return matches;

            }) ?? false;
            return matches || matchesWildcard;
        }

        // TODO: ISubscriptionLocation (string location, List<string> areas)
        private static bool IsNearby(Subscription user, Coordinate coord, bool checkGeofence = false, string webhookLocationName = null, List<string> areas = null, string geofenceName = null)
        {
            var globalLocation = user.Locations?.FirstOrDefault(x => string.Compare(x.Name, user.Location, true) == 0);
            var webhookLocation = user.Locations?.FirstOrDefault(x => string.Compare(x.Name, webhookLocationName, true) == 0);
            var globalDistanceMatches = globalLocation?.DistanceM > 0
                && globalLocation?.DistanceM > new Coordinate(globalLocation?.Latitude ?? 0, globalLocation?.Longitude ?? 0).DistanceTo(coord);
            var webhookDistanceMatches = webhookLocation?.DistanceM > 0
                && webhookLocation?.DistanceM > new Coordinate(webhookLocation?.Latitude ?? 0, webhookLocation?.Longitude ?? 0).DistanceTo(coord);

            // Skip if set distance does not match and no geofences match...
            var matchesLocation = globalDistanceMatches || webhookDistanceMatches;
            if (checkGeofence)
            {
                var geofenceNameLower = geofenceName.ToLower();
                var matchesGeofence = areas?.Select(x => x.ToLower())
                                            .Contains(geofenceNameLower) ?? false;
                return matchesGeofence || matchesLocation;
            }
            return matchesLocation;
        }

        private static bool DiscordExists(ulong guildId, Dictionary<ulong, DiscordServerConfig> servers, IReadOnlyDictionary<ulong, DiscordClient> discordClients)
        {
            if (!servers.ContainsKey(guildId))
                return false;

            if (!servers[guildId].Subscriptions.Enabled)
                return false;

            if (!discordClients.ContainsKey(guildId))
                return false;

            return true;
        }

        private static bool IsSubscriberValid(DiscordMember member, Dictionary<ulong, IEnumerable<SubscriptionAccessType>> donorRoleIds, SubscriptionAccessType accessType)
        {
            if (member?.Roles == null)
            {
                return false;
            }

            // Check if member has donor role
            if (!member.HasSupporterRole(donorRoleIds.Keys.ToList()))
            {
                return false;
            }

            // Check donor role access for subscription access type
            if (!member.HasRoleAccess(donorRoleIds, accessType))
            {
                return false;
            }

            return true;
        }

        #region Background Service

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Information(
                $"{nameof(SubscriptionProcessorService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information(
                $"{nameof(SubscriptionProcessorService)} is now running in the background.");

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
                    var tasks = workItems.Select(item => Task.Factory.StartNew(async () => await item(stoppingToken)));
                    Task.WaitAll(tasks.ToArray(), stoppingToken);
                    /*
                    foreach (var workItem in workItems)
                    {
                        await workItem(stoppingToken);
                    }
                    */
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error occurred executing task work item.");
                }
                //await Task.Delay(10, stoppingToken);
                Thread.Sleep(10);
            }

            _logger.Error("Exited background processing...");
        }

        private async Task EnqueueEmbedAsync(NotificationItem embed)
        {
            //CheckQueueLength();

            await _taskQueue.EnqueueAsync(async token =>
                await ProcessWorkItemAsync(embed, token));
        }

        private async Task<CancellationToken> ProcessWorkItemAsync(
            NotificationItem embed,
            CancellationToken stoppingToken)
        {
            if (embed?.Subscription == null || embed?.Member == null || embed?.Embed == null)
                return stoppingToken;

            if (!_discordService.DiscordClients.ContainsKey(embed.Subscription.GuildId))
            {
                _logger.Error($"User subscription for guild that's not configured. UserId={embed.Subscription.UserId} GuildId={embed.Subscription.GuildId}");
                return stoppingToken;
            }

            // Check if user is receiving messages too fast.
            if (!_config.Instance.Servers.ContainsKey(embed.Subscription.GuildId))
            {
                // Config does not contain subscription guild for some reason o.O
                return stoppingToken;
            }

            CheckQueueLength();

            var config = _config.Instance.Servers[embed.Subscription.GuildId];
            var maxNotificationsPerMinute = config.Subscriptions.MaxNotificationsPerMinute;
            if (embed.Subscription.Limiter.IsLimited(maxNotificationsPerMinute))
            {
                await SendRateLimitedMessage(embed, maxNotificationsPerMinute);
                return stoppingToken;
            }

            // Ratelimit is up, allow for ratelimiting again
            embed.Subscription.RateLimitNotificationSent = false;

            // Send text message notification to user if a phone number is set
            /* TODO: Twilio notifications
            if (_config.Instance.Twilio.Enabled && !string.IsNullOrEmpty(embed.Subscription.PhoneNumber))
            {
                // Check if user is in the allowed text message list or server owner
                if (HasRole(embed.Member, _config.Instance.Twilio.RoleIds) ||
                    _config.Instance.Twilio.UserIds.Contains(embed.Member.Id) ||
                    _config.Instance.Servers[embed.Subscription.GuildId].Bot.OwnerId == embed.Member.Id)
                {
                    // Send text message (max 160 characters)
                    if (embed.Pokemon != null && IsUltraRare(_config.Instance.Twilio, embed.Pokemon))
                    {
                        // TODO: Generate SMS message string from embed
                        var result = Utils.SendSmsMessage(StripEmbed(embed), _config.Instance.Twilio, embed.Subscription.PhoneNumber);
                        if (!result)
                        {
                            _logger.Error($"Failed to send text message to phone number '{embed.Subscription.PhoneNumber}' for user {embed.Subscription.UserId}");
                        }
                    }
                }
            }
            */

            // Send direct message notification to user
            await embed.Member.SendDirectMessageAsync(string.Empty, embed.Embed);
            _logger.Information($"[WEBHOOK] Notified user {embed.Member.Username} of {embed.Description}.");
            Thread.Sleep(1);

            return stoppingToken;
        }

        private async Task SendRateLimitedMessage(NotificationItem embed, uint maxNotificationsPerMinute)
        {
            _logger.Warning($"{embed.Member.Username} notifications rate limited, waiting {(60 - embed.Subscription.Limiter.TimeLeft.TotalSeconds)} seconds...", embed.Subscription.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
            // Send ratelimited notification to user if not already sent to adjust subscription settings to more reasonable settings.
            if (!embed.Subscription.RateLimitNotificationSent)
            {
                if (!_discordService.DiscordClients.ContainsKey(embed.Subscription.GuildId))
                    return;

                var client = _discordService.DiscordClients[embed.Subscription.GuildId].Guilds[embed.Subscription.GuildId];
                var emoji = DiscordEmoji.FromName(_discordService.DiscordClients.FirstOrDefault().Value, ":no_entry:");
                var guildIconUrl = _discordService.DiscordClients.ContainsKey(embed.Subscription.GuildId) ? client?.IconUrl : string.Empty;
                // TODO: Localize rate limited messaged
                var rateLimitMessage = $"{emoji} Your notification subscriptions have exceeded {maxNotificationsPerMinute:N0}) per minute and are now being rate limited." +
                                       $"Please adjust your subscriptions to receive a maximum of {maxNotificationsPerMinute:N0} notifications within a {NotificationLimiter.ThresholdTimeout} second time span.";
                var eb = new DiscordEmbedBuilder
                {
                    Title = "Rate Limited",
                    Description = rateLimitMessage,
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{client?.Name} | {DateTime.Now}",
                        IconUrl = guildIconUrl,
                    }
                };

                await embed.Member.SendDirectMessageAsync(eb.Build());
                embed.Subscription.RateLimitNotificationSent = true;

                await _subscriptionManager.SetSubscriptionStatusAsync(embed.Subscription, NotificationStatusType.None);
            }
        }

        #endregion

        private void CheckQueueLength()
        {
            if (_taskQueue.Count > Strings.Defaults.MaximumQueueSizeWarning)
            {
                _logger.Warning($"Subscription queue is {_taskQueue.Count:N0} items long.");
            }
        }
    }
}