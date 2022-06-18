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
        private readonly Dictionary<int, bool> _rateLimitedMessagesSent; // subscription_id -> rateLimitedMessageSent

        // TODO: Use BenchmarkTimes property
        public bool BenchmarkTimes { get; set; }

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
            _rateLimitedMessagesSent = new Dictionary<int, bool>();
        }

        #region Subscription Processing

        public async Task ProcessPokemonSubscriptionAsync(PokemonData pokemon)
        {
            if (!IsValidPokedexPokemon(pokemon.Id))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var pkmnCoord = new Coordinate(pokemon);
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
            var matchesCP = false;
            var matchesLvl = false;
            var matchesGender = false;
            var matchesIVList = false;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    var start = DateTime.Now;
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
                    var pokemonSubscriptions = GetFilteredPokemonSubscriptions((HashSet<PokemonSubscription>)user.Pokemon, pokemon.Id, form);
                    if (pokemonSubscriptions == null)
                        continue;

                    foreach (var pkmnSub in pokemonSubscriptions)
                    {
                        matchesIV = Filters.MatchesIV(pokemon.IV, (uint)pkmnSub.MinimumIV, 100);
                        matchesCP = Filters.MatchesCP(pokemon.CP, (uint)pkmnSub.MinimumCP, (uint)pkmnSub.MaximumCP);
                        matchesLvl = Filters.MatchesLvl(pokemon.Level, (uint)pkmnSub.MinimumLevel, (uint)pkmnSub.MaximumLevel);
                        matchesGender = Filters.MatchesGender(pokemon.Gender, pkmnSub.Gender);
                        matchesIVList = IvListMatches(pkmnSub.IVList, pokemon);

                        if (!matchesCP)
                            continue;

                        // If no IV list specified check whole IV value, otherwise ignore whole IV value and only check IV list.
                        if (!(
                            (!pkmnSub.HasIVStats && matchesIV && matchesLvl && matchesGender) ||
                            (pkmnSub.HasIVStats && matchesIVList)
                            ))
                            continue;

                        var pokemonSize = pokemon.Id.GetSize(pokemon.Height ?? 0, pokemon.Weight ?? 0);
                        if (!pokemon.IsMissingStats && pkmn.Height != null && pkmn.Weight != null
                            && !Filters.MatchesSize(pokemonSize, pkmnSub.Size))
                        {
                            // Pokemon doesn't match size
                            continue;
                        }

                        var geofence = GetGeofence(user.GuildId);
                        if (geofence == null)
                        {
                            //_logger.LogWarning($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!user.IsNearby(pkmnCoord, true, pkmnSub.Location, pkmnSub.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process Pokemon subscription for user {user.UserId}");
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
            if (!IsValidPokedexPokemon(pokemon.Id))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var pkmnCoord = new Coordinate(pokemon);
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
            for (var i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    var start = DateTime.Now;
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
                    var pokemonSubscriptions = GetFilteredPokemonSubscriptions((HashSet<PvpSubscription>)user.PvP, pokemon.Id, form, evolutionIds);
                    if (pokemonSubscriptions == null)
                        continue;

                    var validPvpLeagues = Startup.Config.PvpLeagues;
                    foreach (var pkmnSub in pokemonSubscriptions)
                    {
                        // TODO: Combine filtered and matchesAny

                        // Filter Pokemon PvP rankings based on user subscription settings
                        var filtered = pokemon.PvpRankings?.Where(pvp =>
                        {
                            (PvpLeague pokemonPvpLeague, List<PvpRankData> ranks) = pvp;

                            // Skip if PvP subscription's league does not match set Pokemon rank league
                            if (pokemonPvpLeague != pkmnSub.League)
                                return false;

                            // Only return PvP subscriptions that are equal to the Pokemon's rank
                            return ranks.Exists(rank => pkmnSub.PokemonId.Contains(rank.PokemonId));
                        }).ToList();

                        // Skip any pvp ranks that do not match evolutions
                        if (filtered.Count == 0)
                            continue;

                        // Check if PvP ranks match any relevant great or ultra league ranks, if not skip.
                        var matchesAny = filtered.Any(pvp =>
                        {
                            (PvpLeague league, List<PvpRankData> ranks) = pvp;

                            // Check if league set in config
                            if (!validPvpLeagues.ContainsKey(league))
                                return false;

                            return pkmnSub.RankExists(ranks, league, validPvpLeagues[league]);
                        });

                        // Skip if no relevent ranks for set PvP leagues.
                        if (!matchesAny)
                            continue;

                        var geofence = GetGeofence(user.GuildId);
                        if (geofence == null)
                        {
                            //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!user.IsNearby(pkmnCoord, true, pkmnSub.Location, pkmnSub.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process PvP subscription for user {user.UserId}");
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
            if (!IsValidPokedexPokemon(raid.PokemonId))
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var raidCoord = new Coordinate(raid);
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
                try
                {
                    var start = DateTime.Now;
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
                    var pokemonSubscriptions = GetFilteredPokemonSubscriptions((HashSet<RaidSubscription>)user.Raids, raid.PokemonId, form);
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
                            // Skip raids that are not ex eligible when we only want ex eligible raids
                            continue;
                        }

                        // Skip if not nearby or within set global location, individual subscription locations, or geofence does not match
                        if (!user.IsNearby(raidCoord, true, subRaid.Location, subRaid.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process raid subscription for user {user.UserId}");
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
            var questCoord = new Coordinate(quest);
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
                try
                {
                    var start = DateTime.Now;
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
                    if (!user.IsNearby(questCoord, true, questSub.Location, questSub.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process quest subscription for user {user.UserId}");
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

        public async Task ProcessInvasionSubscriptionAsync(PokestopData pokestop)
        {
            if (pokestop.GruntType == InvasionCharacter.CharacterUnset)
                return;

            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var invasionCoord = new Coordinate(pokestop);
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
                try
                {
                    var start = DateTime.Now;
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
                    if (!user.IsNearby(invasionCoord, true, invasionSub.Location, invasionSub.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process invasion subscription for user {user.UserId}");
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

        public async Task ProcessLureSubscriptionAsync(PokestopData pokestop)
        {
            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var lureCoord = new Coordinate(pokestop);
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
                try
                {
                    var start = DateTime.Now;
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
                    if (!user.IsNearby(lureCoord, true, lureSub.Location, lureSub.Areas, geofence.Name.ToLower()))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now.Subtract(start);
                        _logger.Debug($"Took {end} to process lure subscription for user {user.UserId}");
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

        public async Task ProcessGymSubscriptionAsync(RaidData raid)
        {
            // Cache the result per-guild so that geospatial stuff isn't queried for every single subscription below
            var locationCache = new Dictionary<ulong, Geofence>();
            var gymCoord = new Coordinate(raid);
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
                try
                {
                    var start = DateTime.Now;
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
                    if (!user.IsNearby(gymCoord, checkGeofence: false))
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

                    if (BenchmarkTimes)
                    {
                        var end = DateTime.Now;
                        _logger.Debug($"Took {end} to process gym raid subscription for user {user.UserId}");
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

        #endregion

        // TODO: Move helpers to extensions class
        #region Helper Methods

        private bool IsValidPokedexPokemon(uint pokemonId)
        {
            if (pokemonId == 0)
            {
                return false;
            }

            if (!GameMaster.Instance.Pokedex.ContainsKey(pokemonId))
            {
                _logger.Warning($"Pokemon '{pokemonId}' does not exist in 'masterfile.json', please make sure you're using an up to date version.");
                return false;
            }
            return true;
        }

        private static IEnumerable<T> GetFilteredPokemonSubscriptions<T>(HashSet<T> subscriptions, uint pokemonId, string form, List<uint> evolutionIds = null)
            where T : BasePokemonSubscription
        {
            var pokemonSubscriptions = subscriptions.Where(sub =>
            {
                var containsPokemon = evolutionIds != null
                    // If evolutionIds is set, check if subscription's pokemonId matches evolutionId
                    ? sub.PokemonId.Any(id => evolutionIds.Contains(id))
                    // Otherwise check if subscription's pokemonId matches webhook pokemonId (base evo)
                    : sub.PokemonId.Contains(pokemonId);
                var isEmptyForm = /* TODO: Workaround for UI */ (sub.Forms?.Exists(y => string.IsNullOrEmpty(y)) ?? false && sub.Forms?.Count == 1);
                var containsForm = (sub.Forms?.Contains(form) ?? true) || (sub.Forms?.Count ?? 0) == 0 || isEmptyForm;
                return containsPokemon && containsForm;
            });
            return pokemonSubscriptions;
        }

        private static bool IvWildcardMatches(string ivEntry, ushort? pokemonIvEntry)
        {
            // Skip IV ranges
            if (ivEntry.Contains("-"))
            {
                return false;
            }

            // Return true if wildcard specified.
            if (ivEntry == "*")
            {
                return true;
            }

            // Validate IV entry is a valid integer.
            if (!ushort.TryParse(ivEntry, out var ivValue))
            {
                return false;
            }

            // Check if IV entry matches Pokemon IV.
            return ivValue == pokemonIvEntry;
        }

        private static bool IvListMatches(List<string> ivList, PokemonData pokemon)
        {
            // Check if IV list is null or no entries and Pokemon has IV values, otherwise return false.
            if (ivList?.Count == 0 ||
                pokemon.Attack == null ||
                pokemon.Defense == null ||
                pokemon.Stamina == null)
            {
                return false;
            }

            // Construct expected formatted IV entry string
            var ivEntry = $"{pokemon.Attack}/{pokemon.Defense}/{pokemon.Stamina}";

            // Check if IV matches any IV list range or wildcard entries
            var matches = ivList?.Exists(iv =>
            {
                // Check if IV list entries matches Pokemon IV string verbatim
                if (string.Equals(iv, ivEntry))
                {
                    return true;
                }

                var split = iv.Split('/');

                // Ensure user specified all IV parts required
                if (split.Length != 3)
                    return false;

                var ivAttack = split[0];
                var ivDefense = split[1];
                var ivStamina = split[2];

                var matchesWildcard =
                    IvWildcardMatches(ivAttack, pokemon.Attack) &&
                    IvWildcardMatches(ivDefense, pokemon.Defense) &&
                    IvWildcardMatches(ivStamina, pokemon.Stamina);

                var matchesRange = IvRangeMatches(ivAttack, ivDefense, ivStamina, pokemon);
                return matchesWildcard || matchesRange;
            }) ?? false;

            return matches;
        }

        private static bool IvRangeMatches(string ivAttack, string ivDefense, string ivStamina, PokemonData pokemon)
        {
            if (pokemon.Attack == null ||
                pokemon.Defense == null ||
                pokemon.Stamina == null)
            {
                return false;
            }

            // Check if none of the IV entries contain range indicator
            if (!ivAttack.Contains("-") &&
                !ivDefense.Contains("-") &&
                !ivStamina.Contains("-"))
            {
                return false;
            }

            // Parse min/max IV values for all entries
            var (minAttack, maxAttack) = ParseMinMaxValues(ivAttack);
            var (minDefense, maxDefense) = ParseMinMaxValues(ivDefense);
            var (minStamina, maxStamina) = ParseMinMaxValues(ivStamina);

            // Check if Pokemon IV is within min/max range
            var matches = (pokemon.Attack ?? 0) >= minAttack && (pokemon.Attack ?? 0) <= maxAttack &&
                          (pokemon.Defense ?? 0) >= minDefense && (pokemon.Defense ?? 0) <= maxDefense &&
                          (pokemon.Stamina ?? 0) >= minStamina && (pokemon.Stamina ?? 0) <= maxStamina;

            return matches;
        }

        private static (ushort, ushort) ParseRangeEntry(string ivEntry)
        {
            // Parse IV range min/max values
            var split = ivEntry.Split('-');

            // If count mismatch, skip
            if (split.Length != 2)
            {
                return default;
            }

            // Parse first range value for minimum
            if (!ushort.TryParse(split[0], out var minRange))
            {
                return default;
            }

            // Parse second range value for maximum
            if (!ushort.TryParse(split[1], out var maxRange))
            {
                return default;
            }
            return (minRange, maxRange);
        }

        private static (ushort, ushort) ParseMinMaxValues(string ivEntry)
        {
            ushort minRange;
            ushort maxRange;
            if (ivEntry.Contains("-"))
            {
                // Parse min/max range values
                var (min, max) = ParseRangeEntry(ivEntry);
                minRange = min;
                maxRange = max;
            }
            // Check if attack IV contains wildcard, otherwise value should be a whole value
            else if (ivEntry.Contains("*"))
            {
                // Wildcard specified, set min/max to 0-15
                minRange = 0;
                maxRange = 15;
            }
            else
            {
                // No range indicator found for attack IV, parse and assign whole IV value to min/max values
                var atk = ushort.Parse(ivEntry);
                minRange = atk;
                maxRange = atk;
            }
            return (minRange, maxRange);
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

        #endregion

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
            var subscriptionId = embed.Subscription.Id;

            if (!_rateLimitedMessagesSent.ContainsKey(subscriptionId))
            {
                _rateLimitedMessagesSent.Add(subscriptionId, false);
            }

            if (embed.Subscription.Limiter.IsLimited(maxNotificationsPerMinute) && !_rateLimitedMessagesSent[subscriptionId])//!subscription.RateLimitNotificationSent)
            {
                // Disable subscription immediately
                await _subscriptionManager.SetSubscriptionStatusAsync(subscriptionId, NotificationStatusType.None);

                await SendRateLimitedMessage(embed.Subscription, embed.Member, maxNotificationsPerMinute);
                //return stoppingToken;
            }
            else
            {
                // Ratelimit is up, allow for ratelimiting again
                _rateLimitedMessagesSent[subscriptionId] = false;

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
                _logger.Information($"[SUBSCRIPTION] Notified user {embed.Member.Username} of {embed.Description}.");
                Thread.Sleep(1);
            }

            return stoppingToken;
        }

        private async Task SendRateLimitedMessage(Subscription subscription, DiscordMember member, uint maxNotificationsPerMinute)
        {
            _logger.Warning($"{member.Username} notifications rate limited, waiting {(60 - subscription.Limiter.TimeLeft.TotalSeconds)} seconds...", subscription.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
            // Send ratelimited notification to user if not already sent to adjust subscription settings to more reasonable settings.
            if (!_rateLimitedMessagesSent[subscription.Id])
            {
                if (!_discordService.DiscordClients.ContainsKey(subscription.GuildId))
                    return;

                var client = _discordService.DiscordClients[subscription.GuildId].Guilds[subscription.GuildId];
                var emoji = DiscordEmoji.FromName(_discordService.DiscordClients.FirstOrDefault().Value, ":no_entry:");
                var guildIconUrl = _discordService.DiscordClients.ContainsKey(subscription.GuildId) ? client?.IconUrl : string.Empty;
                // TODO: Localize rate limited message
                var rateLimitMessage = $"{emoji} Your notification subscriptions have exceeded {maxNotificationsPerMinute:N0} per minute and are now being rate limited. " +
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

                await member.SendDirectMessageAsync(eb.Build());
                _rateLimitedMessagesSent[subscription.Id] = true;
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
