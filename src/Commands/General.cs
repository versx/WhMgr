namespace WhMgr.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Extensions;

    public class General
    {
        private readonly Dependencies _dep;

        public General(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("create-stream"),
            Description("")
        ]
        public async Task CreateStreamChannelAsync(CommandContext ctx,
            [Description("Stream channel name"), RemainingText] string channelName)
        {
            if (!await ctx.Message.IsDirectMessageSupported())
                return;

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, _dep.WhConfig);
            if (!isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (_dep.WhConfig.StreamCategoryChannelId == 0)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Stream category channel not setup.", DiscordColor.Red);
                return;
            }

            var streamCategoryChannelId = _dep.WhConfig.StreamCategoryChannelId;
            var streamCategory = await ctx.Client.GetChannelAsync(streamCategoryChannelId);
            if (streamCategory == null)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Failed to get stream category channel with id {streamCategoryChannelId}.", DiscordColor.Red);
                return;
            }

            var children = streamCategory.Children.ToList();
            if (children.Find(x => string.Compare(x.Name, channelName, true) == 0) != null)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Failed to create new stream channel, channel already exists with name `{channelName}`.", DiscordColor.Red);
                return;
            }

            var newStreamChannel = await ctx.Guild?.CreateChannelAsync(channelName, ChannelType.Voice, streamCategory);
            if (newStreamChannel == null)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Failed to create new stream channel, unknown error occurred.", DiscordColor.Red);
                return;
            }

            await ctx.RespondEmbed($"{ctx.User.Username} New stream channel created successfully. {newStreamChannel.Mention} (#{newStreamChannel.Name}).");
        }
    }
}
