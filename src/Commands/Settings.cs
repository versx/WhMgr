namespace WhMgr.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    [
        Group("settings"),
        Aliases("config", "cfg", "conf", "c"),
        Description("Event Pokemon management commands."),
        Hidden,
        RequirePermissions(Permissions.KickMembers)
    ]
    public class Settings
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("SETTINGS");

        private readonly Dependencies _dep;

        public Settings(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("list"),
            Aliases("l"),
            Description("List config settings for current guild.")
        ]
        public async Task ListSettingsAsync(CommandContext ctx)
        {
            if (!await ctx.Message.IsDirectMessageSupported())
                return;

            if (!_dep.WhConfig.Servers.ContainsKey(ctx.Guild?.Id ?? 0))
            {
                // TODO: Localize
                await ctx.RespondEmbed($"{ctx.User.Username} Guild {ctx.Guild?.Name} ({ctx.Guild?.Id}) not configured in {Strings.ConfigFileName}");
                return;
            }

            var guildConfig = _dep.WhConfig.Servers[ctx.Guild.Id];
            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blurple,
                Title = $"{ctx.Guild.Name} ({ctx.Guild.Id}) Config",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };

            eb.AddField($"Enable Cities", guildConfig.EnableCities ? "Yes" : "No", true);
            eb.AddField($"City Roles", string.Join("\r\n", guildConfig.CityRoles), true);
            eb.AddField($"Enable Subscriptions", guildConfig.EnableSubscriptions ? "Yes" : "No", true);
            eb.AddField($"Command Prefix", guildConfig.CommandPrefix ?? "@BotMentionHere", true);
            eb.AddField($"City Roles Require Donor Role", guildConfig.CitiesRequireSupporterRole ? "Yes" : "No", true);
            eb.AddField($"Donor Roles", string.Join("\r\n", guildConfig.DonorRoleIds.Select(x => $"{ctx.Guild.GetRole(x).Name}:{x}")), true);
            eb.AddField($"Moderators", string.Join("\r\n", guildConfig.Moderators.Select(async x => $"{(await ctx.Client.GetUserAsync(x))?.Username}:{x}")), true);
            eb.AddField($"Nest Channel", guildConfig.NestsChannelId == 0 ? "Not Set" : $"{ctx.Guild.GetChannel(guildConfig.NestsChannelId)?.Name}:{guildConfig.NestsChannelId}", true);
            eb.AddField($"Prune Quest Channels", guildConfig.PruneQuestChannels ? "Yes" : "No", true);
            eb.AddField($"Quest Channels", string.Join("\r\n", guildConfig.QuestChannelIds.Select(x => ctx.Guild.GetChannel(x)?.Name)), true);
            eb.AddField($"Enable Shiny Stats", guildConfig.ShinyStats?.Enabled ?? false ? "Yes" : "No", true);
            eb.AddField($"Shiny Stats Channel", guildConfig.ShinyStats?.ChannelId.ToString(), true);
            eb.AddField($"Clear Previous Shiny Stats", guildConfig.ShinyStats?.ClearMessages ?? false ? "Yes" : "No", true);
            eb.AddField($"Icon Style", guildConfig.IconStyle, true);
            await ctx.RespondAsync(embed: eb);
        }
    }
}
//List/add/remove city roles
//Enable quest pruning
//List/add/remove quest channel pruning
//Set command prefix
//Enable/disable subscriptions
//Enable/disable cities
//Set nest channel
//Manage shiny stats
//Set server icon style