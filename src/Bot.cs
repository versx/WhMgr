namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using WhMgr.Commands;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Geofence;
    using WhMgr.Localization;
    using WhMgr.Net.Models;
    using WhMgr.Net.Webhooks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.CommandsNext;

    using ServiceStack.OrmLite;

    public class Bot
    {
        #region Variables

        private readonly DiscordClient _client;
        private readonly CommandsNextModule _commands;
        private readonly Dependencies _dep;
        private readonly WebhookManager _whm;
        private readonly WhConfig _whConfig;
        private readonly SubscriptionManager _subMgr;
        private readonly Translator _lang;
        private readonly NotificationQueue _queue;
        private readonly IEventLogger _logger;

        #endregion

        #region Constructor

        public Bot(WhConfig whConfig)
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            _logger = EventLogger.GetLogger(name);
            _logger.Trace($"Bot::Bot [WhConfig={whConfig.GuildId}, OwnerId={whConfig.OwnerId}, SupporterRoleId={whConfig.SupporterRoleId}, WebhookPort={whConfig.WebHookPort}]");

            _lang = new Translator();
            _queue = new NotificationQueue();

            _whConfig = whConfig;
            DataAccessLayer.ConnectionString = _whConfig.ConnectionString;

            _whm = new WebhookManager(_whConfig.WebHookPort);
            _whm.PokemonAlarmTriggered += OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered += OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered += OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered += OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered += OnGymAlarmTriggered;
            if (_whConfig.EnableSubscriptions)
            {
                _whm.PokemonSubscriptionTriggered += OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered += OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered += OnQuestSubscriptionTriggered;
            }

            _logger.Info("WebhookManager is running...");

            var midnight = new DandTSoftware.Timers.MidnightTimer();
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            midnight.TimeReached += async (e) => await ResetQuests();
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            midnight.Start();

            _client = new DiscordClient(new DiscordConfiguration
            {
                AutomaticGuildSync = true,
                AutoReconnect = true,
                EnableCompression = true,
                Token = _whConfig.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });
            _client.Ready += Client_Ready;
            _client.ClientErrored += Client_ClientErrored;
            _client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;

            if (_whConfig.EnableSubscriptions)
            {
                _subMgr = new SubscriptionManager();
            }

            DependencyCollection dep;
            using (var d = new DependencyCollectionBuilder())
            {
                d.AddInstance(_dep = new Dependencies(_subMgr, _whConfig, _lang));
                dep = d.Build();
            }

            _commands = _client.UseCommandsNext
            (
                new CommandsNextConfiguration
                {
                    StringPrefix = _whConfig.CommandPrefix?.ToString(),
                    EnableDms = true,
                    EnableMentionPrefix = string.IsNullOrEmpty(_whConfig.CommandPrefix),
                    EnableDefaultHelp = false,
                    CaseSensitive = false,
                    IgnoreExtraArguments = true,
                    Dependencies = dep
                }
            );
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
            _commands.RegisterCommands<Notifications>();
            _commands.RegisterCommands<Quests>();

            ProcessQueue();
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _logger.Trace("Bot::Start");
            _logger.Info("Connecting to Discord...");

            _client.ConnectAsync();
        }

        #endregion

        #region Discord Events

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info($"Connected.");

            await CreateEmojis();
        }

        private async Task Client_ClientErrored(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception);

            await Task.CompletedTask;
        }

        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, Strings.BotName, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            await Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, Strings.BotName, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack of required permissions
            if (e.Exception is DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)
            {
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync("", embed: embed);
            }
            else if (e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException)
            {
                _logger.Warn($"User {e.Context.User.Username} tried executing command {e.Context.Message.Content} but command does not exist.");
            }
            else
            {
                _logger.Error($"User {e.Context.User.Username} tried executing command {e.Command?.Name} and unknown error occurred.\r\n: {e.Exception.ToString()}");
            }
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Level)
            {
                case LogLevel.Error: color = ConsoleColor.DarkRed; break;
                case LogLevel.Warning: color = ConsoleColor.Yellow; break;
                case LogLevel.Info: color = ConsoleColor.White; break;
                case LogLevel.Critical: color = ConsoleColor.Red; break;
                case LogLevel.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Source
            var sourceName = e.Application;

            //Text
            var text = e.Message;

            //Build message
            var builder = new System.Text.StringBuilder(text.Length + (sourceName?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }

            for (var i = 0; i < text.Length; i++)
            {
                //Strip control chars
                var c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }

            if (text != null)
            {
                builder.Append(": ");
                builder.Append(text);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        #endregion

        #region WebHookManager Events

        private async void OnPokemonAlarmTriggered(object sender, PokemonAlarmTriggeredEventArgs e)
        {
            _logger.Info($"Pokemon Found [Alarm: {e.Alarm.Name}, Pokemon: {e.Pokemon.Id}, Despawn: {e.Pokemon.DespawnTime}");

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var pkmn = Database.Instance.Pokemon[e.Pokemon.Id];
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(e.Pokemon.Latitude, e.Pokemon.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city from coordinates {e.Pokemon.Latitude},{e.Pokemon.Longitude} {pkmn.Name} {e.Pokemon.IV}, skipping...");
                return;
            }

            var form = e.Pokemon.Id.GetPokemonForm(e.Pokemon.FormId);
            var pkmnImage = string.Format(Strings.PokemonImage, e.Pokemon.Id, Convert.ToInt32(string.IsNullOrEmpty(e.Pokemon.FormId) ? "0" : e.Pokemon.FormId));
            var eb = BuildPokemonMessage(e.Pokemon, loc.Name);

            var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
            var name = $"{pkmn.Name}{e.Pokemon.Gender.GetPokemonGenderIcon()}{form}";
            await whData.ExecuteAsync(string.Empty, name, pkmnImage, false, new List<DiscordEmbed> { eb });
        }

        private async void OnRaidAlarmTriggered(object sender, RaidAlarmTriggeredEventArgs e)
        {
            _logger.Info($"Raid Found [Alarm: {e.Alarm.Name}, Raid: {e.Raid.PokemonId}, Level: {e.Raid.Level}, StartTime: {e.Raid.StartTime}]");

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var pkmn = Database.Instance.Pokemon[e.Raid.PokemonId];
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(e.Raid.Latitude, e.Raid.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city from coordinates {e.Raid.Latitude},{e.Raid.Longitude} {pkmn.Name} {e.Raid.Level}, skipping...");
                return;
            }

            var pkmnImage = e.Raid.IsEgg ? string.Format(Strings.EggImage, e.Raid.Level) : string.Format(Strings.PokemonImage, e.Raid.PokemonId, 0);
            var eb = BuildRaidMessage(e.Raid, loc.Name);

            var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
            var name = e.Raid.IsEgg ? $"Level {e.Raid.Level} {pkmn.Name}" : $"{pkmn.Name} Raid";
            await whData.ExecuteAsync(string.Empty, name, pkmnImage, false, new List<DiscordEmbed> { eb });
        }

        private async void OnQuestAlarmTriggered(object sender, QuestAlarmTriggeredEventArgs e)
        {
            _logger.Info($"Quest Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Quest.PokestopId}, Type={e.Quest.Type}]");

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            try
            {
                var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(e.Quest.Latitude, e.Quest.Longitude));
                if (loc == null)
                {
                    _logger.Warn($"Failed to lookup city for coordinates {e.Quest.Latitude},{e.Quest.Longitude}, skipping...");
                    return;
                }

                var eb = BuildQuestMessage(e.Quest, loc?.Name ?? e.Alarm.Name);
                var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
                await whData.ExecuteAsync(string.Empty, e.Quest.GetMessage(), e.Quest.GetIconUrl(), false, new List<DiscordEmbed> { eb });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPokestopAlarmTriggered(object sender, PokestopAlarmTriggeredEventArgs e)
        {
        }

        private void OnGymAlarmTriggered(object sender, GymAlarmTriggeredEventArgs e)
        {
        }

        #endregion

        #region Subscription Events

        private void OnPokemonSubscriptionTriggered(object sender, PokemonData e)
        {
            new System.Threading.Thread(() => ProcessPokemonSubscription(e)) { IsBackground = true }.Start();
        }

        private void OnRaidSubscriptionTriggered(object sender, RaidData e)
        {
            new System.Threading.Thread(() => ProcessRaidSubscription(e)) { IsBackground = true }.Start();
        }

        private void OnQuestSubscriptionTriggered(object sender, QuestData e)
        {
            new System.Threading.Thread(() => ProcessQuestSubscription(e)) { IsBackground = true }.Start();
        }

        #endregion

        #region Subscription Processor

        private void ProcessPokemonSubscription(PokemonData pkmn)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pkmn.Id))
                return;

            var loc = GetGeofence(pkmn.Latitude, pkmn.Longitude);
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city from coordinates {pkmn.Latitude},{pkmn.Longitude} {db.Pokemon[pkmn.Id].Name} {pkmn.IV}, skipping...");
                return;
            }

            var subscriptions = _subMgr.GetUserSubscriptionsByPokemonId(pkmn.Id);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            SubscriptionObject user;
            bool isSupporter;
            PokemonSubscription subscribedPokemon;
            var pokemon = db.Pokemon[pkmn.Id];
            bool matchesIV;
            bool matchesLvl;
            bool matchesGender;
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
                        member = _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"FAILED TO GET MEMBER BY ID {user.UserId}");
                        _logger.Error(ex);
                    }

                    if (member == null)
                    {
                        _logger.Warn($"Failed to find Discord member with id {user.UserId}.");
                        continue;
                    }

                    isSupporter = member.HasSupporterRole(_whConfig.SupporterRoleId);
                    if (!isSupporter)
                    {
                        _logger.Debug($"User {member.Username} is not a supporter, skipping pokemon {pokemon.Name}...");
                        continue;
                    }

                    subscribedPokemon = user.Pokemon.FirstOrDefault(x => x.PokemonId == pkmn.Id);
                    if (subscribedPokemon == null)
                    {
                        _logger.Info($"User {member.Username} not subscribed to Pokemon {pokemon.Name}.");
                        continue;
                    }

                    if (!member.Roles.Select(x => x.Name.ToLower()).Contains(loc.Name.ToLower()))
                    {
                        _logger.Info($"User {member.Username} does not have city role {loc.Name}, skipping pokemon {pokemon.Name}.");
                        continue;
                    }

                    matchesIV = _whm.Filters.MatchesIV(pkmn.IV, subscribedPokemon.MinimumIV);
                    //var matchesCP = _whm.Filters.MatchesCpFilter(pkmn.CP, subscribedPokemon.MinimumCP);
                    matchesLvl = _whm.Filters.MatchesLvl(pkmn.Level, subscribedPokemon.MinimumLevel);
                    matchesGender = _whm.Filters.MatchesGender(pkmn.Gender, subscribedPokemon.Gender);

                    if (!(matchesIV && matchesLvl && matchesGender))
                        continue;

                    //if (user.Limiter.IsLimited())
                    //{
                    //    //if (!user.NotifiedOfLimited)
                    //    //{
                    //    //    await _client.SendDirectMessage(member, string.Format(NotificationsLimitedMessage, NotificationLimiter.MaxNotificationsPerMinute), null);
                    //    //    user.NotifiedOfLimited = true;
                    //    //}
                    //    _logger.Debug($"Discord user {member.Username}'s ({member.Id}) notifications are being limited...");
                    //    continue;
                    //}

                    //user.NotifiedOfLimited = false;

                    _logger.Debug($"Notifying user {member.Username} that a {pokemon.Name} {pkmn.CP}CP {pkmn.IV} IV L{pkmn.Level} has spawned...");

                    //if (await CheckIfExceededNotificationLimit(user)) return;

                    user.NotificationsToday++;

                    var embed = BuildPokemonMessage(pkmn, loc.Name);
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, pokemon.Name, embed));
                    System.Threading.Thread.Sleep(5);
                    //await SendNotification(user.UserId, pokemon.Name, embed);
                    //await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void ProcessRaidSubscription(RaidData raid)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(raid.PokemonId))
                return;

            var loc = GetGeofence(raid.Latitude, raid.Longitude);
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city for coordinates {raid.Latitude},{raid.Longitude}, skipping...");
                return;
            }

            var subscriptions = _subMgr.GetUserSubscriptionsByRaidBossId(raid.PokemonId);
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            bool isSupporter;
            SubscriptionObject user;
            RaidSubscription subscribedRaid;
            var pokemon = db.Pokemon[raid.PokemonId];
            var embed = BuildRaidMessage(raid, loc.Name);
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    var member = _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    isSupporter = member.HasSupporterRole(_whConfig.SupporterRoleId);
                    if (!isSupporter)
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping raid boss {pokemon.Name}...");
                        continue;
                    }

                    subscribedRaid = user.Raids.FirstOrDefault(x => x.PokemonId == raid.PokemonId);
                    if (subscribedRaid == null)
                        continue;

                    if (!member.Roles.Select(x => x.Name.ToLower()).Contains(loc.Name.ToLower()))
                    {
                        _logger.Debug($"[{loc.Name}] Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name} because they do not have the city role '{loc.Name}'.");
                        continue;
                    }

                    var exists = user.Raids.FirstOrDefault(x => x.PokemonId == raid.PokemonId
                        && (string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0))
                    ) != null;
                    if (!exists)
                    {
                        _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for raid boss {pokemon.Name} because the raid is in city '{loc.Name}'.");
                        continue;
                    }

                    //if (user.Limiter.IsLimited())
                    //{
                    //    //if (!user.NotifiedOfLimited)
                    //    //{
                    //    //    await _client.SendDirectMessage(member, string.Format(NotificationsLimitedMessage, NotificationLimiter.MaxNotificationsPerMinute), null);
                    //    //    user.NotifiedOfLimited = true;
                    //    //}

                    //    continue;
                    //}

                    //user.NotifiedOfLimited = false;

                    _logger.Debug($"Notifying user {member.Username} that a {raid.PokemonId} raid is available...");

                    //if (await CheckIfExceededNotificationLimit(user)) return;

                    user.NotificationsToday++;

                    //await SendNotification(user.UserId, pokemon.Name, embed);
                    //await Task.Delay(10);
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, pokemon.Name, embed));
                    System.Threading.Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void ProcessQuestSubscription(QuestData quest)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            var db = Database.Instance;
            var reward = quest.Rewards[0].Info;
            var rewardKeyword = quest.GetRewardString();
            var questName = quest.GetMessage();

            var loc = GetGeofence(quest.Latitude, quest.Longitude);
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city for coordinates {quest.Latitude},{quest.Longitude}, skipping...");
                return;
            }

            var subscriptions = _subMgr.GetUserSubscriptions();
            if (subscriptions == null)
            {
                _logger.Warn($"Failed to get subscriptions from database table.");
                return;
            }

            bool isSupporter;
            SubscriptionObject user;
            QuestSubscription subscribedQuest;
            var embed = BuildQuestMessage(quest, loc.Name);
            for (int i = 0; i < subscriptions.Count; i++)
            {
                try
                {
                    user = subscriptions[i];
                    if (user == null)
                        continue;

                    if (!user.Enabled)
                        continue;

                    var member = _client.GetMemberById(_whConfig.GuildId, user.UserId);
                    if (member == null)
                    {
                        _logger.Warn($"Failed to find member with id {user.UserId}.");
                        continue;
                    }

                    isSupporter = member.HasSupporterRole(_whConfig.SupporterRoleId);
                    if (!isSupporter)
                    {
                        _logger.Info($"User {user.UserId} is not a supporter, skipping quest {questName}...");
                        continue;
                    }

                    subscribedQuest = user.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()));
                    if (subscribedQuest == null)
                        continue;

                    if (!member.Roles.Select(x => x.Name).Contains(loc.Name))
                    {
                        _logger.Debug($"[{loc.Name}] Skipping notification for user {member.DisplayName} ({member.Id}) for quest {questName} because they do not have the city role '{loc.Name}'.");
                        continue;
                    }

                    var exists = user.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()) &&
                    (
                        string.IsNullOrEmpty(x.City) || (!string.IsNullOrEmpty(x.City) && string.Compare(loc.Name, x.City, true) == 0)
                    )) != null;
                    if (!exists)
                    {
                        _logger.Debug($"Skipping notification for user {member.DisplayName} ({member.Id}) for quest {questName} because the quest is in city '{loc.Name}'.");
                        continue;
                    }

                    //if (user.Limiter.IsLimited())
                    //{
                    //    //if (!user.NotifiedOfLimited)
                    //    //{
                    //    //    await _client.SendDirectMessage(member, string.Format(NotificationsLimitedMessage, NotificationLimiter.MaxNotificationsPerMinute), null);
                    //    //    user.NotifiedOfLimited = true;
                    //    //}

                    //    continue;
                    //}

                    //user.NotifiedOfLimited = false;

                    _logger.Debug($"Notifying user {member.Username} that a {rewardKeyword} quest is available...");

                    //if (await CheckIfExceededNotificationLimit(user)) return;

                    user.NotificationsToday++;

                    //await SendNotification(user.UserId, questName, embed);
                    //await Task.Delay(10);
                    _queue.Enqueue(new Tuple<DiscordUser, string, DiscordEmbed>(member, questName, embed));
                    System.Threading.Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void ProcessQueue()
        {
            _logger.Trace($"Bot::ProcessQueue");

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            new System.Threading.Thread(async () =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        System.Threading.Thread.Sleep(10);
                        continue;
                    }

                    var item = _queue.Dequeue();
                    await _client.SendDirectMessage(item.Item1, item.Item3);

                    _logger.Debug($"[WEBHOOK] Notified user {item.Item1.Username} of {item.Item2}.");
                    //await Task.Delay(10);

                    System.Threading.Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }

        #endregion

        #region Embed Builder

        private DiscordEmbed BuildPokemonMessage(PokemonData pokemon, string city)
        {
            _logger.Trace($"Bot::BuildPokemonMessage [Pokemon={pokemon.Id}, City={city}]");

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pokemon.Id))
                return null;

            var pkmn = db.Pokemon[pokemon.Id];
            if (pkmn == null)
            {
                _logger.Error($"Failed to lookup Pokemon '{pokemon.Id}' in database.");
                return null;
            }

            var form = pokemon.Id.GetPokemonForm(pokemon.FormId);
            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") /*"DIRECTIONS"*/ : city,
                Url = string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, pokemon.Latitude, pokemon.Longitude) + $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = string.Format(Strings.PokemonImage, pokemon.Id, Convert.ToInt32(string.IsNullOrEmpty(pokemon.FormId) ? "0" : pokemon.FormId)),
                Color = pokemon.IV.BuildColor()
            };

            //if (pokemon.IsMissingStats)
            //{
            //    eb.Description = string.Format(_lang.Translate("EMBED_POKEMON_TITLE_WITHOUT_DETAILS"), pkmn.Name, form, pokemon.Gender.GetPokemonGenderIcon(), pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableStringNoSeconds()); //$"{pkmn.Name} {form}{pokemon.Gender.GetPokemonGenderIcon()} Despawn: {pokemon.DespawnTime.ToLongTimeString()} ({pokemon.SecondsLeft.ToReadableStringNoSeconds()} left)\r\n\r\n";
            //}
            //else
            //{
            eb.Description = _lang.Translate("EMBED_POKEMON_TITLE").FormatText(pkmn.Name, form, pokemon.Gender.GetPokemonGenderIcon(), pokemon.IV, pokemon.Level, pokemon.DespawnTime.ToLongTimeString(), pokemon.SecondsLeft.ToReadableStringNoSeconds()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_POKEMON_DETAILS").FormatText(pokemon.CP, pokemon.IV, pokemon.Level) + "\r\n";
            eb.Description += _lang.Translate("EMBED_POKEMON_STATS").FormatText(pokemon.Attack, pokemon.Defense, pokemon.Stamina) + "\r\n";
            //eb.Description = $"{pkmn.Name} {form}{pokemon.Gender.GetPokemonGenderIcon()} {pokemon.IV} L{pokemon.Level} Despawn: {pokemon.DespawnTime.ToLongTimeString()} ({pokemon.SecondsLeft.ToReadableStringNoSeconds()} left)\r\n\r\n";
            //eb.Description += $"**Details:** CP: {pokemon.CP} IV: {pokemon.IV} LV: {pokemon.Level}\r\n";
            //eb.Description += $"**IV Stats:** Atk: {pokemon.Attack}/Def: {pokemon.Defense}/Sta: {pokemon.Stamina}\r\n";
            //}

            if (!string.IsNullOrEmpty(form))
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_FORM").FormatText(form) + "\r\n";
                //eb.Description += $"**Form:** {form}\r\n";
            }

            if (int.TryParse(pokemon.Level, out var lvl) && lvl >= 30)
            {
                eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER_BOOSTED") + "\r\n";
                //eb.Description += $":white_sun_rain_cloud: Boosted\r\n";
            }

            //var maxCp = db.MaxCpAtLevel(pokemon.Id, 40);
            //var maxWildCp = db.MaxCpAtLevel(pokemon.Id, 35);
            //eb.Description += $"**Max Wild CP:** {maxWildCp}, **Max CP:** {maxCp} \r\n";

            eb.Description += _lang.Translate("EMBED_POKEMON_WEATHER").FormatText(Strings.WeatherEmojis[pokemon.Weather]) + "\r\n";
            //eb.Description += $"**Weather:** {Strings.WeatherEmojis[pokemon.Weather]}\r\n";

            if (pkmn.Types != null)
            {
                eb.Description += _lang.Translate("EMBED_TYPES").FormatText(GetTypeEmojiIcons(pkmn.Types)) + "\r\n";
                //eb.Description += $"**Types:** {GetTypeEmojiIcons(pkmn.Types)}\r\n";
            }

            if (float.TryParse(pokemon.Height, out var height) && float.TryParse(pokemon.Weight, out var weight))
            {
                var size = pokemon.Id.GetSize(height, weight);
                eb.Description += _lang.Translate("EMBED_POKEMON_SIZE").FormatText(size) + "\r\n";
                //eb.Description += $"**Size:** {size}\r\n";
            }

            var fastMoveId = Convert.ToInt32(pokemon.FastMove ?? "0");
            if (db.Movesets.ContainsKey(fastMoveId))
            {
                var fastMove = db.Movesets[fastMoveId];
                eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "\r\n";
                //eb.Description += $"**Fast Move:** {fastMove.Name}\r\n";

            }

            var chargeMoveId = Convert.ToInt32(pokemon.ChargeMove ?? "0");
            if (db.Movesets.ContainsKey(chargeMoveId))
            {
                var chargeMove = db.Movesets[chargeMoveId];
                eb.Description += _lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
                //eb.Description += $"**Charge Move:** {chargeMove.Name}\r\n";
            }

            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(pokemon.Latitude, 5), Math.Round(pokemon.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Location:** {Math.Round(pokemon.Latitude, 5)},{Math.Round(pokemon.Longitude, 5)}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(pokemon.Latitude, pokemon.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude)) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({string.Format(Strings.GoogleMaps, pokemon.Latitude, pokemon.Longitude)})**";
            eb.ImageUrl = string.Format(Strings.GoogleMapsStaticImage, pokemon.Latitude, pokemon.Longitude) + $"&key={_whConfig.GmapsKey}";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };
            var embed = eb.Build();

            return embed;
        }

        private DiscordEmbed BuildRaidMessage(RaidData raid, string city)
        {
            _logger.Trace($"Bot::BuildRaidMessage [Raid={raid.PokemonId}, City={city}]");

            var db = Database.Instance;
            var pkmn = db.Pokemon[raid.PokemonId];
            if (pkmn == null)
            {
                _logger.Error($"Failed to lookup Raid Pokemon '{raid.PokemonId}' in database.");
                return null;
            }

            var pkmnImage = raid.IsEgg ? string.Format(Strings.EggImage, raid.Level) : string.Format(Strings.PokemonImage, raid.PokemonId, 0);
            var eb = new DiscordEmbedBuilder
            {
                Title = string.IsNullOrEmpty(city) ? _lang.Translate("EMBED_DIRECTIONS") /*"DIRECTIONS"*/ : $"{city}: {raid.GymName}",
                Url = string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude),
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, raid.Latitude, raid.Longitude) + $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = pkmnImage,
                Color = Convert.ToInt32(raid.Level).BuildRaidColor()
            };

            if (raid.IsEgg)
            {
                eb.Description = _lang.Translate("EMBED_EGG_HATCHES").FormatText(raid.StartTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_EGG_ENDS").FormatText(raid.EndTime.ToLongTimeString(), DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()) + "\r\n";
                //eb.Description += $"Hatches: {raid.StartTime.ToLongTimeString()} ({DateTime.Now.GetTimeRemaining(raid.StartTime).ToReadableStringNoSeconds()} left)\r\n";
                //eb.Description += $"**Ends:** {raid.EndTime.ToLongTimeString()} ({DateTime.Now.GetTimeRemaining(raid.EndTime).ToReadableStringNoSeconds()} left)\r\n";
            }
            else
            {
                eb.Description = _lang.Translate("EMBED_RAID_ENDS").FormatText(pkmn.Name, raid.EndTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_STARTED").FormatText(raid.StartTime.ToLongTimeString()) + "\r\n";
                eb.Description += _lang.Translate("EMBED_RAID_ENDS_WITH_TIME_LEFT").FormatText(raid.EndTime.ToLongTimeString(), raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()) + "\r\n";
                //eb.Description += $"{pkmn.Name} Raid Ends: {raid.EndTime.ToLongTimeString()}\r\n\r\n";
                //eb.Description += $"**Started:** {raid.StartTime.ToLongTimeString()}\r\n";
                //eb.Description += $"**Ends:** {raid.EndTime.ToLongTimeString()} ({raid.EndTime.GetTimeRemaining().ToReadableStringNoSeconds()} left)\r\n";

                var perfectRange = raid.PokemonId.MaxCpAtLevel(20);
                var boostedRange = raid.PokemonId.MaxCpAtLevel(25);
                eb.Description += _lang.Translate("EMBED_RAID_PERFECT_CP").FormatText(perfectRange, boostedRange) + "\r\n";
                //eb.Description += $"**Perfect CP:** {perfectRange} / :white_sun_rain_cloud: {boostedRange}\r\n";

                if (pkmn.Types != null)
                {
                    eb.Description += _lang.Translate("EMBED_TYPES").FormatText(GetTypeEmojiIcons(pkmn.Types)) + "\r\n";
                    //eb.Description += $"**Types:** {GetTypeEmojiIcons(pkmn.Types)}\r\n";
                }

                var fastMoveId = Convert.ToInt32(raid.FastMove ?? "0");
                if (db.Movesets.ContainsKey(fastMoveId))
                {
                    var fastMove = db.Movesets[fastMoveId];
                    eb.Description += _lang.Translate("EMBED_MOVE_FAST").FormatText(fastMove.Name) + "\r\n";
                    //eb.Description += $"**Fast Move:** {fastMove.Name}\r\n";
                }

                var chargeMoveId = Convert.ToInt32(raid.ChargeMove ?? "0");
                if (db.Movesets.ContainsKey(chargeMoveId))
                {
                    var chargeMove = db.Movesets[chargeMoveId];
                    eb.Description += _lang.Translate("EMBED_MOVE_CHARGE").FormatText(chargeMove.Name) + "\r\n";
                    //eb.Description += $"**Charge Move:** {chargeMove.Name}\r\n";
                }

                var weaknessesEmojis = GetWeaknessEmojiIcons(pkmn.Types);
                if (!string.IsNullOrEmpty(weaknessesEmojis))
                {
                    eb.Description += _lang.Translate("EMBED_RAID_WEAKNESSES").FormatText(weaknessesEmojis) + "\r\n";
                    //eb.Description += $"**Weaknesses:** {weaknessesEmojis}\r\n";
                }
            }

            if (raid.IsExclusive || raid.SponsorId)
            {
                var exEmojiId = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId].GetEmojiId(_lang.Translate("EMOJI_EX")) : 0;
                var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
                eb.Description += _lang.Translate("EMBED_RAID_EX_GYM").FormatText(exEmoji) + "\r\n";
                //eb.Description += $"{exEmoji} **Gym!**\r\n";
            }
            var teamEmojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId(raid.Team.ToString().ToLower());
            var teamEmoji = teamEmojiId > 0 ? $"<:{raid.Team.ToString().ToLower()}:{teamEmojiId}>" : raid.Team.ToString();
            eb.Description += _lang.Translate("EMBED_TEAM").FormatText(teamEmoji) + "\r\n";
            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(raid.Latitude, 5), Math.Round(raid.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Team:** {teamEmoji}\r\n";
            //eb.Description += $"**Location:** {Math.Round(raid.Latitude, 5)},{Math.Round(raid.Longitude, 5)}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(raid.Latitude, raid.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({string.Format(Strings.GoogleMaps, raid.Latitude, raid.Longitude)})**";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };
            var embed = eb.Build();

            return embed;
        }

        private DiscordEmbed BuildQuestMessage(QuestData quest, string city)
        {
            _logger.Trace($"Bot::BuildQuestMessage [Quest={quest.PokestopId}, City={city}]");

            var gmapsUrl = string.Format(Strings.GoogleMaps, quest.Latitude, quest.Longitude);
            var eb = new DiscordEmbedBuilder
            {
                Title = $"{city.Replace("Quests", null).Replace("Spinda", null).Replace("Nincada", null)}: {(string.IsNullOrEmpty(quest.PokestopName) ? _lang.Translate("UNKNOWN_POKESTOP") : quest.PokestopName)}",
                Url = gmapsUrl,
                ImageUrl = string.Format(Strings.GoogleMapsStaticImage, quest.Latitude, quest.Longitude) + $"&key={_whConfig.GmapsKey}",
                ThumbnailUrl = quest.GetIconUrl(),
                Color = DiscordColor.Orange
            };

            eb.Description = _lang.Translate("EMBED_QUEST_QUEST").FormatText(quest.GetMessage()) + "\r\n";
            //eb.Description += $"**Quest:** {quest.GetMessage()}\r\n";
            if (quest.Conditions != null && quest.Conditions.Count > 0)
            {
                var condition = quest.Conditions[0];
                eb.Description += _lang.Translate("EMBED_QUEST_CONDITION").FormatText(quest.GetConditionName()) + "\r\n";
                //eb.Description += $"**Condition:** {quest.GetConditionName()}\r\n";
            }
            eb.Description += _lang.Translate("EMBED_QUEST_REWARD").FormatText(quest.GetRewardString()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_TIME_REMAINING").FormatText(quest.TimeLeft.ToReadableStringNoSeconds()) + "\r\n";
            eb.Description += _lang.Translate("EMBED_LOCATION").FormatText(Math.Round(quest.Latitude, 5), Math.Round(quest.Longitude, 5)) + "\r\n";
            //eb.Description += $"**Reward:** {quest.GetRewardString()}\r\n";
            //eb.Description += $"**Time Remaining:** {quest.TimeLeft.ToReadableStringNoSeconds()}\r\n";
            //eb.Description += $"**Location:** {quest.Latitude},{quest.Longitude}\r\n";
            //eb.Description += $"**Address:** {Utils.GetGoogleAddress(quest.Latitude, quest.Longitude, _whConfig.GmapsKey)?.Address}\r\n";
            eb.Description += _lang.Translate("EMBED_GMAPS").FormatText(gmapsUrl) + "\r\n";
            //eb.Description += $"**[Google Maps Link]({gmapsUrl})**\r\n";
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = _client.Guilds.ContainsKey(_whConfig.GuildId) ? _client.Guilds[_whConfig.GuildId]?.IconUrl : string.Empty
            };

            return eb.Build();
        }

        private DiscordEmbed BuildPokestopMessage(PokestopData pokestop, string city)
        {
            return null;
        }

        private DiscordEmbed BuildGymMessage(GymData gym, string city)
        {
            return null;
        }

        #endregion

        #region Private Methods

        private async Task CreateEmojis()
        {
            _logger.Trace($"Bot::CreateEmojis");

            var guild = _client.Guilds[_whConfig.GuildId];
            for (var i = 0; i < Strings.EmojiList.Length; i++)
            {
                var emoji = Strings.EmojiList[i];
                var emojis = await guild.GetEmojisAsync();
                var emojiExists = emojis.FirstOrDefault(x => string.Compare(x.Name, emoji, true) == 0);
                if (emojiExists == null)
                {
                    _logger.Debug($"Emoji {emoji} doesn't exist, creating...");

                    var emojiPath = Path.Combine(Strings.EmojisFolder, emoji + ".png");
                    if (!File.Exists(emojiPath))
                    {
                        _logger.Error($"Failed to file emoji file at {emojiPath}, skipping...");
                        continue;
                    }

                    var fs = new FileStream(emojiPath, FileMode.Open, FileAccess.Read);
                    await guild.CreateEmojiAsync(emoji, fs, null, "Missing Valor emoji.");

                    _logger.Info($"Emoji {emoji} created successfully.");
                }
            }
        }

        private async Task ResetQuests()
        {
            _logger.Debug($"MIDNIGHT {DateTime.Now}");
            _logger.Debug($"Starting automatic quest messages cleanup...");

            for (var i = 0; i < _dep.WhConfig.QuestChannelIds.Count; i++)
            {
                var channel = await _client.GetChannelAsync(_dep.WhConfig.QuestChannelIds[i]);
                if (channel == null)
                {
                    _logger.Warn($"Failed to find channel by id {_dep.WhConfig.QuestChannelIds[i]}, skipping...");
                    continue;
                }

                var messages = await channel.GetMessagesAsync();
                while (messages.Count > 0)
                {
                    for (var j = 0; j < messages.Count; j++)
                    {
                        var message = messages[j];
                        if (message == null)
                            continue;

                        await message.DeleteAsync("Channel reset.");
                    }

                    messages = await channel.GetMessagesAsync();
                }

                _logger.Debug($"Deleted all {messages.Count.ToString("N0")} quest messages from channel {channel.Name}.");
            }

            _logger.Debug($"Finished automatic quest messages cleanup...");
        }

        private GeofenceItem GetGeofence(double latitude, double longitude)
        {
            var loc = _whm.GeofenceService.GetGeofence(_whm.Geofences.Select(x => x.Value).ToList(), new Location(latitude, longitude));
            return loc;
        }

        private string GetTypeEmojiIcons(List<PokemonType> pokemonTypes)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                if (!_client.Guilds.ContainsKey(_whConfig.GuildId))
                    continue;

                var emojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId($"types_{type.ToString().ToLower()}");
                var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, type.ToString().ToLower(), emojiId) : type.ToString().ToLower();
                if (!list.Contains(emojiName))
                {
                    list.Add(emojiName);
                }
            }

            return string.Join("/", list);
        }

        private string GetWeaknessEmojiIcons(List<PokemonType> pokemonTypes)
        {
            var list = new List<string>();
            foreach (var type in pokemonTypes)
            {
                var weaknessLst = type.ToString().StringToObject<PokemonType>().GetWeaknesses().Distinct();
                foreach (var weakness in weaknessLst)
                {
                    if (!_client.Guilds.ContainsKey(_whConfig.GuildId))
                        continue;

                    var emojiId = _client.Guilds[_whConfig.GuildId].GetEmojiId($"types_{weakness.ToString().ToLower()}");
                    var emojiName = emojiId > 0 ? string.Format(Strings.TypeEmojiSchema, weakness.ToString().ToLower(), emojiId) : weakness.ToString();
                    if (!list.Contains(emojiName))
                    {
                        list.Add(emojiName);
                    }
                }
            }

            return string.Join(" ", list);
        }

        #endregion

        //private void ParseLogs()
        //{
        //    var logsFile = "logs.log";
        //    var logLines = System.IO.File.ReadAllLines(logsFile);
        //    for (var i = 0; i < logLines.Length; i++)
        //    {
        //        var logLine = logLines[i];
        //        var logData = logLine.Split(' ');
        //        var dateTime = $"{logData[0]} {logData[1]}";
        //        var logType = logData[2];
        //        var logMessage = string.Join(" ", logData.Skip(3));

        //        Console.WriteLine($"{dateTime} {logType} {logMessage}");
        //    }
        //}
    }
}