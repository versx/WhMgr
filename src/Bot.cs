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
    using WhMgr.Data.Subscriptions;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Geofence;
    using WhMgr.Localization;
    using WhMgr.Net.Models;
    using WhMgr.Net.Webhooks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.Exceptions;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;

    //TODO: User subscriptions and Pokemon, Raid, and Quest alarm statistics by day. date/pokemonId/count
    //TODO: Optimize webhook and subscription processing.
    //TODO: Raid lobby manager
    //TODO: Optimize database queries
    //TODO: Reload config on change

    public class Bot
    {
        #region Variables

        private readonly DiscordClient _client;
        private readonly CommandsNextModule _commands;
        private readonly InteractivityModule _interactivity;
        private readonly Dependencies _dep;
        private readonly WebhookManager _whm;
        private readonly WhConfig _whConfig;
        private readonly SubscriptionProcessor _subProcessor;
        private readonly EmbedBuilder _embedBuilder;
        private readonly Translator _lang;

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        #endregion

        #region Constructor

        public Bot(WhConfig whConfig, string alarmsFilePath)
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            //_logger = EventLogger.GetLogger(name);
            _logger.Trace($"Bot::Bot [WhConfig={whConfig.GuildId}, OwnerId={whConfig.OwnerId}, SupporterRoleId={whConfig.SupporterRoleId}, WebhookPort={whConfig.WebhookPort}]");

            _lang = new Translator();
            _whConfig = whConfig;
            DataAccessLayer.ConnectionString = _whConfig.ConnectionString;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                _logger.Debug("Unhandled exception caught.");
                _logger.Error((Exception)e.ExceptionObject);

                if (e.IsTerminating)
                {
                    if (_client != null)
                    {
                        var owner = await _client.GetUserAsync(_whConfig.OwnerId);
                        if (owner == null)
                        {
                            _logger.Warn($"Failed to get owner from id {_whConfig.OwnerId}.");
                            return;
                        }

                        await _client.SendDirectMessage(owner, Strings.CrashMessage, null);
                    }
                }
            };

            _whm = new WebhookManager(_whConfig, alarmsFilePath);
            _whm.PokemonAlarmTriggered += OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered += OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered += OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered += OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered += OnGymAlarmTriggered;
            _whm.GymDetailsAlarmTriggered += OnGymDetailsAlarmTriggered;
            if (_whConfig.EnableSubscriptions)
            {
                _whm.PokemonSubscriptionTriggered += OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered += OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered += OnQuestSubscriptionTriggered;
                _whm.InvasionSubscriptionTriggered += OnInvasionSubscriptionTriggered;
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

            _interactivity = _client.UseInteractivity
            (
                new InteractivityConfiguration
                {
                    // default pagination behaviour to just ignore the reactions
                    PaginationBehaviour = TimeoutBehaviour.Ignore,

                    // default pagination timeout to 5 minutes
                    PaginationTimeout = TimeSpan.FromMinutes(5), //TODO: Set prod

                    // default timeout for other actions to 2 minutes
                    Timeout = TimeSpan.FromMinutes(2) //TODO: Set prod
                }
            );

            _embedBuilder = new EmbedBuilder(_client, _whConfig, _lang);
            if (_whConfig.EnableSubscriptions)
            {
                _subProcessor = new SubscriptionProcessor(_client, _whConfig, _whm, _embedBuilder);
            }

            DependencyCollection dep;
            using (var d = new DependencyCollectionBuilder())
            {
                d.AddInstance(_dep = new Dependencies(_interactivity, _whm, _subProcessor, _whConfig, _lang));
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
            _commands.RegisterCommands<CommunityDay>();
            _commands.RegisterCommands<ShinyStats>();
            if (_whConfig.EnableSubscriptions)
            {
                _commands.RegisterCommands<Notifications>();
            }
            if (_whConfig.EnableCities)
            {
                _commands.RegisterCommands<Feeds>();
            }
            _commands.RegisterCommands<Quests>();
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
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, Strings.BotName, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? e.Context.Message.Content}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

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
                await e.Context.RespondAsync(string.Empty, embed: embed);
            }
            else if (e.Exception is ArgumentException)
            {
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                var example = $"Command Example: ```{_whConfig.CommandPrefix}{e.Command.Name} {string.Join(" ", e.Command.Arguments.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name))}```\r\n*Parameters in brackets are optional.*";

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji} Invalid Argument(s)",
                    Description = $"{string.Join(Environment.NewLine, e.Command.Arguments.Select(x => $"Parameter **{x.Name}** expects type **{x.Type}.**"))}.\r\n\r\n{example}",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(string.Empty, embed: embed);
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
            if (e.Application == "REST")
            {
                _logger.Error("RATE LIMITED-----------------");
                return;
            }

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

        #region WebhookManager Events

        private async void OnPokemonAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<PokemonData> e)
        {
            _logger.Info($"Pokemon Found [Alarm: {e.Alarm.Name}, Pokemon: {e.Data.Id}, Despawn: {e.Data.DespawnTime}]");

            if (!_whm.WebHooks.ContainsKey(e.Alarm.Name))
                return;

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var pokemon = e.Data;
            var pkmn = Database.Instance.Pokemon[pokemon.Id];
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokemon.Latitude, pokemon.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            try
            {
                var form = pokemon.Id.GetPokemonForm(pokemon.FormId.ToString());
                //var costume = e.Pokemon.Id.GetCostume(e.Pokemon.Costume.ToString());
                //var costumeFormatted = (string.IsNullOrEmpty(costume) ? "" : " " + costume);
                var pkmnImage = pokemon.Id.GetPokemonImage(_whConfig.Urls.PokemonImage, pokemon.Gender, pokemon.FormId, pokemon.Costume);
                var eb = _embedBuilder.BuildPokemonMessage(pokemon, loc.Name);

                var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
                var name = $"{pkmn.Name}{pokemon.Gender.GetPokemonGenderIcon()}{form}";
                await whData.ExecuteAsync(string.Empty, name, pkmnImage, false, new List<DiscordEmbed> { eb });
                //TODO: Change to raw webhooks
                Statistics.Instance.PokemonSent++;
                Statistics.Instance.IncrementPokemonStats(pokemon.Id);

                if (pokemon.IV == "100%")
                {
                    Statistics.Instance.Add100Percent(pokemon);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void OnRaidAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<RaidData> e)
        {
            _logger.Info($"Raid Found [Alarm: {e.Alarm.Name}, Raid: {e.Data.PokemonId}, Level: {e.Data.Level}, StartTime: {e.Data.StartTime}]");

            if (!_whm.WebHooks.ContainsKey(e.Alarm.Name))
                return;

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var raid = e.Data;
            var pkmn = Database.Instance.Pokemon[raid.PokemonId];
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(raid.Latitude, raid.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city from coordinates {raid.Latitude},{raid.Longitude} {pkmn.Name} {raid.Level}, skipping...");
                return;
            }

            try
            {
                var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                var pkmnImage = raid.IsEgg ? string.Format(_whConfig.Urls.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(_whConfig.Urls.PokemonImage, PokemonGender.Unset, raid.Form);
                var eb = _embedBuilder.BuildRaidMessage(raid, loc.Name);

                var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
                var name = raid.IsEgg ? $"Level {raid.Level} {pkmn.Name}" : $"{(string.IsNullOrEmpty(form) ? null : form + "-")}{pkmn.Name} Raid";
                await whData.ExecuteAsync(string.Empty, name, pkmnImage, false, new List<DiscordEmbed> { eb });
                Statistics.Instance.RaidsSent++;
                if (raid.PokemonId > 0)
                {
                    Statistics.Instance.IncrementRaidStats(raid.PokemonId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void OnQuestAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<QuestData> e)
        {
            _logger.Info($"Quest Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Data.PokestopId}, Type={e.Data.Type}]");

            if (!_whm.WebHooks.ContainsKey(e.Alarm.Name))
                return;

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var quest = e.Data;
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(quest.Latitude, quest.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city for coordinates {quest.Latitude},{quest.Longitude}, skipping...");
                return;
            }

            try
            {
                var eb = _embedBuilder.BuildQuestMessage(quest, loc?.Name ?? e.Alarm.Name);
                var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
                await whData.ExecuteAsync(string.Empty, quest.GetQuestMessage() ?? string.Empty, quest.GetIconUrl(_whConfig) ?? string.Empty, false, new List<DiscordEmbed> { eb });
                Statistics.Instance.QuestsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void OnPokestopAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<PokestopData> e)
        {
            _logger.Info($"Pokestop Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Data.PokestopId}, LureExpire={e.Data.LureExpire}, InvasionExpire={e.Data.IncidentExpire}]");

            if (!_whm.WebHooks.ContainsKey(e.Alarm.Name))
                return;

            var wh = _whm.WebHooks[e.Alarm.Name];
            if (wh == null)
            {
                _logger.Error($"Failed to parse webhook data from {e.Alarm.Name} {e.Alarm.Webhook}.");
                return;
            }

            var pokestop = e.Data;
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokestop.Latitude, pokestop.Longitude));
            if (loc == null)
            {
                _logger.Warn($"Failed to lookup city for coordinates {pokestop.Latitude},{pokestop.Longitude}, skipping...");
                return;
            }

            string icon;
            if (pokestop.HasInvasion)
            {
                //TODO: Load from local file
                icon = "http://images2.fanpop.com/image/photos/11300000/Team-Rocket-Logo-team-rocket-11302897-198-187.jpg";
            }
            else if (pokestop.HasLure)
            {
                icon = string.Format(_whConfig.Urls.QuestImage, Convert.ToInt32(pokestop.LureType));//"https://serebii.net/pokemongo/items/luremodule.png";
            }
            else
            {
                icon = pokestop.Url;
            }

            try
            {
                var eb = _embedBuilder.BuildPokestopMessage(pokestop, loc?.Name ?? e.Alarm.Name);
                var whData = await _client.GetWebhookWithTokenAsync(wh.Id, wh.Token);
                await whData.ExecuteAsync(string.Empty, pokestop.Name ?? "Unknown Pokestop", icon, false, new List<DiscordEmbed> { eb });
                //Statistics.Instance.QuestsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnGymAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<GymData> e)
        {
            _logger.Info($"Gym Found [Alarm: {e.Alarm.Name}, GymId: {e.Data.GymId}, Team={e.Data.Team}, SlotsAvailable={e.Data.SlotsAvailable}, GuardPokemonId={e.Data.GuardPokemonId}]");

            if (!e.Alarm.Filters.Gyms.Enabled)
                return;

            //TODO: Implement gym alarms.
        }

        private void OnGymDetailsAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<GymDetailsData> e)
        {
            _logger.Info($"Gym Details Found [Alarm: {e.Alarm.Name}, GymId: {e.Data.GymId}, InBattle={e.Data.InBattle}, Team={e.Data.Team}]");

            if (!e.Alarm.Filters.Gyms.Enabled)
                return;

            var gymDetails = e.Data;
            if (gymDetails.InBattle)
            {
                //TODO: Send gym battle alarms.
            }
        }

        #endregion

        #region Subscription Events

        private void OnPokemonSubscriptionTriggered(object sender, PokemonData e)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            if (_subProcessor == null)
                return;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            new System.Threading.Thread(async () => await _subProcessor.ProcessPokemonSubscription(e)) { IsBackground = true }.Start();
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            //_subProcessor.EnqueuePokemonSubscription(e);
            //Task.Run(() => _subProcessor.ProcessPokemonSubscription(e));
        }

        private void OnRaidSubscriptionTriggered(object sender, RaidData e)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            if (_subProcessor == null)
                return;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            new System.Threading.Thread(async () => await _subProcessor.ProcessRaidSubscription(e)) { IsBackground = true }.Start();
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            //_subProcessor.EnqueueRaidSubscription(e);
            //Task.Run(() => _subProcessor.ProcessRaidSubscription(e));
        }

        private void OnQuestSubscriptionTriggered(object sender, QuestData e)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            if (_subProcessor == null)
                return;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            new System.Threading.Thread(async () => await _subProcessor.ProcessQuestSubscription(e)) { IsBackground = true }.Start();
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            //_subProcessor.EnqueueQuestSubscription(e);
        }

        private void OnInvasionSubscriptionTriggered(object sender, PokestopData e)
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            if (_subProcessor == null)
                return;

            new System.Threading.Thread(async () => await _subProcessor.ProcessInvasionSubscription(e)) { IsBackground = true }.Start();
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

            Statistics.Instance.WriteOut();
            Statistics.Instance.Reset();

            if (_whConfig.EnableSubscriptions)
            {
                //_subProcessor.Manager.RemoveAllSnoozedQuests();
            }

            if (_whConfig.ShinyStats.Enabled)
            {
                var statsChannel = await _client.GetChannelAsync(_whConfig.ShinyStats.ChannelId);
                if (statsChannel == null)
                {
                    _logger.Warn($"Failed to get channel id {_whConfig.ShinyStats.ChannelId} to post shiny stats.");
                }
                else
                {
                    if (_whConfig.ShinyStats.ClearMessages)
                    {
                        await _client.DeleteMessages(_whConfig.ShinyStats.ChannelId);
                    }

                    await statsChannel.SendMessageAsync($"[**Shiny Pokemon stats for {DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString()}**]\r\n----------------------------------------------");
                    var stats = await ShinyStats.GetStats(_whConfig);
                    var sorted = stats.Keys.ToList();
                    sorted.Sort();

                    foreach (var pokemon in sorted)
                    {
                        if (pokemon == 0)
                            continue;

                        if (!Database.Instance.Pokemon.ContainsKey(pokemon))
                            continue;

                        var pkmn = Database.Instance.Pokemon[pokemon];
                        var pkmnStats = stats[pokemon];
                        var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                        var chanceMessage = chance == 0 ? null : $" with a **1/{chance}** ratio";
                        await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**  |  **{pkmnStats.Shiny.ToString("N0")}** shiny out of **{pkmnStats.Total.ToString("N0")}** total seen in the last 24 hours{chanceMessage}.");
                    }

                    var total = stats[0];
                    var ratio = total.Shiny == 0 || total.Total == 0 ? null : $" with a **1/{Convert.ToInt32(total.Total / total.Shiny)}** ratio in total";
                    await statsChannel.SendMessageAsync($"Found **{total.Shiny.ToString("N0")}** total shinies out of **{total.Total.ToString("N0")}** possiblities{ratio}.");
                }
            }

            var channelIds = _dep.WhConfig.QuestChannelIds;
            for (var i = 0; i < channelIds.Count; i++)
            {
                var item = await _client.DeleteMessages(channelIds[i]);
                _logger.Debug($"Deleted all {item.Item2.ToString("N0")} quest messages from channel {item.Item1.Name}.");
            }

            _logger.Debug($"Finished automatic quest messages cleanup...");

            CleanupDepartedMembers();
        }

        private void CleanupDepartedMembers()
        {
            if (!_whConfig.EnableSubscriptions)
                return;

            _logger.Debug($"Checking if there are any subscriptions for members that are no longer apart of the server...");

            var users = _subProcessor.Manager.Subscriptions;// GetUserSubscriptions();
            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var discordUser = _client.GetMemberById(_whConfig.GuildId, user.UserId);
                if (discordUser == null)
                {
                    _logger.Debug($"Removing user {user.UserId} subscription settings because they are no longer a member of the server.");
                    if (!_subProcessor.Manager.RemoveAllUserSubscriptions(user.UserId))
                    {
                        _logger.Warn($"Could not remove user {user.UserId} subscription settings from the database.");
                    }
                }
            }
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