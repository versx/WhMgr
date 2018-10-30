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
                //TODO: Delete from all quest channels.
                return;
            }

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