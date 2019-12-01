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

    //TODO: User subscriptions and Pokemon, Raid, and Quest alarm statistics by day. date/pokemonId/count
    //TODO: Reload config on change
    //TODO: PvP ranks dts
    //TODO: Separate subscriptions dts

    public class Bot
    {
        #region Variables

        private readonly Dictionary<ulong, DiscordClient> _servers;
        private readonly WebhookController _whm;
        private readonly WhConfig _whConfig;
        private readonly SubscriptionProcessor _subProcessor;
        private readonly Translator _lang;
        private readonly Dictionary<string, GymDetailsData> _gyms;

        private static readonly IEventLogger _logger = EventLogger.GetLogger("BOT");

        #endregion

        #region Constructor

        public Bot(WhConfig whConfig)
        {
            _logger.Trace($"WhConfig={whConfig.Servers.Count}, WebhookPort={whConfig.WebhookPort}");
            _lang = new Translator();
            _servers = new Dictionary<ulong, DiscordClient>();
            _whConfig = whConfig;
            _gyms = new Dictionary<string, GymDetailsData>();
            _whm = new WebhookController(_whConfig);

            DataAccessLayer.ConnectionString = _whConfig.Database.Main.ToString();
            DataAccessLayer.ScannerConnectionString = _whConfig.Database.Scanner.ToString();

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            var midnight = new DandTSoftware.Timers.MidnightTimer();
            midnight.TimeReached += async (e) => await ResetQuests();
            midnight.Start();

            if (_whConfig.Servers.Values.ToList().Exists(x => x.EnableSubscriptions))
            {
                _subProcessor = new SubscriptionProcessor(_servers, _whConfig, _whm);
            }

            var keys = _whConfig.Servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var server = _whConfig.Servers[guildId];
                var client = new DiscordClient(new DiscordConfiguration
                {
                    AutomaticGuildSync = true,
                    AutoReconnect = true,
                    EnableCompression = true,
                    Token = server.Token,
                    TokenType = TokenType.Bot,
                    UseInternalLogHandler = true
                });
                client.Ready += Client_Ready;
                //_client.MessageCreated += Client_MessageCreated;
                client.ClientErrored += Client_ClientErrored;
                client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;

                var interactivity = client.UseInteractivity
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

                DependencyCollection dep;
                using (var d = new DependencyCollectionBuilder())
                {
                    d.AddInstance(new Dependencies(interactivity, _whm, _subProcessor, _whConfig, _lang, new StripeService(_whConfig.StripeApiKey)));
                    dep = d.Build();
                }

                var commands = client.UseCommandsNext
                (
                    new CommandsNextConfiguration
                    {
                        StringPrefix = server.CommandPrefix?.ToString(),
                        EnableDms = true,
                        EnableMentionPrefix = string.IsNullOrEmpty(server.CommandPrefix),
                        EnableDefaultHelp = false,
                        CaseSensitive = false,
                        IgnoreExtraArguments = true,
                        Dependencies = dep
                    }
                );
                commands.CommandExecuted += Commands_CommandExecuted;
                commands.CommandErrored += Commands_CommandErrored;
                commands.RegisterCommands<Owner>();
                commands.RegisterCommands<CommunityDay>();
                commands.RegisterCommands<Nests>();
                commands.RegisterCommands<ShinyStats>();
                commands.RegisterCommands<Gyms>();
                commands.RegisterCommands<Quests>();
                if (server.EnableSubscriptions)
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

                Task.Delay(3000).GetAwaiter().GetResult();
            }
        }

        #endregion

        #region Public Methods

        public async Task Start()
        {
            _logger.Trace("Start");
            _logger.Info("Connecting to Discord...");

            var keys = _servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var client = _servers[guildId];

                _logger.Info($"Attempting connection to Discord server {guildId}");
                await client.ConnectAsync();
                await Task.Delay(5000);
                await CreateEmojis(guildId);
            }

            _whm.PokemonAlarmTriggered += OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered += OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered += OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered += OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered += OnGymAlarmTriggered;
            _whm.GymDetailsAlarmTriggered += OnGymDetailsAlarmTriggered;
            _whm.WeatherAlarmTriggered += OnWeatherAlarmTriggered;
            if (_whConfig.Servers.FirstOrDefault(x => x.Value.EnableSubscriptions).Value != null) //At least one server wants subscriptions
            {
                _whm.PokemonSubscriptionTriggered += OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered += OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered += OnQuestSubscriptionTriggered;
                _whm.InvasionSubscriptionTriggered += OnInvasionSubscriptionTriggered;
            }

            _logger.Info("WebhookManager is running...");
        }

        public async Task Stop()
        {
            _logger.Trace("Stop");
            _logger.Info("Disconnecting from Discord...");

            var keys = _servers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var guildId = keys[i];
                var client = _servers[guildId];

                _logger.Info($"Attempting connection to Discord server {guildId}");
                await client.DisconnectAsync();
                await Task.Delay(5000);
            }

            _whm.PokemonAlarmTriggered -= OnPokemonAlarmTriggered;
            _whm.RaidAlarmTriggered -= OnRaidAlarmTriggered;
            _whm.QuestAlarmTriggered -= OnQuestAlarmTriggered;
            _whm.PokestopAlarmTriggered -= OnPokestopAlarmTriggered;
            _whm.GymAlarmTriggered -= OnGymAlarmTriggered;
            _whm.GymDetailsAlarmTriggered -= OnGymDetailsAlarmTriggered;
            _whm.WeatherAlarmTriggered -= OnWeatherAlarmTriggered;
            if (_whConfig.Servers.FirstOrDefault(x => x.Value.EnableSubscriptions).Value != null) //At least one server wants subscriptions
            {
                _whm.PokemonSubscriptionTriggered -= OnPokemonSubscriptionTriggered;
                _whm.RaidSubscriptionTriggered -= OnRaidSubscriptionTriggered;
                _whm.QuestSubscriptionTriggered -= OnQuestSubscriptionTriggered;
                _whm.InvasionSubscriptionTriggered -= OnInvasionSubscriptionTriggered;
            }

            _logger.Info("WebhookManager is stopped...");
        }

        #endregion

        #region Discord Events

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info($"[DISCORD] Connected.");
            _logger.Info($"[DISCORD] Current Application:");
            _logger.Info($"[DISCORD] Name: {e.Client.CurrentApplication.Name}");
            _logger.Info($"[DISCORD] Description: {e.Client.CurrentApplication.Description}");
            _logger.Info($"[DISCORD] Owner: {e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}");
            _logger.Info($"[DISCORD] Current User:");
            _logger.Info($"[DISCORD] Id: {e.Client.CurrentUser.Id}");
            _logger.Info($"[DISCORD] Name: {e.Client.CurrentUser.Username}#{e.Client.CurrentUser.Discriminator}");
            _logger.Info($"[DISCORD] Email: {e.Client.CurrentUser.Email}");

            await Task.CompletedTask;
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

                var prefix = _whConfig.Servers.ContainsKey(e.Context.Guild.Id) ? _whConfig.Servers[e.Context.Guild.Id].CommandPrefix : "!";
                var example = $"Command Example: ```{prefix}{e.Command.Name} {string.Join(" ", e.Command.Arguments.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name))}```\r\n*Parameters in brackets are optional.*";

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
                _logger.Error("[DISCORD] RATE LIMITED-----------------");
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
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Pokemon Found [Alarm: {e.Alarm.Name}, Pokemon: {e.Data.Id}, Despawn: {e.Data.DespawnTime}]");

            var pokemon = e.Data;
            var pkmn = Database.Instance.Pokemon[pokemon.Id];
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokemon.Latitude, pokemon.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[POKEMON] Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            try
            {
                var form = pokemon.Id.GetPokemonForm(pokemon.FormId.ToString());
                //var costume = e.Pokemon.Id.GetCostume(e.Pokemon.Costume.ToString());
                //var costumeFormatted = (string.IsNullOrEmpty(costume) ? "" : " " + costume);
                var pkmnImage = pokemon.Id.GetPokemonImage(_whConfig.Urls.PokemonImage, pokemon.Gender, pokemon.FormId, pokemon.Costume);

                var guildId = _whConfig.Servers.Values?.FirstOrDefault(x => x.CityRoles.Select(y => y.ToLower()).Contains(loc?.Name.ToLower()))?.GuildId ?? 0;
                if (!_servers.ContainsKey(guildId))
                    return;

                var client = _servers[guildId];
                var eb = await pokemon.GeneratePokemonMessage(guildId, client, _whConfig, pokemon, e.Alarm, loc.Name, pkmnImage);
                var name = $"{pkmn.Name}{pokemon.Gender.GetPokemonGenderIconValue()}{form}";
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = name,
                    AvatarUrl = pkmnImage,
                    Embeds = new List<DiscordEmbed> { eb }
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

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

        private void OnRaidAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<RaidData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Raid Found [Alarm: {e.Alarm.Name}, Raid: {e.Data.PokemonId}, Level: {e.Data.Level}, StartTime: {e.Data.StartTime}]");

            var raid = e.Data;
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(raid.Latitude, raid.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[RAID] Failed to lookup city from coordinates {raid.Latitude},{raid.Longitude} {pkmn.Name} {raid.Level}, skipping...");
                return;
            }

            try
            {
                var pkmn = Database.Instance.Pokemon[raid.PokemonId];
                var form = raid.PokemonId.GetPokemonForm(raid.Form.ToString());
                var pkmnImage = raid.IsEgg ? string.Format(_whConfig.Urls.EggImage, raid.Level) : raid.PokemonId.GetPokemonImage(_whConfig.Urls.PokemonImage, PokemonGender.Unset, raid.Form);

                var guildId = _whConfig.Servers.Values?.FirstOrDefault(x => x.CityRoles.Select(y => y.ToLower()).Contains(loc?.Name.ToLower()))?.GuildId ?? 0;
                if (!_servers.ContainsKey(guildId))
                    return;

                var client = _servers[guildId];
                var eb = raid.GenerateRaidMessage(guildId, client, _whConfig, e.Alarm, loc.Name, pkmnImage);
                var name = raid.IsEgg ? $"Level {raid.Level} {pkmn.Name}" : $"{(string.IsNullOrEmpty(form) ? null : form + "-")}{pkmn.Name} Raid";
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = name,
                    AvatarUrl = pkmnImage,
                    Embeds = new List<DiscordEmbed> { eb }
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

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

        private void OnQuestAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<QuestData> e)
        {
            if (string.IsNullOrEmpty(e.Alarm.Webhook))
                return;

            _logger.Info($"Quest Found [Alarm: {e.Alarm.Name}, PokestopId: {e.Data.PokestopId}, Type={e.Data.Type}]");

            var quest = e.Data;
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(quest.Latitude, quest.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[QUEST] Failed to lookup city for coordinates {quest.Latitude},{quest.Longitude}, skipping...");
                return;
            }

            try
            {
                var guildId = _whConfig.Servers.Values?.FirstOrDefault(x => x.CityRoles.Select(y => y.ToLower()).Contains(loc?.Name.ToLower()))?.GuildId ?? 0;
                if (!_servers.ContainsKey(guildId))
                    return;

                var client = _servers[guildId];
                var eb = quest.GenerateQuestMessage(guildId, client, _whConfig, e.Alarm, loc?.Name ?? e.Alarm.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = quest.GetQuestMessage(),
                    AvatarUrl = quest.GetIconUrl(_whConfig),
                    Embeds = new List<DiscordEmbed> { eb }
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

                Statistics.Instance.QuestsSent++;
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
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(pokestop.Latitude, pokestop.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"[POKESTOP] Failed to lookup city for coordinates {pokestop.Latitude},{pokestop.Longitude}, skipping...");
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
                var guildId = _whConfig.Servers.Values?.FirstOrDefault(x => x.CityRoles.Select(y => y.ToLower()).Contains(loc?.Name.ToLower()))?.GuildId ?? 0;
                if (!_servers.ContainsKey(guildId))
                    return;

                var client = _servers[guildId];
                var eb = pokestop.GeneratePokestopMessage(guildId, client, _whConfig, e.Alarm, loc?.Name ?? e.Alarm.Name);
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = pokestop.Name ?? "Unknown Pokestop",
                    AvatarUrl = icon,
                    Embeds = new List<DiscordEmbed> { eb }
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

                //Statistics.Instance.QuestsSent++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnGymAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<GymData> e)
        {
            //if (!_whm.WebHooks.ContainsKey(e.Alarm.Name))
            //    return;
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
            var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(gymDetails.Latitude, gymDetails.Longitude));
            if (loc == null)
            {
                //_logger.Warn($"Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
                return;
            }

            try
            {
                if (!_gyms.ContainsKey(gymDetails.GymId))
                {
                    _gyms.Add(gymDetails.GymId, gymDetails);
                }

                var oldGym = _gyms[gymDetails.GymId];
                var changed = oldGym.Team != gymDetails.Team;// || /*oldGym.InBattle != gymDetails.InBattle ||*/ gymDetails.InBattle;
                if (!changed)
                    return;

                var guildId = _whConfig.Servers.Values?.FirstOrDefault(x => x.CityRoles.Select(y => y.ToLower()).Contains(loc?.Name.ToLower()))?.GuildId ?? 0;
                if (!_servers.ContainsKey(guildId))
                    return;

                var client = _servers[guildId];
                var eb = gymDetails.GenerateGymMessage(guildId, client, _whConfig, e.Alarm, oldGym, loc?.Name ?? e.Alarm.Name);
                var name = gymDetails.GymName;
                var jsonEmbed = new DiscordWebhookMessage
                {
                    Username = name,
                    AvatarUrl = gymDetails.Url,
                    Embeds = new List<DiscordEmbed> { eb }
                }.Build();
                NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

                _gyms[gymDetails.GymId] = gymDetails;

                //Statistics.Instance.PokemonSent++;
                //Statistics.Instance.IncrementPokemonStats(pokemon.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnWeatherAlarmTriggered(object sender, AlarmEventTriggeredEventArgs<WeatherData> e)
        {
            //if (string.IsNullOrEmpty(e.Alarm.Webhook))
            //    return;

            //_logger.Info($"Weather Found [Alarm: {e.Alarm.Name}, S2CellId: {e.Data.Id}, Condition={e.Data.GameplayCondition}, Severity={e.Data.Severity}]");

            //var weather = e.Data;
            //var loc = _whm.GeofenceService.GetGeofence(e.Alarm.Geofences, new Location(weather.Latitude, weather.Longitude));
            //if (loc == null)
            //{
            //    //_logger.Warn($"Failed to lookup city from coordinates {pokemon.Latitude},{pokemon.Longitude} {pkmn.Name} {pokemon.IV}, skipping...");
            //    return;
            //}

            //try
            //{
            //    if (!_gyms.ContainsKey(weather.Id))
            //    {
            //        _gyms.Add(weather.Id, weather);
            //    }

            //    var oldGym = _gyms[weather.Id];
            //    var changed = oldGym.Team != weather.Team;// || /*oldGym.InBattle != gymDetails.InBattle ||*/ gymDetails.InBattle;
            //    if (!changed)
            //        return;

            //    var eb = weather.GenerateGymMessage(_client, _whConfig, e.Alarm, oldGym, loc?.Name ?? e.Alarm.Name);
            //    var name = weather.GymName;
            //    var jsonEmbed = new DiscordWebhookMessage
            //    {
            //        Username = name,
            //        AvatarUrl = weather.Url,
            //        Embeds = new List<DiscordEmbed> { eb }
            //    }.Build();
            //    NetUtil.SendWebhook(e.Alarm.Webhook, jsonEmbed);

            //    _gyms[weather.Id] = weather;

            //    //Statistics.Instance.PokemonSent++;
            //    //Statistics.Instance.IncrementPokemonStats(pokemon.Id);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //}
        }

        #endregion

        #region Subscription Events

        private void OnPokemonSubscriptionTriggered(object sender, PokemonData e)
        {
            //if (!_whConfig.Discord.EnableSubscriptions)
            //    return;

            if (_subProcessor == null)
                return;

            new System.Threading.Thread(async () => await _subProcessor.ProcessPokemonSubscription(e)) { IsBackground = true }.Start();
        }

        private void OnRaidSubscriptionTriggered(object sender, RaidData e)
        {
            //if (!_whConfig.Discord.EnableSubscriptions)
            //    return;

            if (_subProcessor == null)
                return;

            new System.Threading.Thread(async () => await _subProcessor.ProcessRaidSubscription(e)) { IsBackground = true }.Start();
        }

        private void OnQuestSubscriptionTriggered(object sender, QuestData e)
        {
            //if (!_whConfig.Discord.EnableSubscriptions)
            //    return;

            if (_subProcessor == null)
                return;

            new System.Threading.Thread(async () => await _subProcessor.ProcessQuestSubscription(e)) { IsBackground = true }.Start();
        }

        private void OnInvasionSubscriptionTriggered(object sender, PokestopData e)
        {
            //if (!_whConfig.Discord.EnableSubscriptions)
            //    return;

            if (_subProcessor == null)
                return;

            new System.Threading.Thread(async () => await _subProcessor.ProcessInvasionSubscription(e)) { IsBackground = true }.Start();
        }

        #endregion

        #region Private Methods

        private async Task CreateEmojis(ulong guildId)
        {
            _logger.Trace($"CreateEmojis");

            var server = _whConfig.Servers[guildId];
            var client = _servers[guildId];
            if (!client.Guilds?.ContainsKey(server.EmojiGuildId) ?? false)
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
                            _logger.Error($"Failed to file emoji file at {emojiPath}, skipping...");
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
        }

        private async Task ResetQuests()
        {
            _logger.Debug($"MIDNIGHT {DateTime.Now}");
            _logger.Debug($"Starting automatic quest messages cleanup...");

            Statistics.Instance.WriteOut();
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
                if (!server.ShinyStats.Enabled)
                {
                    _logger.Debug($"Shiny stats not enabled for guild with id {guildId}.");
                    continue;
                }
                    var statsChannel = await client.GetChannelAsync(server.ShinyStats.ChannelId);
                if (statsChannel == null)
                {
                    _logger.Warn($"Failed to get channel id {server.ShinyStats.ChannelId} to post shiny stats.");
                }
                else
                {
                    if (server.ShinyStats.ClearMessages)
                    {
                        _logger.Debug($"Deleting previous shiny stats messages in channel {server.ShinyStats.ChannelId}");
                        await client.DeleteMessages(server.ShinyStats.ChannelId);
                    }

                    _logger.Debug($"Posting shiny stats for guild {client.Guilds[guildId].Name} ({guildId}) in channel {server.ShinyStats.ChannelId}");
                    //Subtract an hour to make sure it shows yesterdays date.
                    await statsChannel.SendMessageAsync($"[**Shiny Pokemon stats for {DateTime.Now.Subtract(TimeSpan.FromHours(1)).ToLongDateString()}**]\r\n----------------------------------------------");
                    var stats = await ShinyStats.GetShinyStats(_whConfig.Database.Scanner.ToString());
                    var sorted = stats.Keys.ToList();
                    sorted.Sort();

                    foreach (var pokemon in sorted)
                    {
                        if (pokemon == 0)
                            continue;

                        if (!Database.Instance.Pokemon.ContainsKey((int)pokemon))
                            continue;

                        var pkmn = Database.Instance.Pokemon[(int)pokemon];
                        var pkmnStats = stats[pokemon];
                        var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                        var chanceMessage = chance == 0 ? null : $" with a **1/{chance}** ratio";
                        await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**  |  **{pkmnStats.Shiny.ToString("N0")}** shiny out of **{pkmnStats.Total.ToString("N0")}** total seen in the last 24 hours{chanceMessage}.");
                    }

                    var total = stats[0];
                    var ratio = total.Shiny == 0 || total.Total == 0 ? null : $" with a **1/{Convert.ToInt32(total.Total / total.Shiny)}** ratio in total";
                    await statsChannel.SendMessageAsync($"Found **{total.Shiny.ToString("N0")}** total shinies out of **{total.Total.ToString("N0")}** possiblities{ratio}.");
                }

                var channelIds = server.QuestChannelIds;
                _logger.Debug($"Quest channel pruning started for {channelIds.Count.ToString("N0")} channels...");
                for (var j = 0; j < channelIds.Count; j++)
                {
                    var item = await client.DeleteMessages(channelIds[j]);
                    _logger.Debug($"Deleted all {item.Item2.ToString("N0")} quest messages from channel {item.Item1.Name}.");
                }

                _logger.Debug($"Finished automatic quest messages cleanup...");
            }

            CleanupDepartedMembers();
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
                if (!server.EnableSubscriptions)
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
                        if (!_subProcessor.Manager.RemoveAllUserSubscriptions(user.GuildId, user.UserId))
                        {
                            _logger.Warn($"Could not remove user {user.UserId} subscription settings from the database.");
                        }
                    }
                }
            }
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
                        _logger.Error($"Failed to find guild id {guildId} in server config list.");
                        continue;
                    }
                    var server = _whConfig.Servers[guildId];

                    if (!_servers.ContainsKey(guildId))
                    {
                        _logger.Error($"Failed to find guild id {guildId} in Discord server client list.");
                        continue;
                    }
                    var client = _servers[guildId];
                    if (client != null)
                    {
                        var owner = await client.GetUserAsync(server.OwnerId);
                        if (owner == null)
                        {
                            _logger.Warn($"Failed to get owner from id {server.OwnerId}.");
                            return;
                        }

                        await client.SendDirectMessage(owner, Strings.CrashMessage, null);
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