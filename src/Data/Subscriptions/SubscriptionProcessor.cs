namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using ServiceStack.OrmLite;

    using WhMgr.Alarms.Filters;
    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Net.Models;
    using WhMgr.Net.Webhooks;
    using Utils = WhMgr.Utilities.Utils;

    /// <summary>
    /// Discord user subscription processing class.
    /// </summary>
    public class SubscriptionProcessor
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("SUBSCRIPTION", Program.LogLevel);

        private readonly Dictionary<ulong, DiscordClient> _servers;
        private readonly WhConfig _whConfig;
        private readonly WebhookController _whm;
        private readonly NotificationQueue _queue;

        #endregion

        #region Properties

        /// <summary>
        /// Get subscription manager class
        /// </summary>
        public SubscriptionManager Manager { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="SubscriptionProcessor"/> class.
        /// </summary>
        /// <param name="servers">Discord servers dictionary</param>
        /// <param name="config">Configuration file</param>
        /// <param name="whm">Webhook controller class</param>
        public SubscriptionProcessor(Dictionary<ulong, DiscordClient> servers, WhConfig config, WebhookController whm)
        {
            _logger.Trace($"SubscriptionProcessor::SubscriptionProcessor");

            _servers = servers;
            _whConfig = config;
            _whm = whm;
            _queue = new NotificationQueue();

            Manager = new SubscriptionManager(_whConfig);

            ProcessQueue();
        }

        #endregion

        #region Public Methods

        public async Task ProcessPokemonSubscription(PokemonData pkmn)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(pkmn.Id))
                return;

            var loc = _whm.GetGeofence(pkmn.Latitude, pkmn.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                return;
            }

            var subscriptions = Manager.GetUserSubscriptionsByPokemonId(pkmn.Id);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            SubscriptionObject user;
            PokemonSubscription subscribedPokemon;
            DiscordMember member = null;
            var pokemon = MasterFile.GetPokemon(pkmn.Id, pkmn.FormId);
            var matchesIV = false;
            var matchesLvl = false;
            var matchesGender = false;
            var matchesIVList = false;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    if (!_whConfig.Servers.ContainsKey(user.GuildId))
                        continue;

                    if (!_whConfig.Servers[user.GuildId].EnableSubscriptions)
                        continue;

                    if (!_servers.ContainsKey(user.GuildId))
                        continue;

                    var client = _servers[user.GuildId];

                    try
                    {
                        member = await client.GetMemberById(user.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error(ex);
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.Servers[user.GuildId].DonorRoleIds))
                    {
                        _logger.Debug($"User {member?.Username} ({user.UserId}) is not a supporter, skipping pokemon {pokemon.Name}...");
                        continue;
                    }

                    if (member?.Roles == null || loc == null)
                        continue;

                    if (!member.Roles.Select(x => x?.Name?.ToLower()).Contains(loc?.Name?.ToLower()))
                    {
                        //_logger.Info($"User {member.Username} does not have city role {loc.Name}, skipping pokemon {pokemon.Name}.");
                        continue;
                    }

                    /*
                    var exists = user.Pokemon.Exists(x => string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0));
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for Pokemon {pokemon.PokemonId} because the Pokemon is in city '{loc.Name}'.");
                        continue;
                    }
                    */

                    // Only check distance if user has it set
                    if (user.DistanceM > 0)
                    {
                        var distance = new Coordinates(user.Latitude, user.Longitude).DistanceTo(new Coordinates(pkmn.Latitude, pkmn.Longitude));
                        if (user.DistanceM < distance)
                        {
                            //Skip if distance is set and is not with specified distance.
                            _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for Pokemon {pokemon.Name}, Pokemon is farther than set distance of '{user.DistanceM} meters.");
                            continue;
                        }
                    }

                    var form = Translator.Instance.GetFormName(pkmn.FormId);
                    subscribedPokemon = user.Pokemon.FirstOrDefault(x =>
                        x.PokemonId == pkmn.Id &&
                        (string.IsNullOrEmpty(x.Form) || string.Compare(x.Form, form, true) == 0) &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    );
                    if (subscribedPokemon == null)
                    {
                        //_logger.Info($"User {member.Username} not subscribed to Pokemon {pokemon.Name} (Form: {form}).");
                        continue;
                    }

                    matchesIV = Filters.MatchesIV(pkmn.IV, subscribedPokemon.MinimumIV);
                    //var matchesCP = _whm.Filters.MatchesCpFilter(pkmn.CP, subscribedPokemon.MinimumCP);
                    matchesLvl = Filters.MatchesLvl(pkmn.Level, (uint)subscribedPokemon.MinimumLevel, (uint)subscribedPokemon.MaximumLevel);
                    matchesGender = Filters.MatchesGender(pkmn.Gender, subscribedPokemon.Gender);
                    matchesIVList = subscribedPokemon.IVList?.Contains($"{pkmn.Attack}/{pkmn.Defense}/{pkmn.Stamina}") ?? false;

                    if (!(
                        (/*!subscribedPokemon.HasStats && */matchesIV && matchesLvl && matchesGender) ||
                        (subscribedPokemon.HasStats && matchesIVList)
                        ))
                        continue;

                    var embed = await pkmn.GeneratePokemonMessage(user.GuildId, client, _whConfig, null, loc.Name);
                    foreach (var emb in embed.Embeds)
                    {
                        _queue.Enqueue(new NotificationItem(user, member, emb, pokemon.Name, loc.Name, pkmn));
                    }

                    Statistics.Instance.SubscriptionPokemonSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            member = null;
            user = null;
            loc = null;
            pokemon = null;

            await Task.CompletedTask;
        }

        public async Task ProcessPvPSubscription(PokemonData pkmn)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(pkmn.Id))
                return;

            var loc = _whm.GetGeofence(pkmn.Latitude, pkmn.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                return;
            }

            var subscriptions = Manager.GetUserSubscriptionsByPvPPokemonId(pkmn.Id);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            SubscriptionObject user;
            PvPSubscription subscribedPokemon;
            DiscordMember member = null;
            var pokemon = MasterFile.GetPokemon(pkmn.Id, pkmn.FormId);
            var matchesGreat = false;
            var matchesUltra = false;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    if (!_whConfig.Servers.ContainsKey(user.GuildId))
                        continue;

                    if (!_whConfig.Servers[user.GuildId].EnableSubscriptions)
                        continue;

                    if (!_servers.ContainsKey(user.GuildId))
                        continue;

                    var client = _servers[user.GuildId];

                    try
                    {
                        member = await client.GetMemberById(user.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error(ex);
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.Servers[user.GuildId].DonorRoleIds))
                    {
                        _logger.Debug($"User {member?.Username} ({user.UserId}) is not a supporter, skipping pvp pokemon {pokemon.Name}...");
                        continue;
                    }

                    if (member?.Roles == null || loc == null)
                        continue;

                    // If member does not have role associated with city and server does have city roles configured, skip subscription.
                    if (!member.Roles.Select(x => x?.Name?.ToLower()).Contains(loc?.Name?.ToLower()) && _whConfig.Servers[user.GuildId].CityRoles?.Count > 0)
                    {
                        //_logger.Info($"User {member.Username} does not have city role {loc.Name}, skipping pokemon {pokemon.Name}.");
                        continue;
                    }

                    // Only check distance if user has it set
                    if (user.DistanceM > 0)
                    {
                        var distance = new Coordinates(user.Latitude, user.Longitude).DistanceTo(new Coordinates(pkmn.Latitude, pkmn.Longitude));
                        if (user.DistanceM < distance)
                        {
                            //Skip if distance is set and is not with specified distance.
                            _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for PvP Pokemon {pokemon.Name}, Pokemon is farther than set distance of '{user.DistanceM} meters.");
                            continue;
                        }
                    }

                    var form = Translator.Instance.GetFormName(pkmn.FormId);
                    subscribedPokemon = user.PvP.FirstOrDefault(x =>
                        x.PokemonId == pkmn.Id &&
                        (string.IsNullOrEmpty(x.Form) || string.Compare(x.Form, form, true) == 0) &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    );
                    if (subscribedPokemon == null)
                    {
                        //_logger.Info($"User {member.Username} not subscribed to PvP Pokemon {pokemon.Name} (Form: {form}).");
                        continue;
                    }

                    matchesGreat = pkmn.GreatLeague?.Exists(x => subscribedPokemon.League == PvPLeague.Great &&
                                                                     (x.CP ?? 0) >= 2400 && (x.CP ?? 0) <= 2500 &&
                                                                     (x.Rank ?? 4096) <= subscribedPokemon.MinimumRank &&
                                                                     (x.Percentage ?? 0) >= subscribedPokemon.MinimumPercent) ?? false;
                    matchesUltra = pkmn.GreatLeague?.Exists(x => subscribedPokemon.League == PvPLeague.Ultra &&
                                                                     (x.CP ?? 0) >= 2400 && (x.CP ?? 0) <= 2500 &&
                                                                     (x.Rank ?? 4096) <= subscribedPokemon.MinimumRank &&
                                                                     (x.Percentage ?? 0) >= subscribedPokemon.MinimumPercent) ?? false;

                    if (!matchesGreat && !matchesUltra)
                        continue;

                    var embed = await pkmn.GeneratePokemonMessage(user.GuildId, client, _whConfig, null, loc.Name);
                    foreach (var emb in embed.Embeds)
                    {
                        _queue.Enqueue(new NotificationItem(user, member, emb, pokemon.Name, loc.Name));
                    }

                    Statistics.Instance.SubscriptionPokemonSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            member = null;
            user = null;
            loc = null;
            pokemon = null;

            await Task.CompletedTask;
        }

        public async Task ProcessRaidSubscription(RaidData raid)
        {
            if (!MasterFile.Instance.Pokedex.ContainsKey(raid.PokemonId))
                return;

            var loc = _whm.GetGeofence(raid.Latitude, raid.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city for coordinates {raid.Latitude},{raid.Longitude}, skipping...");
                return;
            }

            var subscriptions = Manager.GetUserSubscriptionsByRaidBossId(raid.PokemonId);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            SubscriptionObject user;
            var pokemon = MasterFile.GetPokemon(raid.PokemonId, raid.Form);
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    if (!_whConfig.Servers.ContainsKey(user.GuildId))
                        continue;

                    if (!_whConfig.Servers[user.GuildId].EnableSubscriptions)
                        continue;

                    if (!_servers.ContainsKey(user.GuildId))
                        continue;

                    var client = _servers[user.GuildId];

                    var member = await client.GetMemberById(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.Servers[user.GuildId].DonorRoleIds))
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping raid boss {pokemon.Name}...");
                        continue;
                    }

                    // Only check distance if user has it set
                    if (user.DistanceM > 0)
                    {
                        var distance = new Coordinates(user.Latitude, user.Longitude).DistanceTo(new Coordinates(raid.Latitude, raid.Longitude));
                        if (user.DistanceM < distance)
                        {
                            //Skip if distance is set and is not met.
                            _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is farther than set distance of '{user.DistanceM} meters.");
                            continue;
                        }
                    }

                    if (user.Gyms.Count > 0 && (!user.Gyms?.Exists(x =>
                        !string.IsNullOrEmpty(x?.Name) &&
                        (
                            (raid.GymName?.ToLower()?.Contains(x.Name?.ToLower()) ?? false) ||
                            (raid.GymName?.ToLower()?.StartsWith(x.Name?.ToLower()) ?? false)
                        )
                    ) ?? false))
                    {
                        //Skip if list is not empty and gym is not in list.
                        _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid '{raid.GymName}' is not in list of subscribed gyms.");
                        continue;
                    }

                    var form = Translator.Instance.GetFormName(raid.Form);
                    var exists = user.Raids.FirstOrDefault(x =>
                        x.PokemonId == raid.PokemonId &&
                        (string.IsNullOrEmpty(x.Form) || string.Compare(x.Form, form, true) == 0) &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    ) != null;
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is in city '{loc.Name}'.");
                        continue;
                    }

                    var embed = raid.GenerateRaidMessage(user.GuildId, client, _whConfig, null, loc.Name);
                    foreach (var emb in embed.Embeds)
                    {
                        _queue.Enqueue(new NotificationItem(user, member, emb, pokemon.Name, loc.Name));
                    }

                    Statistics.Instance.SubscriptionRaidsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;
            loc = null;

            await Task.CompletedTask;
        }

        public async Task ProcessQuestSubscription(QuestData quest)
        {
            var reward = quest.Rewards[0].Info;
            var rewardKeyword = quest.GetReward();
            var questName = quest.GetQuestMessage();

            var loc = _whm.GetGeofence(quest.Latitude, quest.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city for coordinates {quest.Latitude},{quest.Longitude}, skipping...");
                return;
            }

            var subscriptions = Manager.GetUserSubscriptions();
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            bool isSupporter;
            SubscriptionObject user;
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    if (!_whConfig.Servers.ContainsKey(user.GuildId))
                        continue;

                    if (!_whConfig.Servers[user.GuildId].EnableSubscriptions)
                        continue;

                    if (!_servers.ContainsKey(user.GuildId))
                        continue;

                    var client = _servers[user.GuildId];

                    var member = await client.GetMemberById(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    isSupporter = member.HasSupporterRole(_whConfig.Servers[user.GuildId].DonorRoleIds);
                    if (!isSupporter)
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping quest {questName}...");
                        continue;
                    }

                    var exists = user.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()) &&
                    (
                        string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0)
                    )) != null;
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for quest {questName} because the quest is in city '{loc.Name}'.");
                        continue;
                    }

                    // Only check distance if user has it set
                    if (user.DistanceM > 0)
                    {
                        var distance = new Coordinates(user.Latitude, user.Longitude).DistanceTo(new Coordinates(quest.Latitude, quest.Longitude));
                        if (user.DistanceM < distance)
                        {
                            //Skip if distance is set and is not met.
                            _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for Quest {quest.Template}, Quest is farther than set distance of '{user.DistanceM} meters.");
                            continue;
                        }
                    }

                    var embed = quest.GenerateQuestMessage(user.GuildId, client, _whConfig, null, loc.Name);
                    foreach (var emb in embed.Embeds)
                    {
                        _queue.Enqueue(new NotificationItem(user, member, emb, questName, loc.Name));
                    }

                    Statistics.Instance.SubscriptionQuestsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;
            loc = null;

            await Task.CompletedTask;
        }

        public async Task ProcessInvasionSubscription(PokestopData pokestop)
        {
            var loc = _whm.GetGeofence(pokestop.Latitude, pokestop.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city for coordinates {pokestop.Latitude},{pokestop.Longitude}, skipping...");
                return;
            }

            var invasion = MasterFile.Instance.GruntTypes.ContainsKey(pokestop.GruntType) ? MasterFile.Instance.GruntTypes[pokestop.GruntType] : null;
            var encounters = invasion?.GetEncounterRewards();
            if (encounters == null)
                return;

            var subscriptions = Manager.GetUserSubscriptionsByEncounterReward(encounters);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            if (!MasterFile.Instance.GruntTypes.ContainsKey(pokestop.GruntType))
            {
                //_logger.Error($"Failed to parse grunt type {pokestop.GruntType}, not in `grunttype.json` list.");
                return;
            }
         
            SubscriptionObject user;
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    if (!_whConfig.Servers.ContainsKey(user.GuildId))
                        continue;

                    if (!_whConfig.Servers[user.GuildId].EnableSubscriptions)
                        continue;

                    if (!_servers.ContainsKey(user.GuildId))
                        continue;

                    var client = _servers[user.GuildId];

                    var member = await client.GetMemberById(user.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.Servers[user.GuildId].DonorRoleIds))
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping Team Rocket invasion {pokestop.Name}...");
                        continue;
                    }

                    var exists = user.Invasions.FirstOrDefault(x =>
                        encounters.Contains(x.RewardPokemonId) &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    ) != null;
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is in city '{loc.Name}'.");
                        continue;
                    }

                    // Only check distance if user has it set
                    if (user.DistanceM > 0)
                    {
                        var distance = new Coordinates(user.Latitude, user.Longitude).DistanceTo(new Coordinates(pokestop.Latitude, pokestop.Longitude));
                        if (user.DistanceM < distance)
                        {
                            //Skip if distance is set and is not met.
                            _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for TR Invasion {pokestop.Name}, TR Invasion is farther than set distance of '{user.DistanceM} meters.");
                            continue;
                        }
                    }

                    var embed = pokestop.GeneratePokestopMessage(user.GuildId, client, _whConfig, null, loc?.Name);
                    foreach (var emb in embed.Embeds)
                    {
                        _queue.Enqueue(new NotificationItem(user, member, emb, pokestop.Name, loc.Name));
                    }

                    Statistics.Instance.SubscriptionInvasionsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            subscriptions.Clear();
            subscriptions = null;
            user = null;
            loc = null;

            await Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private void ProcessQueue()
        {
            _logger.Trace($"SubscriptionProcessor::ProcessQueue");

            new Thread(async () =>
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    if (_queue.Count > Strings.MaxQueueCountWarning)
                    {
                        _logger.Warn($"Subscription queue is {_queue.Count:N0} items long.");
                    }

                    var item = _queue.Dequeue();
                    if (item == null || item?.Subscription == null || item?.Member == null || item?.Embed == null)
                        continue;

                    // Check if user is receiving messages too fast.
                    if (item.Subscription.Limiter.IsLimited())
                    {
                        _logger.Warn($"{item.Member.Username} notifications rate limited, waiting {(60 - item.Subscription.Limiter.TimeLeft.TotalSeconds)} seconds...", item.Subscription.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        // Send ratelimited notification to user if not already sent to adjust subscription settings to more reasonable settings.
                        if (!item.Subscription.RateLimitNotificationSent)
                        {
                            var guildName = _servers.ContainsKey(item.Subscription.GuildId) ? _servers[item.Subscription.GuildId].Guilds[item.Subscription.GuildId]?.Name : Strings.Creator;
                            var guildIconUrl = _servers.ContainsKey(item.Subscription.GuildId) ? _servers[item.Subscription.GuildId].Guilds[item.Subscription.GuildId]?.IconUrl : string.Empty;
                            var rateLimitMessage = $"Your notification subscriptions have exceeded the {NotificationLimiter.MaxNotificationsPerMinute:N0}) per minute and you are now being rate limited." +
                                                   $"Please adjust your subscriptions to receive a maximum of {NotificationLimiter.MaxNotificationsPerMinute:N0} notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"{guildName} | {DateTime.Now}",
                                    IconUrl = guildIconUrl
                                }
                            };
                            if (!_servers.ContainsKey(item.Subscription.GuildId))
                                continue;

                            await _servers[item.Subscription.GuildId].SendDirectMessage(item.Member, string.Empty, eb.Build());
                            item.Subscription.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    // Ratelimit is up, allow for ratelimiting again
                    item.Subscription.RateLimitNotificationSent = false;

                    if (!_servers.ContainsKey(item.Subscription.GuildId))
                    {
                        _logger.Error($"User subscription for guild that's not configured. UserId={item.Subscription.UserId} GuildId={item.Subscription.GuildId}");
                        continue;
                    }

                    // Send text message notification to user if a phone number is set
                    if (!string.IsNullOrEmpty(item.Subscription.PhoneNumber))
                    {
                        // Check if user is in the allowed text message list or server owner
                        if (_whConfig.Twilio.UserIds.Contains(item.Member.Id) ||
                            _whConfig.Servers[item.Subscription.GuildId].OwnerId == item.Member.Id)
                        {
                            // Send text message (max 160 characters)
                            if (item.Pokemon != null && IsUltraRare(_whConfig.Twilio, item.Pokemon))
                            {
                                var result = Utils.SendSmsMessage(StripEmbed(item), _whConfig.Twilio, item.Subscription.PhoneNumber);
                                if (!result)
                                {
                                    _logger.Error($"Failed to send text message to phone number '{item.Subscription.PhoneNumber}' for user {item.Subscription.UserId}");
                                }
                            }
                        }
                    }

                    // Send direct message notification to user
                    var client = _servers[item.Subscription.GuildId];
                    await client.SendDirectMessage(item.Member, item.Embed);
                    _logger.Info($"[WEBHOOK] Notified user {item.Member.Username} of {item.Description}.");
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }

        private bool IsUltraRare(TwilioConfig twilo, PokemonData pkmn)
        {
            // If no Pokemon are set, do not send text messages
            if (twilo.PokemonIds.Count == 0)
                return false;

            // Check if Pokemon is in list of allowed IDs
            if (!twilo.PokemonIds.Contains(pkmn.Id))
                return false;

            // Send text message if Unown, Azelf, etc
            if (pkmn.Id.IsRarePokemon())
                return true;

            // Send text message if 100% Gible, Deino, and Axew
            if (Filters.MatchesIV(pkmn.IV, twilo.MinimumIV))
                return true;

            return false;
        }

        private static string StripEmbed(NotificationItem item)
        {
            const int MAX_TEXT_LENGTH = 120;
            var text = item.Embed.Description;
            text = text.Replace("**", null);
            text = text.Length > MAX_TEXT_LENGTH
                ? text.Substring(0, Math.Min(text.Length, MAX_TEXT_LENGTH))
                : text;
            // TODO: Construct text message instead of using embed description and url for google maps link
            return $"{item.City}\n{text}\n{item.Embed.Url}";
        }

        #endregion
    }
}