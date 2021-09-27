namespace WhMgr.Commands.Discord
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    //using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Localization;

    public class Quests : BaseCommandModule
    {
        private readonly ConfigHolder _config;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Quests(
            ConfigHolder config,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger(typeof(Quests).FullName);
        }

        [
             Command("reset-quests"),
             Hidden,
             RequirePermissions(Permissions.KickMembers)
         ]
        public async Task ResetChannelAsync(CommandContext ctx,
            [Description("Discord channel to reset.")] DiscordChannel channel = null)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x));
            if (guildId == 0)
            {
                _logger.Warning($"Failed to find any configured guild {guildId} for Discord bot.");
                return;
            }

            var server = _config.Instance.Servers[guildId];
            // If channel specified, delete all messages in channel
            if (channel != null)
            {
                await DeleteChannelMessages(ctx, channel);
                return;
            }

            // Otherwise loop channels and delete all messages in all channels
            var questChannels = server.QuestsPurge.ChannelIds;
            foreach (var (timezone, channelIds) in questChannels)
            {
                foreach (var channelId in channelIds)
                {
                    var questChannel = await ctx.Client.GetChannelAsync(channelId);
                    if (questChannel == null)
                    {
                        _logger.Warning($"Unable to get quest channel with id '{channelId}'.");
                        continue;
                    }

                    // Delete all channel messages
                    await DeleteChannelMessages(ctx, questChannel);
                }
            }
        }

        private async Task DeleteChannelMessages(CommandContext ctx, DiscordChannel channel)
        {
            var messages = await channel.GetMessagesAsync();
            while (messages.Count > 0)
            {
                for (var i = 0; i < messages.Count; i++)
                {
                    var message = messages[i];
                    if (message == null)
                        continue;

                    await message.DeleteAsync("Channel reset.");
                    Thread.Sleep(100);
                }

                messages = await channel.GetMessagesAsync();
            }
            await ctx.RespondEmbed(Translator.Instance.Translate("CHANNEL_MESSAGES_DELETED").FormatText(new
            {
                author = ctx.User.Username,
                channel = channel.Mention,
            }));
        }
    }
}