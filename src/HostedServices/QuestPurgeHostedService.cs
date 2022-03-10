namespace WhMgr.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;
    //using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Discord;

    public class QuestPurgeHostedService : IHostedService, IDisposable
    {
        #region Variables

        private readonly Microsoft.Extensions.Logging.ILogger<QuestPurgeHostedService> _logger;
        private readonly ConfigHolder _config;
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;
        private readonly IDiscordClientService _discordService;

        #endregion

        #region Constructor

        public QuestPurgeHostedService(
            Microsoft.Extensions.Logging.ILogger<QuestPurgeHostedService> logger,
            ConfigHolder config,
            IDiscordClientService discordService)
        {
            _logger = logger;
            _config = config;
            _discordService = discordService;
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
        }

        #endregion

        #region Public Interface Implementation

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Hosted service started...");

            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                // Check if quest purge is enabled for guild
                if (!(guildConfig.QuestsPurge?.Enabled ?? false))
                    continue;

                // If so, loop available channels
                foreach (var (timezone, questChannelIds) in guildConfig.QuestsPurge.ChannelIds)
                {
                    // Check if duplicate timezone exists
                    if (_tzMidnightTimers.ContainsKey(timezone))
                    {
                        _logger.Warning($"Midnight timer already configured for timezone '{timezone}'");
                        continue;
                    }

                    // Create and start midnight timer for timezone
                    var midnightTimer = new MidnightTimer(0, timezone);
                    midnightTimer.TimeReached += OnMidnightTimerTimeReached;
                    midnightTimer.Start();

                    // Add to cache list
                    _tzMidnightTimers.Add(timezone, midnightTimer);
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Hosted service stopped...");
            foreach (var (timezone, midnightTimer) in _tzMidnightTimers)
            {
                _logger.Information($"Stopping midnight timer for timezone {timezone}");
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        public async void Dispose()
        {
            _logger.Debug($"Disposing...");
            await _discordService.Stop();
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private async void OnMidnightTimerTimeReached(DateTime time, string timezone)
        {
            _logger.Information($"Midnight timer hit {time} for timezone {timezone}");
            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                if (!(guildConfig.QuestsPurge?.Enabled ?? false))
                    continue;

                if (!(guildConfig.QuestsPurge?.ChannelIds.ContainsKey(timezone) ?? false))
                    continue;

                var channelIds = guildConfig.QuestsPurge.ChannelIds[timezone];
                _logger.Information($"Clearing quest channels {string.Join(", ", channelIds)} for guild {guildId}");
                await ClearQuestChannels(channelIds);
            }
        }

        private async Task ClearQuestChannels(List<ulong> channelIds)
        {
            if (channelIds?.Count == 0)
            {
                _logger.Warning($"Clear quest channels list was empty");
                return;
            }

            // Loop all specified channel ids
            foreach (var channelId in channelIds)
            {
                // Loop all provided Discord clients
                foreach (var (serverId, serverClient) in _discordService.DiscordClients)
                {
                    try
                    {
                        // Get Discord channel if available
                        _logger.Information($"Deleting messages in channel {channelId}");
                        var channel = await serverClient.GetChannelAsync(channelId).ConfigureAwait(false);
                        if (channel == null)
                            continue;

                        // Delete all Discord channel messages
                        _logger.Information($"Deleting messages for channel: {channelId} (GuildId: {serverId})");
                        await serverClient.DeleteMessagesAsync(channelId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to delete messages in channel {channelId}: {ex}");
                    }
                }
            }
            _logger.Information($"Completed deleting messages for channel(s) {string.Join(", ", channelIds)}");
        }

        #endregion
    }
}