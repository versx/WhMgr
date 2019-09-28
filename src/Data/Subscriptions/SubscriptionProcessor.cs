namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Geofence;
    using WhMgr.Net.Models;
    using WhMgr.Net.Webhooks;

    public class SubscriptionProcessor
    {
        const int MaxQueueCountWarning = 30;

        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly DiscordClient _client;
        private readonly WhConfig _whConfig;
        private readonly WebhookManager _whm;
        private readonly EmbedBuilder _embedBuilder;
        private readonly NotificationQueue _queue;
        //private readonly Queue<PokemonData> _pkmnQueue;
        //private readonly Queue<RaidData> _raidQueue;
        //private readonly Queue<QuestData> _questQueue;

        #endregion

        #region Properties

        public SubscriptionManager Manager { get; }

        #endregion

        #region Constructor

        public SubscriptionProcessor(DiscordClient client, WhConfig config, WebhookManager whm, EmbedBuilder embedBuilder)
        {
            _logger.Trace($"SubscriptionProcessor::SubscriptionProcessor");

            _client = client;
            _whConfig = config;
            _whm = whm;
            _embedBuilder = embedBuilder;
            _queue = new NotificationQueue();

            //_pkmnQueue = new Queue<PokemonData>();
            //_raidQueue = new Queue<RaidData>();
            //_questQueue = new Queue<QuestData>();

            Manager = new SubscriptionManager(_whConfig);

            ProcessQueue();
        }

        #endregion

        #region Public Methods

        //public void EnqueuePokemonSubscription(PokemonData pkmn)
        //{
        //    if (!_pkmnQueue.Contains(pkmn))
        //    {
        //        _pkmnQueue.Enqueue(pkmn);
        //    }
        //}

        //public void EnqueueRaidSubscription(RaidData raid)
        //{
        //    if (!_raidQueue.Contains(raid))
        //    {
        //        _raidQueue.Enqueue(raid);
        //    }
        //}

        //public void EnqueueQuestSubscription(QuestData quest)
        //{
        //    if (!_questQueue.Contains(quest))
        //    {
        //        _questQueue.Enqueue(quest);
        //    }
        //}

        #endregion

        #region Private Methods

        public async Task ProcessPokemonSubscription(PokemonData pkmn)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pkmn.Id))
                return;

            var loc = GetGeofence(pkmn.Latitude, pkmn.Longitude);
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
            var pokemon = db.Pokemon[pkmn.Id];
            bool matchesIV;
            bool matchesLvl;
            bool matchesGender;
            bool matchesAttack;
            bool matchesDefense;
            bool matchesStamina;
            DiscordMember member = null;
            for (var i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    try
                    {
                        member = await _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error(ex);
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.DonorRoleIds))
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

                    var form = pkmn.Id.GetPokemonForm(pkmn.FormId.ToString());
                    subscribedPokemon = user.Pokemon.FirstOrDefault(x => x.PokemonId == pkmn.Id && (string.IsNullOrEmpty(form) || string.Compare(x.Form, form, true) == 0));
                    if (subscribedPokemon == null)
                    {
                        _logger.Info($"User {member.Username} not subscribed to Pokemon {pokemon.Name}.");
                        continue;
                    }

                    matchesIV = _whm.Filters.MatchesIV(pkmn.IV, subscribedPokemon.MinimumIV);
                    //var matchesCP = _whm.Filters.MatchesCpFilter(pkmn.CP, subscribedPokemon.MinimumCP);
                    matchesLvl = _whm.Filters.MatchesLvl(pkmn.Level, subscribedPokemon.MinimumLevel);
                    matchesGender = _whm.Filters.MatchesGender(pkmn.Gender, subscribedPokemon.Gender);
                    matchesAttack = _whm.Filters.MatchesAttack(pkmn.Attack, subscribedPokemon.Attack);
                    matchesDefense = _whm.Filters.MatchesDefense(pkmn.Defense, subscribedPokemon.Defense);
                    matchesStamina = _whm.Filters.MatchesStamina(pkmn.Stamina, subscribedPokemon.Stamina);

                    if (!(
                        (!subscribedPokemon.HasStats && matchesIV && matchesLvl && matchesGender) || 
                        (subscribedPokemon.HasStats && matchesAttack && matchesDefense && matchesStamina)
                         ))
                        continue;

                    /*
                    if (user.Limiter.IsLimited())
                    {
                        _logger.Warn($"{member.Username} notifications rate limited, waiting {(60 - user.Limiter.TimeLeft.TotalSeconds)} seconds...", user.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        if (!user.RateLimitNotificationSent)
                        {
                            var rateLimitMessage = $"Your Pokemon notifications have exceeded {NotificationLimiter.MaxNotificationsPerMinute} per minute and you are now being rate limited. Please adjust your subscriptions to receive a maximum of 30 notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"versx | {DateTime.Now}",
                                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                                }
                            };
                            await _client.SendDirectMessage(member, string.Empty, eb.Build());
                            user.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    user.RateLimitNotificationSent = false;
                    */

                    //_logger.Debug($"Notifying user {member.Username} that a {pokemon.Name} {pkmn.CP}CP {pkmn.IV} IV L{pkmn.Level} has spawned...");

                    var iconStyle = Manager.GetUserIconStyle(user.UserId);
                    var embed = _embedBuilder.BuildPokemonMessage(pkmn, loc.Name, _whConfig.IconStyles[iconStyle]);
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, pokemon.Name, embed));

                    //if (!Manager.AddPokemonStatistic(member.Id, pkmn))
                    //{
                    //    _logger.Warn($"Failed to add {pokemon.Name} Pokemon statistic for user {user.Id}.");
                    //}

                    //await _client.SendDirectMessage(member, embed);
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
            //embed = null;
            user = null;
            loc = null;
            pokemon = null;
            db = null;

            await Task.CompletedTask;
        }

        public async Task ProcessRaidSubscription(RaidData raid)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(raid.PokemonId))
                return;

            var loc = GetGeofence(raid.Latitude, raid.Longitude);
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
            var pokemon = db.Pokemon[raid.PokemonId];
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    var member = await _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.DonorRoleIds))
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping raid boss {pokemon.Name}...");
                        continue;
                    }

                    //if (!member.Roles.Select(x => x.Name.ToLower()).Contains(loc.Name.ToLower()))
                    //{
                    //    _logger.Debug($"[{loc.Name}] Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name} because they do not have the city role '{loc.Name}'.");
                    //    continue;
                    //}

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

                    if (user.Gyms.Count > 0 && !user.Gyms.Exists(x => !string.IsNullOrEmpty(x?.Name) && raid.GymName.ToLower().Contains(x.Name?.ToLower())))
                    {
                        //Skip if list is not empty and gym is not in list.
                        _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid '{raid.GymName}' is not in list of subscribed gyms.");
                        continue;
                    }

                    var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                    var exists = user.Raids.FirstOrDefault(x =>
                        x.PokemonId == raid.PokemonId &&
                        (string.Compare(x.Form, form, true) == 0 || string.IsNullOrEmpty(form)) &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    ) != null;
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is in city '{loc.Name}'.");
                        continue;
                    }

                    /*
                    if (user.Limiter.IsLimited())
                    {
                        _logger.Warn($"{member.Username} notifications rate limited, waiting {(60 - user.Limiter.TimeLeft.TotalSeconds)} seconds...", user.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        if (!user.RateLimitNotificationSent)
                        {
                            var rateLimitMessage = $"Your Pokemon notifications have exceeded {NotificationLimiter.MaxNotificationsPerMinute} per minute and you are now being rate limited. Please adjust your subscriptions to receive a maximum of 30 notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"versx | {DateTime.Now}",
                                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                                }
                            };
                            await _client.SendDirectMessage(member, string.Empty, eb.Build());
                            user.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    user.RateLimitNotificationSent = false;
                    */

                    //_logger.Debug($"Notifying user {member.Username} that a {raid.PokemonId} raid is available...");

                    var iconStyle = Manager.GetUserIconStyle(user.UserId);
                    var embed = _embedBuilder.BuildRaidMessage(raid, loc.Name, _whConfig.IconStyles[iconStyle]);
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, pokemon.Name, embed));

                    //if (!Manager.AddRaidStatistic(member.Id, raid))
                    //{
                    //    _logger.Warn($"Failed to add {pokemon.Name} raid statistic for user {user.Id}.");
                    //}
                    //await _client.SendDirectMessage(member, embed);
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
            //embed = null;
            user = null;
            loc = null;
            db = null;

            await Task.CompletedTask;
        }

        public async Task ProcessQuestSubscription(QuestData quest)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            var reward = quest.Rewards[0].Info;
            var rewardKeyword = quest.GetReward();
            var questName = quest.GetQuestMessage();

            var loc = GetGeofence(quest.Latitude, quest.Longitude);
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
            var embed = _embedBuilder.BuildQuestMessage(quest, loc.Name);
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    var member = await _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    isSupporter = member.HasSupporterRole(_whConfig.DonorRoleIds);
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

                    //continue;
                    //}

                    /*
                    if (user.Limiter.IsLimited())
                    {
                        _logger.Warn($"{member.Username} notifications rate limited, waiting {(60 - user.Limiter.TimeLeft.TotalSeconds)} seconds...", user.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        if (!user.RateLimitNotificationSent)
                        {
                            var rateLimitMessage = $"Your Pokemon notifications have exceeded {NotificationLimiter.MaxNotificationsPerMinute} per minute and you are now being rate limited. Please adjust your subscriptions to receive a maximum of 30 notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"versx | {DateTime.Now}",
                                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                                }
                            };
                            await _client.SendDirectMessage(member, string.Empty, eb.Build());
                            user.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    user.RateLimitNotificationSent = false;
                    */

                    //_logger.Debug($"Notifying user {member.Username} that a {rewardKeyword} quest is available...");
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, questName, embed));

                    //if (!Manager.AddQuestStatistic(member.Id, quest))
                    //{
                    //    _logger.Warn($"Failed to add {quest.GetRewardString()} quest statistic for user {user.Id}.");
                    //}
                    Statistics.Instance.SubscriptionQuestsSent++;
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public async Task ProcessInvasionSubscription(PokestopData pokestop)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var loc = GetGeofence(pokestop.Latitude, pokestop.Longitude);
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city for coordinates {pokestop.Latitude},{pokestop.Longitude}, skipping...");
                return;
            }

            var subscriptions = Manager.GetUserSubscriptionsByGruntType(pokestop.GruntType);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            SubscriptionObject user;
            var embed = _embedBuilder.BuildPokestopMessage(pokestop, loc.Name);
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    var member = await _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    if (!member.HasSupporterRole(_whConfig.DonorRoleIds))
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping Team Rocket invasion {pokestop.Name}...");
                        continue;
                    }

                    var exists = user.Invasions.FirstOrDefault(x =>
                        x.GruntType == pokestop.GruntType &&
                        (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    ) != null;
                    if (!exists)
                    {
                        //_logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name}, raid is in city '{loc.Name}'.");
                        continue;
                    }

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

                    /*
                    if (user.Limiter.IsLimited())
                    {
                        _logger.Warn($"{member.Username} notifications rate limited, waiting {(60 - user.Limiter.TimeLeft.TotalSeconds)} seconds...", user.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        if (!user.RateLimitNotificationSent)
                        {
                            var rateLimitMessage = $"Your Pokemon notifications have exceeded {NotificationLimiter.MaxNotificationsPerMinute} per minute and you are now being rate limited. Please adjust your subscriptions to receive a maximum of 30 notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"versx | {DateTime.Now}",
                                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                                }
                            };
                            await _client.SendDirectMessage(member, string.Empty, eb.Build());
                            user.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    user.RateLimitNotificationSent = false;
                    */

                    //_logger.Debug($"Notifying user {member.Username} that a {raid.PokemonId} raid is available...");

                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, pokestop.Name, embed));

                    //if (!Manager.AddRaidStatistic(member.Id, raid))
                    //{
                    //    _logger.Warn($"Failed to add {pokemon.Name} raid statistic for user {user.Id}.");
                    //}
                    //await _client.SendDirectMessage(member, embed);
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
            embed = null;
            user = null;
            loc = null;

            await Task.CompletedTask;
        }

        private void ProcessQueue()
        {
            _logger.Trace($"SubscriptionProcessor::ProcessQueue");

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            new Thread(async () =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    if (_queue.Count > MaxQueueCountWarning)
                    {
                        _logger.Warn($"Subscription queue is {_queue.Count.ToString("N0")} items long.");
                    }

                    var item = _queue.Dequeue();
                    if (item == null || item?.Item1 == null || item?.Item3 == null)
                        continue;

                    var member = item.Item1;
                    var user = Manager.Subscriptions.FirstOrDefault(x => x.UserId == item.Item1.Id);
                    if (user == null)
                        continue;

                    if (user.Limiter.IsLimited())
                    {
                        _logger.Warn($"{member.Username} notifications rate limited, waiting {(60 - user.Limiter.TimeLeft.TotalSeconds)} seconds...", user.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        if (!user.RateLimitNotificationSent)
                        {
                            var rateLimitMessage = $"Your Pokemon notifications have exceeded {NotificationLimiter.MaxNotificationsPerMinute} per minute and you are now being rate limited. Please adjust your subscriptions to receive a maximum of 30 notifications within a 60 second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"versx | {DateTime.Now}",
                                    IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
                                }
                            };
                            await _client.SendDirectMessage(member, string.Empty, eb.Build());
                            user.RateLimitNotificationSent = true;
                        }
                        continue;
                    }

                    user.RateLimitNotificationSent = false;

                    await _client.SendDirectMessage(item.Item1, item.Item3);
                    _logger.Info($"[WEBHOOK] Notified user {item.Item1.Username} of {item.Item2}.");
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }

        //        private void ProcessQueue()
        //        {
        //#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        //            new Thread(async () =>
        //#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        //            {
        //                while (true)
        //                {
        //                    if (_pkmnQueue.Count > 0)
        //                    {
        //                        if (_pkmnQueue.Count > MaxQueueCountWarning)
        //                        {
        //                            _logger.Warn($"Pokemon subscription queue is {_pkmnQueue.Count.ToString("N0")} items long.");
        //                        }

        //                        var pkmn = _pkmnQueue.Dequeue();
        //                        await ProcessPokemonSubscription(pkmn);
        //                        Thread.Sleep(5);
        //                    }

        //                    if (_raidQueue.Count > 0)
        //                    {
        //                        if (_raidQueue.Count > MaxQueueCountWarning)
        //                        {
        //                            _logger.Warn($"Raid subscription queue is {_pkmnQueue.Count.ToString("N0")} items long.");
        //                        }

        //                        var raid = _raidQueue.Dequeue();
        //                        await ProcessRaidSubscription(raid);
        //                        Thread.Sleep(5);
        //                    }

        //                    if (_questQueue.Count > 0)
        //                    {
        //                        if (_questQueue.Count > MaxQueueCountWarning)
        //                        {
        //                            _logger.Warn($"Quest subscription queue is {_pkmnQueue.Count.ToString("N0")} items long.");
        //                        }

        //                        var quest = _questQueue.Dequeue();
        //                        await ProcessQuestSubscription(quest);
        //                        Thread.Sleep(5);
        //                    }

        //                    Thread.Sleep(10);
        //                }
        //            })
        //            { IsBackground = true }.Start();
        //        }

        private GeofenceItem GetGeofence(double latitude, double longitude)
        {
            var loc = _whm.GeofenceService.GetGeofence(_whm.Geofences.Select(x => x.Value).ToList(), new Location(latitude, longitude));
            return loc;
        }

        #endregion
    }
}