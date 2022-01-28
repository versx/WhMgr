namespace WhMgr.Services.Discord
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Osm;
    using WhMgr.Services.Subscriptions;

    // TODO: Convert to HostedService
    public class DiscordClientService : IDiscordClientService
    {
        public const uint DiscordAccessValidationInterval = 5 * 60000; // Every 5 minutes

        private readonly ILogger<IDiscordClientService> _logger;
        private readonly Dictionary<ulong, DiscordClient> _discordClients;
        private readonly ConfigHolder _config;
        private readonly ISubscriptionManagerService _subscriptionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Timers.Timer _accessValidator;

        public IReadOnlyDictionary<ulong, DiscordClient> DiscordClients =>
            _discordClients;

        public bool Initialized { get; private set; }

        public DiscordClientService(
            ILogger<IDiscordClientService> logger,
            ConfigHolder config,
            ISubscriptionManagerService subscriptionManager,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _subscriptionManager = subscriptionManager;
            _serviceProvider = serviceProvider;

            _discordClients = new Dictionary<ulong, DiscordClient>();
            _accessValidator = new System.Timers.Timer(DiscordAccessValidationInterval);
            _accessValidator.Elapsed += async (sender, e) => await ValidateDiscordMemberAccess();
        }

        #region Public Methods

        public async Task Start()
        {
            _logger.LogTrace($"Initializing Discord clients...");

            // Build the dependency collection which will contain our objects that can be globally used within each command module
            var servicesCol = new ServiceCollection()
                .AddSingleton(typeof(ConfigHolder), _config)
                .AddSingleton(typeof(OsmManager), new OsmManager())
                .AddSingleton(typeof(IServiceProvider), _serviceProvider)
                .AddSingleton(LoggerFactory.Create(configure => configure.AddConsole()));
            var services = servicesCol.BuildServiceProvider();
            await InitializeDiscordClients(services);

            // Start validating Discord member access
            _accessValidator.Start();
        }

        public async Task Stop()
        {
            _logger.LogTrace($"Stopping Discord clients...");

            foreach (var (guildId, discordClient) in _discordClients)
            {
                await discordClient.DisconnectAsync();
                _logger.LogDebug($"Discord client for guild {guildId} disconnected.");
            }
        }

        #endregion

        private async Task InitializeDiscordClients(ServiceProvider services)
        {
            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                _logger.LogDebug($"Configured Discord server {guildId}");
                var client = DiscordClientFactory.CreateDiscordClient(guildConfig, services);
                client.Ready += Client_Ready;
                client.GuildAvailable += Client_GuildAvailable;
                if ((guildConfig.GeofenceRoles?.Enabled ?? false) &&
                    (guildConfig.GeofenceRoles?.AutoRemove ?? false))
                {
                    client.GuildMemberUpdated += Client_GuildMemberUpdated;
                }
                //client.MessageCreated += Client_MessageCreated;
                client.ClientErrored += Client_ClientErrored;

                if (!_discordClients.ContainsKey(guildId))
                {
                    _discordClients.Add(guildId, client);
                    await client.ConnectAsync();
                    _logger.LogDebug($"Discord client for guild {guildId} connecting...");
                }

                // Wait 3 seconds between initializing each Discord client
                await Task.Delay(3 * 1000);
            }

            _logger.LogInformation($"Discord clients all initialized");
            Initialized = true;
        }

        private async Task ValidateDiscordMemberAccess()
        {
            // Check if any Discord clients configured
            if (_discordClients.Count == 0)
                return;

            // Build list of role ids from dictionary of guilds
            var validRoleIdsPerGuild = _config.Instance.Servers.Values
                                                               .ToList()
                                                               .Aggregate(new List<ulong>(), (x, y) => x.Concat(y.DonorRoleIds.Keys.ToList())
                                                                                                        .ToList());
            // Check if subscriptions fetched yet from database
            var subscriptions = _subscriptionManager.Subscriptions;
            if (subscriptions == null)
                return;

            // Loop all available subscriptions
            foreach (var subscription in subscriptions)
            {
                if (!_discordClients.Any(x => x.Value.Guilds.ContainsKey(subscription.GuildId)))
                    continue;

                // Check if configured Discord clients configured with subscription Discord guild
                var discordClient = _discordClients.FirstOrDefault(x => x.Value.Guilds.ContainsKey(subscription.GuildId));
                if (discordClient.Value == null)
                    continue;

                // Get guild for subscription
                var guild = discordClient.Value.Guilds[subscription.GuildId];
                if (guild == null)
                    continue;

                // Get member for subscriptions
                var member = await guild.GetMemberAsync(subscription.UserId);
                if (member == null)
                    continue;

                // Check if guild configured
                if (!_config.Instance.Servers.ContainsKey(guild.Id))
                    continue;

                // Get members existing role ids
                var memberRoleIds = member.Roles.Select(x => x.Id).ToList();

                // Check if member roles contains any of the valid roles for the guild
                var isValid = validRoleIdsPerGuild.Exists(x => memberRoleIds.Contains(x));
                if (!isValid)
                {
                    // Disable all subscriptions
                    await _subscriptionManager.SetSubscriptionStatusAsync(subscription, Subscriptions.Models.NotificationStatusType.None);
                    _logger.LogInformation($"Disabled all subscriptions for user {member.Username} ({member.Id})...");
                }
            }
        }

        #region Discord Events

        private Task Client_Ready(DiscordClient client, ReadyEventArgs e)
        {
            _logger.LogInformation($"------------------------------------------");
            _logger.LogInformation($"[DISCORD] Connected.");
            _logger.LogInformation($"[DISCORD] ----- Current Application");
            _logger.LogInformation($"[DISCORD] Name: {client.CurrentApplication.Name}");
            _logger.LogInformation($"[DISCORD] Description: {client.CurrentApplication.Description}");
            var owners = string.Join(", ", client.CurrentApplication.Owners.Select(x => $"{x.Username}#{x.Discriminator}"));
            _logger.LogInformation($"[DISCORD] Owner: {owners}");
            _logger.LogInformation($"[DISCORD] ----- Current User");
            _logger.LogInformation($"[DISCORD] Id: {client.CurrentUser.Id}");
            _logger.LogInformation($"[DISCORD] Name: {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
            _logger.LogInformation($"[DISCORD] Email: {client.CurrentUser.Email}");
            _logger.LogInformation($"------------------------------------------");

            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            // If guild is in configured servers list then attempt to create emojis needed
            if (!_config.Instance.Servers.ContainsKey(e.Guild.Id))
                return;

            // Create default emojis
            await CreateEmojisAsync(e.Guild.Id);

            // Set custom bot status if guild is in config server list
            var status = _config.Instance.Servers[e.Guild.Id].Bot?.Status ?? $"v{Strings.BotVersion}";
            await client.UpdateStatusAsync(new DiscordActivity(status, ActivityType.Playing), UserStatus.Online);
        }

        private async Task Client_GuildMemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs e)
        {
            if (!_config.Instance.Servers.ContainsKey(e.Guild.Id))
                return;

            var server = _config.Instance.Servers[e.Guild.Id];
            var donorRoleIds = server.DonorRoleIds.Keys.ToList();
            var hasBefore = e.RolesBefore.FirstOrDefault(x => donorRoleIds.Contains(x.Id)) != null;
            var hasAfter = e.RolesAfter.FirstOrDefault(x => donorRoleIds.Contains(x.Id)) != null;

            // Check if donor role was removed
            if (hasBefore && !hasAfter)
            {
                _logger.LogInformation($"Member {e.Member.Username} ({e.Member.Id}) donor role removed, removing any city roles...");
                // If so, remove all city/geofence/area roles
                var areaRoles = server.Geofences.Select(x => x.Name.ToLower());
                foreach (var roleName in areaRoles)
                {
                    var role = e.Guild.Roles.FirstOrDefault(x => x.Value.Name == roleName).Value;
                    if (role == null)
                    {
                        _logger.LogError($"Failed to get role by name {roleName}");
                        continue;
                    }
                    await e.Member.RevokeRoleAsync(role, "No longer a supporter/donor");
                }
                _logger.LogInformation($"All city roles removed from member {e.Member.Username} ({e.Member.Id})");
            }
            else if (!hasBefore && hasAfter)
            {
                // Send thanks for becoming a donor message and include instructions to setting up subscriptions
                _logger.LogInformation($"Member {e.Member.Username} ({e.Member.Id}) donor role added...");
                var eb = new DiscordEmbedBuilder
                {
                    // TODO: Localize
                    Title = $"Welcome to {e.Guild?.Name} {e.Member.Username}#{e.Member.Discriminator}!",
                    Description = $"Thank you for joining {e.Guild?.Name}! Please look around and get familar, you can get " +
                        "exclusive access to Pokemon, PvP, Raids, Quests, Invasions, Lures, and Gyms by typing `$donate` in the #bot " +
                        "channel then following the upgrade link.\n\n" +
                        $"To see different city sections type `{server.Bot?.CommandPrefix}feedme city1,city2` in the #bot channel.\n" +
                        $"Type `{server.Bot?.CommandPrefix}help for more information.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{e.Guild?.Name} {DateTime.Now}",
                        IconUrl = e.Guild?.IconUrl,
                    },
                };
                await e.Member.SendMessageAsync(eb.Build());
            }
        }

        /*
        private async Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            // Ignore bot messages
            if (e.Author.Id == client.CurrentUser.Id || e.Author.IsBot)
                return;

            // Bot not configured for guild
            if (!_config.Instance.Servers.ContainsKey(e.Guild?.Id ?? 0))
                return;

            var guildConfig = _config.Instance.Servers[e.Guild.Id];
            if (guildConfig.Bot.ChannelIds.Count > 0 && !guildConfig.Bot.ChannelIds.Contains(e.Channel.Id))
                return;

            await Task.CompletedTask;
        }
        */

        private async Task Client_ClientErrored(DiscordClient client, ClientErrorEventArgs e)
        {
            _logger.LogError(e.Exception.ToString());

            await Task.CompletedTask;
        }

        #endregion

        #region Discord Emojis

        // TODO: Fix race condition
        private async Task CreateEmojisAsync(ulong guildId)
        {
            if (!_discordClients.ContainsKey(guildId))
            {
                _logger.LogWarning($"Discord client not ready yet to create emojis for guild {guildId}");
                return;
            }

            var server = _config.Instance.Servers[guildId];
            var client = _discordClients[guildId];
            // Check if bot guilds contains emoji guild id
            if (!(client.Guilds?.ContainsKey(server.Bot.EmojiGuildId) ?? false))
            {
                _logger.LogWarning($"Bot not in emoji server {server.Bot.EmojiGuildId}");
                return;
            }

            var guild = client.Guilds[server.Bot.EmojiGuildId];
            foreach (var emoji in Strings.Defaults.EmojisList)
            {
                try
                {
                    // Fetch all guild emojis
                    var emojis = await guild.GetEmojisAsync();

                    // Get emoji from available guild emojis by name
                    var emojiExists = emojis.FirstOrDefault(x => string.Compare(x.Name, emoji, true) == 0);

                    // Check if emoji exists, if so skip
                    if (emojiExists != null)
                        continue;

                    _logger.LogDebug($"Emoji {emoji} doesn't exist, creating...");

                    // Check if emoji exists at path to upload to Discord
                    var emojiPath = Path.Combine(Strings.EmojisFolder, emoji + ".png");
                    if (!File.Exists(emojiPath))
                    {
                        _logger.LogWarning($"Unable to find emoji file at {emojiPath}, skipping...");
                        continue;
                    }

                    // Create steam of emoji file data
                    var fs = new FileStream(emojiPath, FileMode.Open, FileAccess.Read);

                    // Create emoji for Discord guild
                    await guild.CreateEmojiAsync(emoji, fs, null, $"Missing `{emoji}` emoji.");

                    _logger.LogInformation($"Emoji {emoji} created successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            await CacheGuildEmojisListAsync();
        }

        private async Task CacheGuildEmojisListAsync()
        {
            foreach (var (guildId, serverConfig) in _config.Instance.Servers)
            {
                var emojiGuildId = serverConfig.Bot.EmojiGuildId;
                if (!_discordClients.ContainsKey(guildId))
                    continue;

                var guild = _discordClients[guildId];
                if (!guild.Guilds.ContainsKey(emojiGuildId))
                    continue;

                var emojiGuild = guild.Guilds[emojiGuildId];
                var emojis = await emojiGuild.GetEmojisAsync();
                foreach (var name in Strings.Defaults.EmojisList)
                {
                    var emoji = emojis.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0);
                    if (emoji == null)
                        continue;

                    if (!GameMaster.Instance.Emojis.ContainsKey(emoji.Name))
                    {
                        GameMaster.Instance.Emojis.Add(emoji.Name, emoji.Id);
                    }
                }
            }

            _logger.LogDebug($"Cached {GameMaster.Instance.Emojis.Count:N0} emojis:");
            foreach (var (emojiName, emojiId) in GameMaster.Instance.Emojis)
            {
                _logger.LogDebug($"- {emojiName} ({emojiId})");
            }

            _logger.LogDebug($"Emojis overwritten by custom unicode emojis:");
            foreach (var (emojiName, emojiUnicode) in GameMaster.Instance.CustomEmojis)
            {
                if (string.IsNullOrEmpty(emojiUnicode))
                    continue;

                _logger.LogDebug($"- {emojiName} ({emojiUnicode})");
            }
        }

        #endregion
    }
}