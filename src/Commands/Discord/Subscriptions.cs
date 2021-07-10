namespace WhMgr.Commands.Discord
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Subscriptions.Models;

    public class Subscriptions : BaseCommandModule
    {
        private readonly ConfigHolder _config;
        //private readonly ISubscriptionManagerService _subscriptionManager;

        public Subscriptions(ConfigHolder config)//, ISubscriptionManagerService subscriptionManager)
        {
            _config = config;
            //_subscriptionManager = subscriptionManager;
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
            var subscription = await _subscriptionManager.GetUserSubscriptionsAsync(guildId, user.Id);
            if (subscription == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("MSG_USER_NOT_SUBSCRIBED").FormatText(user.Username), DiscordColor.Red);
                return;
            }
            */

            var commandPrefix = _config.Instance.Servers[guildId].Bot.CommandPrefix;
            var cmd = ctx.Message.Content.TrimStart(Convert.ToChar(commandPrefix), ' ');
            var isEnableCommand = cmd.ToLower().Contains("enable");
            /*
            subscription.Status = isEnableCommand
                ? NotificationStatusType.All
                : NotificationStatusType.None;
            _subscriptionManager.Save(subscription);
            await ctx.RespondEmbed(Translator.Instance.Translate("NOTIFY_ENABLE_DISABLE").FormatText(new
            {
                author = user.Username,
                command = cmd,
            }));

            await _subscriptionManager.ReloadSubscriptionsAsync();
            */
        }

        private static ulong ConvertMentionToUserId(string mention)
        {
            //<@201909896357216256>
            //mention = Utils.GetBetween(mention, "<", ">");
            var pattern = new Regex("[<>@! ]");
            mention = pattern.Replace(mention, "");
            return ulong.TryParse(mention, out ulong result) ? result : 0;
        }
    }
}