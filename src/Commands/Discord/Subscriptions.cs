namespace WhMgr.Commands.Discord
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Subscriptions.Models;

    public class Subscriptions
    {
        private readonly ConfigHolder _config;

        public Subscriptions(ConfigHolder config)
        {
            _config = config;
        }

        [
            Command("enable"),
            Aliases("disable"),
            Description("Enables or disables all of your Pokemon and Raid notification subscriptions at once.")
        ]
        public async Task EnableDisableAsync(CommandContext ctx,
            [Description("Discord user mention string.")] string mention = "")
        {
            if (!await ctx.CanExecuteCommand(_config.Instance))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x));

            if (string.IsNullOrEmpty(mention))
            {
                await EnableDisableUserSubscriptions(ctx, ctx.User, guildId);
                return;
            }

            var isModOrHigher = await ctx.Client.IsModeratorOrHigher(ctx.User.Id, guildId, _config.Instance);
            if (!isModOrHigher)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("MSG_NOT_MODERATOR_OR_HIGHER").FormatText(new { author = ctx.User.Mention }), DiscordColor.Red);
                return;
            }

            var userId = ConvertMentionToUserId(mention);
            if (userId <= 0)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("MSG_INVALID_USER_MENTION").FormatText(new
                {
                    author = ctx.User.Mention,
                    mention = mention,
                }), DiscordColor.Red);
                return;
            }

            var user = await ctx.Client.GetUserAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"Failed to get Discord user with id {userId}.");
                return;
            }

            await EnableDisableUserSubscriptions(ctx, user, guildId);
        }

        private async Task EnableDisableUserSubscriptions(CommandContext ctx, DiscordUser user, ulong guildId)
        {
            /*
            var subscription = _subscriptionProcessor.Manager.GetUserSubscriptions(guildId, user.Id);
            if (subscription == null)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondEmbed(Translator.Instance.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(user.Username), DiscordColor.Red);
                return;
            }
            */

            var commandPrefix = _config.Instance.Servers[guildId].Bot.CommandPrefix;
            var cmd = ctx.Message.Content.TrimStart(Convert.ToChar(commandPrefix), ' ');
            /*
            var isEnableCommand = cmd.ToLower().Contains("enable");
            subscription.Status = isEnableCommand
                ? NotificationStatusType.All
                : NotificationStatusType.None;
            subscription.Update();
            //subscription.Save();
            */
            await ctx.TriggerTypingAsync();
            await ctx.RespondEmbed(Translator.Instance.Translate("NOTIFY_ENABLE_DISABLE").FormatText(new
            {
                author = user.Username,
                command = cmd,
            }));

            // TODO: _subscriptionProcessor.Manager.ReloadSubscriptions();
        }

        private static ulong ConvertMentionToUserId(string mention)
        {
            //<@201909896357216256>
            //mention = Utils.GetBetween(mention, "<", ">");
            mention = mention.Replace("<", null);
            mention = mention.Replace(">", null);
            mention = mention.Replace("@", null);
            mention = mention.Replace("!", null);

            return ulong.TryParse(mention, out ulong result) ? result : 0;
        }
    }
}