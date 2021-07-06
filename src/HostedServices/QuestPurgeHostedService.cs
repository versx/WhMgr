namespace WhMgr.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;

    public class QuestPurgeHostedService : IHostedService, IDisposable
    {
        #region Variables

        private readonly ILogger<QuestPurgeHostedService> _logger;
        private readonly ConfigHolder _config;
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;
        private readonly Dictionary<ulong, DiscordClient> _discordClients;

        #endregion

        #region Constructor

        public QuestPurgeHostedService(
            ILogger<QuestPurgeHostedService> logger,
            ConfigHolder config,
            Dictionary<ulong, DiscordClient> discordClients)
        {
            _logger = logger;
            _config = config;
            _discordClients = discordClients;
        }

        #endregion

        #region Public Interface Implementation

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Hosted service started...");

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
                        _logger.LogWarning($"Midnight timer already configured for timezone '{timezone}'");
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
            _logger.LogInformation($"Hosted service stopped...");
            foreach (var (timezone, midnightTimer) in _tzMidnightTimers)
            {
                _logger.LogInformation($"Stopping midnight timer for timezone {timezone}");
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug($"Disposing...");
            _discordClients.Clear();
            _tzMidnightTimers.Clear();
        }

        #endregion

        #region Private Methods

        private async void OnMidnightTimerTimeReached(DateTime time, string timezone)
        {
            _logger.LogInformation($"Midnight timer hit {time} for timezone {timezone}");
            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                if (!(guildConfig.QuestsPurge?.ChannelIds.ContainsKey(timezone) ?? false))
                    continue;

                var channelIds = guildConfig.QuestsPurge.ChannelIds[timezone];
                _logger.LogInformation($"Clearing quest channels {string.Join(", ", channelIds)} for guild {guildId}");
                await ClearQuestChannels(channelIds);
            }
        }

        private async Task ClearQuestChannels(List<ulong> channelIds)
        {
            if (channelIds?.Count == 0)
            {
                _logger.LogWarning($"Clear quest channels list was empty");
                return;
            }

            // Loop all specified channel ids
            foreach (var channelId in channelIds)
            {
                // Loop all provided Discord clients
                foreach (var (serverId, serverClient) in _discordClients)
                {
                    // Get Discord channel if available
                    _logger.LogInformation($"Deleting messages in channel {channelId}");
                    var channel = await serverClient.GetChannelAsync(channelId);
                    if (channel == null)
                        continue;

                    // Delete all Discord channel messages
                    _logger.LogInformation($"Deleting messages for channel: {channelId} (GuildId: {serverId})");
                    await serverClient.DeleteMessages(channelId);
                }
            }
            _logger.LogInformation($"Completed deleting channel messages for channel(s) {string.Join(", ", channelIds)}");
            await Task.CompletedTask;
        }

        #endregion
    }
}