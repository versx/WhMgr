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

    public class DiscordClientService : IDiscordClientService
    {
        private readonly ILogger<IDiscordClientService> _logger;
        private readonly Dictionary<ulong, DiscordClient> _discordClients;
        private readonly ConfigHolder _config;
        private readonly IServiceProvider _serviceProvider;

        public IReadOnlyDictionary<ulong, DiscordClient> DiscordClients =>
            _discordClients;

        public DiscordClientService(
            ILogger<IDiscordClientService> logger,
            ConfigHolder config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;

            _discordClients = new Dictionary<ulong, DiscordClient>();
        }

        #region Public Methods

        public async Task Start()
        {
            _logger.LogTrace($"Initializing Discord clients...");

            // Build the dependency collection which will contain our objects that can be globally used within each command module
            var servicesCol = new ServiceCollection()
                .AddSingleton(typeof(ConfigHolder), _config)
                .AddSingleton(typeof(Osm.OsmManager), new Osm.OsmManager())
                .AddSingleton(typeof(IServiceProvider), _serviceProvider)
                //.AddSingleton(typeof(ISubscriptionManagerService), new SubscriptionManagerService(null, null));
                .AddSingleton<ILoggerFactory>(LoggerFactory.Create(configure => configure.AddConsole()));
            var services = servicesCol.BuildServiceProvider();
            await InitializeDiscord(services);
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

        private async Task InitializeDiscord(ServiceProvider services)
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
                await Task.Delay(3000);
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
            if (_config.Instance.Servers.ContainsKey(e.Guild.Id))
            {
                // Create default emojis
                await CreateEmojis(e.Guild.Id);

                // Set custom bot status if guild is in config server list
                var status = _config.Instance.Servers[e.Guild.Id].Bot?.Status ?? $"v{Strings.BotVersion}";
                await client.UpdateStatusAsync(new DiscordActivity(status, ActivityType.Playing), UserStatus.Online);
            }
        }

        private async Task Client_GuildMemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs e)
        {
            if (!_config.Instance.Servers.ContainsKey(e.Guild.Id))
                return;

            var server = _config.Instance.Servers[e.Guild.Id];
            var hasBefore = e.RolesBefore.FirstOrDefault(x => server.DonorRoleIds.Contains(x.Id)) != null;
            var hasAfter = e.RolesAfter.FirstOrDefault(x => server.DonorRoleIds.Contains(x.Id)) != null;

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

        private async Task CreateEmojis(ulong guildId)
        {
            if (!_discordClients.ContainsKey(guildId))
            {
                _logger.LogWarning($"Discord client not ready yet to create emojis for guild {guildId}");
                return;
            }

            var server = _config.Instance.Servers[guildId];
            var client = _discordClients[guildId];
            if (!(client.Guilds?.ContainsKey(server.Bot.EmojiGuildId) ?? false))
            {
                _logger.LogWarning($"Bot not in emoji server {server.Bot.EmojiGuildId}");
                return;
            }

            var guild = client.Guilds[server.Bot.EmojiGuildId];
            foreach (var emoji in Strings.EmojiList)
            {
                try
                {
                    var emojis = await guild.GetEmojisAsync();
                    var emojiExists = emojis.FirstOrDefault(x => string.Compare(x.Name, emoji, true) == 0);
                    if (emojiExists == null)
                    {
                        _logger.LogDebug($"Emoji {emoji} doesn't exist, creating...");

                        var emojiPath = Path.Combine(Strings.EmojisFolder, emoji + ".png");
                        if (!File.Exists(emojiPath))
                        {
                            _logger.LogWarning($"Unable to find emoji file at {emojiPath}, skipping...");
                            continue;
                        }

                        var fs = new FileStream(emojiPath, FileMode.Open, FileAccess.Read);
                        await guild.CreateEmojiAsync(emoji, fs, null, $"Missing `{emoji}` emoji.");

                        _logger.LogInformation($"Emoji {emoji} created successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            await CacheGuildEmojisList();
        }

        private async Task CacheGuildEmojisList()
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
                foreach (var name in Strings.EmojiList)
                {
                    var emoji = emojis.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0);
                    if (emoji == null)
                        continue;

                    if (!MasterFile.Instance.Emojis.ContainsKey(emoji.Name))
                    {
                        MasterFile.Instance.Emojis.Add(emoji.Name, emoji.Id);
                    }
                }
            }
        }

        #endregion
    }
}