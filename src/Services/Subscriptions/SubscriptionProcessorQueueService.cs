namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Queues;
    using WhMgr.Services.Subscriptions.Models;

    // TODO: Set queue as singleton that hosted subscription processor service handles
    // TODO: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio

    public class SubscriptionProcessorQueueService : ISubscriptionProcessorQueueService
    {
        private readonly ILogger<ISubscriptionProcessorQueueService> _logger;
        private readonly ConfigHolder _config;
        private readonly NotificationQueue _queue;
        private readonly IReadOnlyDictionary<ulong, DiscordClient> _discordClients;

        public SubscriptionProcessorQueueService(
            ILogger<ISubscriptionProcessorQueueService> logger,
            ConfigHolder config,
            IReadOnlyDictionary<ulong, DiscordClient> discordClients)
        {
            _logger = logger;
            _config = config;
            _discordClients = discordClients;
            // TODO: Make notification queue DI singleton
            _queue = new NotificationQueue();

            // Start queue processor
            ProcessQueue();
        }

        public void Add(NotificationItem item)
        {
            if (!_queue.Contains(item))
            {
                _queue.Enqueue(item);
            }
        }

        private void ProcessQueue()
        {
            _logger.LogTrace($"SubscriptionProcessor::ProcessQueue");

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
                        _logger.LogWarning($"Subscription queue is {_queue.Count:N0} items long.");
                    }

                    var item = _queue.Dequeue();
                    if (item == null || item?.Subscription == null || item?.Member == null || item?.Embed == null)
                        continue;

                    // Check if user is receiving messages too fast.
                    var maxNotificationsPerMinute = _config.Instance.MaxNotificationsPerMinute;
                    if (item.Subscription.Limiter.IsLimited(maxNotificationsPerMinute))
                    {
                        _logger.LogWarning($"{item.Member.Username} notifications rate limited, waiting {(60 - item.Subscription.Limiter.TimeLeft.TotalSeconds)} seconds...", item.Subscription.Limiter.TimeLeft.TotalSeconds.ToString("N0"));
                        // Send ratelimited notification to user if not already sent to adjust subscription settings to more reasonable settings.
                        if (!item.Subscription.RateLimitNotificationSent)
                        {
                            if (!_discordClients.ContainsKey(item.Subscription.GuildId))
                                continue;

                            var server = _discordClients[item.Subscription.GuildId].Guilds[item.Subscription.GuildId];
                            var emoji = DiscordEmoji.FromName(_discordClients.FirstOrDefault().Value, ":no_entry:");
                            var guildIconUrl = _discordClients.ContainsKey(item.Subscription.GuildId) ? _discordClients[item.Subscription.GuildId].Guilds[item.Subscription.GuildId]?.IconUrl : string.Empty;
                            // TODO: Localize
                            var rateLimitMessage = $"{emoji} Your notification subscriptions have exceeded {maxNotificationsPerMinute:N0}) per minute and are now being rate limited." +
                                                   $"Please adjust your subscriptions to receive a maximum of {maxNotificationsPerMinute:N0} notifications within a {NotificationLimiter.ThresholdTimeout} second time span.";
                            var eb = new DiscordEmbedBuilder
                            {
                                Title = "Rate Limited",
                                Description = rateLimitMessage,
                                Color = DiscordColor.Red,
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"{server?.Name} | {DateTime.Now}",
                                    IconUrl = server?.IconUrl
                                }
                            };

                            await item.Member.SendDirectMessage(eb.Build());
                            item.Subscription.RateLimitNotificationSent = true;
                            item.Subscription.Status = NotificationStatusType.None;
                            // TODO: Update database, set status to 0 via ISubscriptionManager
                            /*
                            if (!item.Subscription.Update())
                            {
                                _logger.LogError($"Failed to disable {item.Subscription.UserId}'s subscriptions");
                            }
                            */
                        }
                        continue;
                    }

                    // Ratelimit is up, allow for ratelimiting again
                    item.Subscription.RateLimitNotificationSent = false;

                    if (!_discordClients.ContainsKey(item.Subscription.GuildId))
                    {
                        _logger.LogError($"User subscription for guild that's not configured. UserId={item.Subscription.UserId} GuildId={item.Subscription.GuildId}");
                        continue;
                    }

                    // Send text message notification to user if a phone number is set
                    /* TODO: Twilio notifications
                    if (_config.Twilio.Enabled && !string.IsNullOrEmpty(item.Subscription.PhoneNumber))
                    {
                        // Check if user is in the allowed text message list or server owner
                        if (HasRole(item.Member, _config.Instance.Twilio.RoleIds) ||
                            _config.Instance.Twilio.UserIds.Contains(item.Member.Id) ||
                            _config.Instance.Servers[item.Subscription.GuildId].OwnerId == item.Member.Id)
                        {
                            // Send text message (max 160 characters)
                            if (item.Pokemon != null && IsUltraRare(_config.Instance.Twilio, item.Pokemon))
                            {
                                var result = Utils.SendSmsMessage(StripEmbed(item), _config.Instance.Twilio, item.Subscription.PhoneNumber);
                                if (!result)
                                {
                                    _logger.LogError($"Failed to send text message to phone number '{item.Subscription.PhoneNumber}' for user {item.Subscription.UserId}");
                                }
                            }
                        }
                    }
                    */

                    // Send direct message notification to user
                    var client = _discordClients[item.Subscription.GuildId];
                    await item.Member.SendDirectMessage(string.Empty, item.Embed);
                    _logger.LogInformation($"[WEBHOOK] Notified user {item.Member.Username} of {item.Description}.");
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}