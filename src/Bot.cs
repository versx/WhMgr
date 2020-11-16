namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using WhMgr.Commands;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models.Discord;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Geofence;
    using WhMgr.Localization;
    using WhMgr.Net.Models;
    using WhMgr.Net.Webhooks;
    using WhMgr.Utilities;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;

    // TODO: Subscriptions, Pokemon, Raid, Quest, Invasion, Gym, Weather alarm statistics by day. date/pokemonId/count
    // TODO: List all subscriptions with info command
    // TODO: Multiple discord bot tokens per server
    // TODO: Check nests again
    // TODO: IV wildcards
    // TODO: Egg subscriptions (maybe)

    public class Bot
    {
        #region Variables

        private readonly Dictionary<ulong, DiscordClient> _servers;
        private readonly WebhookController _whm;
        private WhConfig _whConfig;
        private readonly SubscriptionProcessor _subProcessor;

        private static readonly IEventLogger _logger = EventLogger.GetLogger("BOT");

        #endregion

        #region Constructor

        /// <summary>
        /// Discord bot class
        /// </summary>
        /// <param name="whConfig">Configuration settings</param>
        public Bot(WhConfig whConfig)
        {
            _logger.Trace($"WhConfig [Servers={whConfig.Servers.Count}, Port={whConfig.WebhookPort}]");
            _servers = new Dictionary<ulong, DiscordClient>();
            _whConfig = whConfig;
            _whm = new WebhookController(_whConfig);

            // Build form lists for icons
            IconFetcher.Instance.SetIconStyles(_whConfig.IconStyles);

            // Set translation language
            Translator.Instance.SetLocale(_whConfig.Locale);

            // Set database connection strings to static properties so we can access within our extension classes
            DataAccessLayer.ConnectionString = _whConfig.Database.Main.ToString();
            DataAccessLayer.ScannerConnectionString = _whConfig.Database.Scanner.ToString();

            // Set unhandled exception event handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            // Initialize and start midnight reset timer
            var midnight = new DandTSoftware.Timers.MidnightTimer();
            midnight.TimeReached += async (e) => await OnMidnightTimer();
            midnight.Start();

            // Initialize the subscription processor if at least one Discord server wants custom notifications
            // and start database migrator
            if (_whConfig.Servers.Values.ToList().Exists(x => x.Subscriptions.Enabled))
            {
                // Start database migrator
                var migrator = new DatabaseMigrator();
                while (!migrator.Finished)
                {
                    Thread.Sleep(50);
                }

                _subProcessor = new SubscriptionProcessor(_servers, _whConfig, _whm);
            }

            // Create a DiscordClient object per Discord server in config
            var keys = _whConfig.Servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var server = _whConfig.Servers[guildId];
                server.LoadDmAlerts();
                var client = new DiscordClient(new DiscordConfiguration
                {
                    AutomaticGuildSync = true,
                    AutoReconnect = true,
                    EnableCompression = true,
                    Token = server.Token,
                    TokenType = TokenType.Bot,
                    UseInternalLogHandler = true
                });

                // If you are on Windows 7 and using .NETFX, install 
                // DSharpPlus.WebSocket.WebSocket4Net from NuGet,
                // add appropriate usings, and uncomment the following
                // line
                //client.SetWebSocketClient<WebSocket4NetClient>();

                // If you are on Windows 7 and using .NET Core, install 
                // DSharpPlus.WebSocket.WebSocket4NetCore from NuGet,
                // add appropriate usings, and uncomment the following
                // line
                //client.SetWebSocketClient<WebSocket4NetCoreClient>();

                // If you are using Mono, install 
                // DSharpPlus.WebSocket.WebSocketSharp from NuGet,
                // add appropriate usings, and uncomment the following
                // line
                //client.SetWebSocketClient<WebSocketSharpClient>();

                client.Ready += Client_Ready;
                client.GuildAvailable += Client_GuildAvailable;
                //_client.MessageCreated += Client_MessageCreated;
                client.ClientErrored += Client_ClientErrored;
                client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;

                // Configure Discord interactivity module
                var interactivity = client.UseInteractivity
                (
                    new InteractivityConfiguration
                    {
                        // default pagination behaviour to just ignore the reactions
                        PaginationBehaviour = TimeoutBehaviour.Ignore,

                        // default pagination timeout to 5 minutes
                        PaginationTimeout = TimeSpan.FromMinutes(5),

                        // default timeout for other actions to 2 minutes
                        Timeout = TimeSpan.FromMinutes(2)
                    }
                );

                // Build the dependency collection which will contain our objects that can be globally used within each command module
                DependencyCollection dep;
                using (var d = new DependencyCollectionBuilder())
                {
                    d.AddInstance(new Dependencies(interactivity, _whm, _subProcessor, _whConfig, new StripeService(_whConfig.StripeApiKey)));
                    dep = d.Build();
                }

                // Discord commands configuration
                var commands = client.UseCommandsNext
                (
                    new CommandsNextConfiguration
                    {
                        StringPrefix = server.CommandPrefix?.ToString(),
                        EnableDms = true,
                        // If command prefix is null, allow for mention prefix
                        EnableMentionPrefix = string.IsNullOrEmpty(server.CommandPrefix),
                        // Use DSharpPlus's built-in help formatter
                        EnableDefaultHelp = true,
                        CaseSensitive = false,
                        IgnoreExtraArguments = true,
                        Dependencies = dep
                    }
                );
                commands.CommandExecuted += Commands_CommandExecuted;
                commands.CommandErrored += Commands_CommandErrored;
                // Register Discord command handler classes
                commands.RegisterCommands<Owner>();
                commands.RegisterCommands<CommunityDay>();
                commands.RegisterCommands<Nests>();
                commands.RegisterCommands<ShinyStats>();
                commands.RegisterCommands<Gyms>();
                commands.RegisterCommands<Quests>();
                commands.RegisterCommands<Settings>();
                if (server.Subscriptions.Enabled)
                {
                    commands.RegisterCommands<Notifications>();
                }
                if (server.EnableCities)
                {
                    commands.RegisterCommands<Feeds>();
                }

                _logger.Info($"Configured Discord server {guildId}");
                if (!_servers.ContainsKey(guildId))
                {
                    _servers.Add(guildId, client);
                }

                // Wait 3 seconds between initializing Discord clients
                Task.Delay(3000).GetAwaiter().GetResult();
            }

            RegisterConfigMonitor();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the Discord bot(s)
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            _logger.Trace("Start");
            _logger.Info("Connecting to Discord...");

            // Loop through each Discord server and attempt initial connection
            var keys = _servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var client = _servers[guildId];

                _logger.Info($"Attempting connection to Discord server {guildId}");
                await client.ConnectAsync();
                await Task.Delay(1000);
            }

            // Register alarm event handlers
            _whm.PokemonAlarmTriggered += OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered += OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered += OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered += OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered += OnGymAlarmTriggered;
            _whm.GymDetailsAlarmTriggered += OnGymDetailsAlarmTriggered;
            _whm.WeatherAlarmTriggered += OnWeatherAlarmTriggered;
            // At least one server wants subscriptions
            if (_whConfig.Servers.FirstOrDefault(x => x.Value.Subscriptions.Enabled).Value != null)
            {
                // Register subscription event handlers
                _whm.PokemonSubscriptionTriggered += OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered += OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered += OnQuestSubscriptionTriggered;
                _whm.InvasionSubscriptionTriggered += OnInvasionSubscriptionTriggered;
            }
            _whm.Start();

            _logger.Info("WebhookManager is running...");
        }

        /// <summary>
        /// Stop the Discord bot(s)
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            _logger.Trace("Stop");
            _logger.Info("Disconnecting from Discord...");

            // Loop through each Discord server and terminate the connection
            var keys = _servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var client = _servers[guildId];

                _logger.Info($"Attempting connection to Discord server {guildId}");
                await client.DisconnectAsync();
                await Task.Delay(1000);
            }

            // Unregister alarm event handlers
            _whm.PokemonAlarmTriggered -= OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered -= OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered -= OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered -= OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered -= OnGymAlarmTriggered;
            _whm.GymDetailsAlarmTriggered -= OnGymDetailsAlarmTriggered;
            _whm.WeatherAlarmTriggered -= OnWeatherAlarmTriggered;
            if (_whConfig.Servers.FirstOrDefault(x => x.Value.Subscriptions.Enabled).Value != null)
            {
                //At least one server wanted subscriptions, unregister the subscription event handlers
                _whm.PokemonSubscriptionTriggered -= OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered -= OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered -= OnQuestSubscriptionTriggered;
                _whm.InvasionSubscriptionTriggered -= OnInvasionSubscriptionTriggered;
            }
            _whm.Stop();

            _logger.Info("WebhookManager is stopped...");
        }

        #endregion

        #region Discord Events

        private Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info($"------------------------------------------");
            _logger.Info($"[DISCORD] Connected.");
            _logger.Info($"[DISCORD] ----- Current Application");
            _logger.Info($"[DISCORD] Name: {e.Client.CurrentApplication.Name}");
            _logger.Info($"[DISCORD] Description: {e.Client.CurrentApplication.Description}");
            _logger.Info($"[DISCORD] Owner: {e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}");
            _logger.Info($"[DISCORD] ----- Current User");
            _logger.Info($"[DISCORD] Id: {e.Client.CurrentUser.Id}");
            _logger.Info($"[DISCORD] Name: {e.Client.CurrentUser.Username}#{e.Client.CurrentUser.Discriminator}");
            _logger.Info($"[DISCORD] Email: {e.Client.CurrentUser.Email}");
            _logger.Info($"------------------------------------------");

            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            // If guild is in configured servers list then attempt to create emojis needed
            if (_whConfig.Servers.ContainsKey(e.Guild.Id))
            {
                // Create default emojis
                await CreateEmojis(e.Guild.Id);

                if (!(e.Client is DiscordClient client))
                {
                    _logger.Error($"DiscordClient is null, Unable to update status.");
                    return;
                }

                // Set custom bot status if guild is in config server list
                if (_whConfig.Servers.ContainsKey(e.Guild.Id))
                {
                    var status = _whConfig.Servers[e.Guild.Id].Status;
                    await client.UpdateStatusAsync(new DiscordGame(status ?? $"v{Strings.Version}"), UserStatus.Online);
                }
            }
        }

        //private async Task Client_MessageCreated(MessageCreateEventArgs e)
        //{
        //    if (e.Author.Id == e.Client.CurrentUser.Id)
        //        return;

        //    if (_whConfig.BotChannelIds.Count > 0 && !_whConfig.BotChannelIds.Contains(e.Channel.Id))
        //        return;

        //    await _commands.HandleCommandsAsync(e);
        //}

        private async Task Client_ClientErrored(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception);

            await Task.CompletedTask;
        }

        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, Strings.BotName, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            await Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Error, Strings.BotName, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? e.Context.Message.Content}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

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
                await e.Context.RespondAsync(embed: embed);
            }
            else if (e.Exception is ArgumentException)
            {
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                var prefix = _whConfig.Servers.ContainsKey(e.Context.Guild.Id) ? _whConfig.Servers[e.Context.Guild.Id].CommandPrefix : "!";
                var example = $"Command Example: ```{prefix}{e.Command.Name} {string.Join(" ", e.Command.Arguments.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name))}```\r\n*Parameters in brackets are optional.*";

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji} Invalid Argument(s)",
                    Description = $"{string.Join(Environment.NewLine, e.Command.Arguments.Select(x => $"Parameter **{x.Name}** expects type **{x.Type}.**"))}.\r\n\r\n{example}",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed: embed);
            }
            else if (e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException)
            {
                _logger.Warn($"User {e.Context.User.Username} tried executing command {e.Context.Message.Content} but command does not exist.");
            }
            else
            {
                _logger.Error($"User {e.Context.User.Username} tried executing command {e.Command?.Name} and unknown error occurred.\r\n: {e.Exception}");
            }
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            if (e.Application == "REST")
            {
                _logger.Error("[DISCORD] RATE LIMITED-----------------");
                return;
            }

            //Color
            ConsoleColor color;
            switch (e.Level)
            {
                case DSharpPlus.LogLevel.Error: color = ConsoleColor.DarkRed; break;
                case DSharpPlus.LogLevel.Warning: color = ConsoleColor.Yellow; break;
                case DSharpPlus.LogLevel.Info: color = ConsoleColor.White; break;
                case DSharpPlus.LogLevel.Critical: color = ConsoleColor.Red; break;
                case DSharpPlus.LogLevel.Debug: default: color = ConsoleColor.DarkGray; break;
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
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Pokemon Found [Alarm: {e.Alarm.Name}, Pokemon: {e.Data.Id}, Despawn: {e.Data.DespawnTime}]");

            var pokemon = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokemon.Latitude, pokemon.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[POKEMON] Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var server = _whConfig.Servers[e.GuildId];
                var client = _servers[e.GuildId];
                var eb = await pokemon.GeneratePokemonMessage(e.GuildId, client, _whConfig, e.Alarm, loc.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username,
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);
                Statistics.Instance.PokemonAlarmsSent++;

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

        private void OnRaidAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<RaidData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Raid Found [Alarm: {e.Alarm.Name}, Raid: {e.Data.PokemonId}, Level: {e.Data.Level}, StartTime: {e.Data.StartTime}]");

            var raid = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(raid.Latitude, raid.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[RAID] Failed to lookup city from coordinates {raid.Latitude},{raid.Longitude} {pkmn.Name} {raid.Level}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var server = _whConfig.Servers[e.GuildId];
                var client = _servers[e.GuildId];
                var eb = raid.GenerateRaidMessage(e.GuildId, client, _whConfig, e.Alarm, loc.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username,
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);
                if (raid.IsEgg)
                    Statistics.Instance.EggAlarmsSent++;
                else
                    Statistics.Instance.RaidAlarmsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnQuestAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<QuestData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Quest Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Data.PokestopId}, Type={e.Data.Type}]");

            var quest = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(quest.Latitude, quest.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[QUEST] Failed to lookup city for coordinates {quest.Latitude},{quest.Longitude}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var client = _servers[e.GuildId];
                var eb = quest.GenerateQuestMessage(e.GuildId, client, _whConfig, e.Alarm, loc?.Name ?? e.Alarm.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username,
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);
                Statistics.Instance.QuestAlarmsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPokestopAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<PokestopData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Pokestop Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Data.PokestopId}, LureExpire={e.Data.LureExpire}, InvasionExpire={e.Data.IncidentExpire}]");

            var pokestop = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokestop.Latitude, pokestop.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[POKESTOP] Failed to lookup city for coordinates {pokestop.Latitude},{pokestop.Longitude}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var client = _servers[e.GuildId];
                var eb = pokestop.GeneratePokestopMessage(e.GuildId, client, _whConfig, e.Alarm, loc?.Name ?? e.Alarm.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username ?? Translator.Instance.Translate("UNKNOWN_POKESTOP"),
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);
                if (pokestop.HasInvasion)
                    Statistics.Instance.InvasionAlarmsSent++;
                else if (pokestop.HasLure)
                    Statistics.Instance.LureAlarmsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnGymAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<GymData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Gym Found [Alarm: {e.Alarm.Name}, GymId: {e.Data.GymId}, Team={e.Data.Team}, SlotsAvailable={e.Data.SlotsAvailable}, GuardPokemonId={e.Data.GuardPokemonId}]");

            //TODO: Implement gym alarms.
        }

        private void OnGymDetailsAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<GymDetailsData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Gym Details Found [Alarm: {e.Alarm.Name}, GymId: {e.Data.GymId}, InBattle={e.Data.InBattle}, Team={e.Data.Team}]");

            var gymDetails = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(gymDetails.Latitude, gymDetails.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var oldGym = _whm.Gyms[gymDetails.GymId];
                var changed = oldGym.Team != gymDetails.Team || gymDetails.InBattle || oldGym.SlotsAvailable != gymDetails.SlotsAvailable;
                if (!changed)
                    return;

                var client = _servers[e.GuildId];
                var eb = gymDetails.GenerateGymMessage(e.GuildId, client, _whConfig, e.Alarm, _whm.Gyms[gymDetails.GymId], loc?.Name ?? e.Alarm.Name);
                var name = gymDetails.GymName;
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username,
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);
                Statistics.Instance.GymAlarmsSent++;

                // Gym team changed, set gym in gym cache
                _whm.SetGym(gymDetails.GymId, gymDetails);

                Statistics.Instance.GymAlarmsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnWeatherAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<WeatherData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Weather Found [Alarm: {e.Alarm.Name}, S2CellId: {e.Data.Id}, Condition={e.Data.GameplayCondition}, Severity={e.Data.Severity}]");

            var weather = e.Data;
            var loc = GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(weather.Latitude, weather.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            if (!_servers.ContainsKey(e.GuildId))
                return;

            if (!_whConfig.Servers.ContainsKey(e.GuildId))
                return;

            try
            {
                var client = _servers[e.GuildId];
                var eb = weather.GenerateWeatherMessage(e.GuildId, client, _whConfig, e.Alarm, loc?.Name ?? e.Alarm.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = eb.Username,
                    AvatarUrl = eb.IconUrl,
                    Content = eb.Description,
                    Embeds = eb.Embeds
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

                // Weather changed, set weather in weather cache
                _whm.SetWeather(weather.Id, weather.GameplayCondition);

                Statistics.Instance.WeatherAlarmsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        #endregion

        #region Subscription Events

        private void OnPokemonSubscriptionTriggered(object sender, PokemonData e)
        {
            if (_subProcessor == null)
                return;

            if (!ThreadPool.QueueUserWorkItem(async x => await _subProcessor.ProcessPokemonSubscription(e)))
            {
                // Failed to queue thread
                _logger.Error($"Failed to queue thread to process Pokemon subscription");
            }
            if (!ThreadPool.QueueUserWorkItem(async x => await _subProcessor.ProcessPvPSubscription(e)))
            {
                // Failed to queue thread
                _logger.Error($"Failed to queue thread to process PVP subscription");
            }
        }

        private void OnRaidSubscriptionTriggered(object sender, RaidData e)
        {
            if (_subProcessor == null)
                return;

            if (!ThreadPool.QueueUserWorkItem(async x => await _subProcessor.ProcessRaidSubscription(e)))
            {
                // Failed to queue thread
                _logger.Error($"Failed to queue thread to process raid subscription");
            }
        }

        private void OnQuestSubscriptionTriggered(object sender, QuestData e)
        {
            if (_subProcessor == null)
                return;

            if (!ThreadPool.QueueUserWorkItem(async x => await _subProcessor.ProcessQuestSubscription(e)))
            {
                // Failed to queue thread
                _logger.Error($"Failed to queue thread to process quest subscription");
            }
        }

        private void OnInvasionSubscriptionTriggered(object sender, PokestopData e)
        {
            if (_subProcessor == null)
                return;

            if (!ThreadPool.QueueUserWorkItem(async x => await _subProcessor.ProcessInvasionSubscription(e)))
            {
                // Failed to queue thread
                _logger.Error($"Failed to queue thread to process invasion subscription");
            }
        }

        #endregion

        #region Private Methods

        private async Task CreateEmojis(ulong guildId)
        {
            _logger.Trace($"CreateEmojis");

            if (!_servers.ContainsKey(guildId))
            {
                _logger.Warn($"Discord client not ready yet to create emojis for guild {guildId}");
                return;
            }

            var server = _whConfig.Servers[guildId];
            var client = _servers[guildId];
            if (!(client.Guilds?.ContainsKey(server.EmojiGuildId) ?? false))
            {
                _logger.Warn($"Bot not in emoji server {server.EmojiGuildId}");
                return;
            }

            var guild = client.Guilds[server.EmojiGuildId];
            for (var j = 0; j < Strings.EmojiList.Length; j++)
            {
                try
                {
                    var emoji = Strings.EmojiList[j];
                    var emojis = await guild.GetEmojisAsync();
                    var emojiExists = emojis.FirstOrDefault(x => string.Compare(x.Name, emoji, true) == 0);
                    if (emojiExists == null)
                    {
                        _logger.Debug($"Emoji {emoji} doesn't exist, creating...");

                        var emojiPath = Path.Combine(Strings.EmojisFolder, emoji + ".png");
                        if (!File.Exists(emojiPath))
                        {
                            _logger.Error($"Unable to find emoji file at {emojiPath}, skipping...");
                            continue;
                        }

                        var fs = new FileStream(emojiPath, FileMode.Open, FileAccess.Read);
                        await guild.CreateEmojiAsync(emoji, fs, null, $"Missing `{emoji}` emoji.");

                        _logger.Info($"Emoji {emoji} created successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            await CacheGuildEmojisList();
        }

        private async Task CacheGuildEmojisList()
        {
            _logger.Trace($"LoadEmojis");

            var keys = _whConfig.Servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var emojiGuildId = _whConfig.Servers[guildId].EmojiGuildId;
                if (!_servers.ContainsKey(guildId))
                    continue;

                var configGuild = _servers[guildId];
                if (!configGuild.Guilds.ContainsKey(emojiGuildId))
                    continue;

                var emojiGuild = configGuild.Guilds[emojiGuildId];
                var emojis = await emojiGuild.GetEmojisAsync();
                for (var j = 0; j < Strings.EmojiList.Length; j++)
                {
                    var name = Strings.EmojiList[j];
                    var emoji = emojis.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0);
                    if (emoji == null)
                        continue;

                    if (!MasterFile.Instance.Emojis.ContainsKey(emoji.Name))
                    {
                        MasterFile.Instance.Emojis.Add(emoji.Name, emoji.Id);
                        continue;
                    }
                }
            }
        }

        private async Task OnMidnightTimer()
        {
            _logger.Debug($"MIDNIGHT {DateTime.Now}");

            Statistics.WriteOut();
            Statistics.Instance.Reset();

            var keys = _whConfig.Servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var server = _whConfig.Servers[guildId];

                if (!_servers.ContainsKey(guildId))
                {
                    _logger.Warn($"{guildId} guild does not exist it Discord servers.");
                    continue;
                }
                var client = _servers[guildId];
                if (server.ShinyStats.Enabled)
                {
                    _logger.Debug($"Starting Shiny Stat posting...");
                    await PostShinyStats(client, guildId, server);
                }
                else
                {
                    _logger.Debug($"Shiny Stat posting not enabled...skipping");
                }

                if (server.PruneQuestChannels && server.QuestChannelIds.Count > 0)
                {
                    _logger.Debug($"Starting automatic quest messages cleanup...");
                    await PruneQuestChannels(client, server);
                }
                else
                {
                    _logger.Debug($"Quest cleanup not enabled...skipping");
                }

                Thread.Sleep(10 * 1000);
            }

            CleanupDepartedMembers();
        }

        private async Task PostShinyStats(DiscordClient client, ulong guildId, DiscordServerConfig server)
        {
            var statsChannel = await client.GetChannelAsync(server.ShinyStats.ChannelId);
            if (statsChannel == null)
            {
                _logger.Warn($"Unable to get channel id {server.ShinyStats.ChannelId} to post shiny stats.");
            }
            else
            {
                if (server.ShinyStats.ClearMessages)
                {
                    _logger.Debug($"Deleting previous shiny stats messages in channel {server.ShinyStats.ChannelId}");
                    await client.DeleteMessages(server.ShinyStats.ChannelId);
                }

                //var guildId = server.GuildId;
                _logger.Debug($"Posting shiny stats for guild {client.Guilds[guildId].Name} ({guildId}) in channel {server.ShinyStats.ChannelId}");
                // Subtract an hour to make sure it shows yesterday's date.
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TITLE").FormatText(DateTime.Now.Subtract(TimeSpan.FromHours(1)).ToLongDateString()));
                Thread.Sleep(500);
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_NEWLINE"));
                var stats = await ShinyStats.GetShinyStats(_whConfig.Database.Scanner.ToString());
                if (stats == null)
                {
                    _logger.Error($"Failed to get list of shiny stats for guild {guildId}, skipping...");
                    return;
                }

                var sorted = stats.Keys.ToList();
                sorted.Sort();

                foreach (var pokemon in sorted)
                {
                    if (pokemon == 0)
                        continue;

                    if (!MasterFile.Instance.Pokedex.ContainsKey((int)pokemon))
                        continue;

                    var pkmn = MasterFile.Instance.Pokedex[(int)pokemon];
                    var pkmnStats = stats[pokemon];
                    var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                    if (chance == 0)
                    {
                        await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE").FormatText(pkmn.Name, pokemon, pkmnStats.Shiny.ToString("N0"), pkmnStats.Total.ToString("N0")));
                    }
                    else
                    {
                        await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE_WITH_RATIO").FormatText(pkmn.Name, pokemon, pkmnStats.Shiny.ToString("N0"), pkmnStats.Total.ToString("N0"), chance));
                    }
                    Thread.Sleep(500);
                }

                var total = stats[0];
                var totalRatio = total.Shiny == 0 || total.Total == 0 ? 0 : Convert.ToInt32(total.Total / total.Shiny);
                if (totalRatio == 0)
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE").FormatText(total.Shiny.ToString("N0"), total.Total.ToString("N0")));
                }
                else
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(total.Shiny.ToString("N0"), total.Total.ToString("N0"), totalRatio));
                }
            }

            Thread.Sleep(10 * 1000);
        }

        private async Task PruneQuestChannels(DiscordClient client, DiscordServerConfig server)
        {
            try
            {
                var channelIds = server.QuestChannelIds;
                _logger.Debug($"Quest channel pruning started for {channelIds.Count:N0} channels...");
                for (var j = 0; j < channelIds.Count; j++)
                {
                    var result = await client.DeleteMessages(channelIds[j]);
                    _logger.Debug($"Deleted all {result.Item2:N0} quest messages from channel {result.Item1.Name}.");
                    Thread.Sleep(1000);
                }
                _logger.Debug($"Finished automatic quest messages cleanup...");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void CleanupDepartedMembers()
        {
            _logger.Trace("CleanupDepartedMembers");

            var keys = _servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var client = _servers[guildId];
                var server = _whConfig.Servers[guildId];
                if (!server.Subscriptions.Enabled)
                    return;

                _logger.Debug($"Checking if there are any subscriptions for members that are no longer apart of the server...");

                var users = _subProcessor.Manager.Subscriptions;
                for (var j = 0; j < users.Count; j++)
                {
                    var user = users[j];
                    var discordUser = client.GetMemberById(guildId, user.UserId);
                    if (discordUser == null)
                    {
                        _logger.Debug($"Removing user {user.UserId} subscription settings because they are no longer a member of the server.");
                        if (!SubscriptionManager.RemoveAllUserSubscriptions(user.GuildId, user.UserId))
                        {
                            _logger.Warn($"Unable to remove user {user.UserId} subscription settings from the database.");
                            continue;
                        }
                        _logger.Info($"Successfully removed user {user.UserId}'s subscription settings from the database.");
                    }
                }
            }
        }

        private void RegisterConfigMonitor()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), Strings.ConfigFileName);
            var fileWatcher = new FileWatcher(path);
            fileWatcher.FileChanged += (sender, e) => _whConfig = WhConfig.Load(e.FilePath);
            fileWatcher.Start();
        }

        private async void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Debug("Unhandled exception caught.");
            _logger.Error((Exception)e.ExceptionObject);

            if (e.IsTerminating)
            {
                var keys = _whConfig.Servers.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    var guildId = keys[i];
                    if (!_whConfig.Servers.ContainsKey(guildId))
                    {
                        _logger.Error($"Unable to find guild id {guildId} in server config list.");
                        continue;
                    }
                    var server = _whConfig.Servers[guildId];

                    if (!_servers.ContainsKey(guildId))
                    {
                        _logger.Error($"Unable to find guild id {guildId} in Discord server client list.");
                        continue;
                    }
                    var client = _servers[guildId];
                    if (client != null)
                    {
                        var owner = await client.GetUserAsync(server.OwnerId);
                        if (owner == null)
                        {
                            _logger.Warn($"Unable to get owner from id {server.OwnerId}.");
                            return;
                        }

                        await client.SendDirectMessage(owner, Translator.Instance.Translate("BOT_CRASH_MESSAGE"), null);
                    }
                }
            }
        }

        #endregion
    }
}
