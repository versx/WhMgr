namespace WhMgr.Commands
{
    using System;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;

    public class Quests
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private readonly Dependencies _dep;

        public Quests(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("reset-quests"),
            Hidden,
            RequireOwner
        ]
        public async Task ResetChannelAsync(CommandContext ctx,
            [Description("Discord channel to reset.")] DiscordChannel channel = null)
        {
            if (channel == null)
            {
                for (var i = 0; i < _dep.WhConfig.QuestChannelIds.Count; i++)
                {
                    var qChannel = await ctx.Client.GetChannelAsync(_dep.WhConfig.QuestChannelIds[i]);
                    if (qChannel == null)
                    {
                        continue; //TODO: Log warning.
                    }

                    await DeleteChannelMessages(ctx, qChannel);
                }
                return;
            }

            await DeleteChannelMessages(ctx, channel);
        }

        private async Task DeleteChannelMessages(CommandContext ctx, DiscordChannel channel)
        {
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
            await ctx.RespondAsync($"{ctx.User.Mention} Channel {channel.Mention} messages have been deleted.");
        }
    }
}