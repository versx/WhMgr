namespace WhMgr.Commands
{
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    public class Quests
    {
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
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                await message.DeleteAsync("Channel reset.");
            }
            await ctx.RespondAsync($"{ctx.User.Mention} Channel {channel.Mention}'s messages have been deleted.");
        }
    }
}