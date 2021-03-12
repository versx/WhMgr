namespace WhMgr.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    [
        Group("modify-filters"),
        RequirePermissions(DSharpPlus.Permissions.KickMembers),
    ]
    public class ModifyFilters
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("FILTERS", Program.LogLevel);

        private readonly Dependencies _dep;

        public ModifyFilters(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("list"),
            Description(""),
        ]
        public async Task ListFiltersAsync(CommandContext ctx, DiscordChannel channel)
        {
            if (!await ctx.IsDirectMessageSupported(_dep.WhConfig))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));
            if (!_dep.WhConfig.Servers.ContainsKey(guildId))
                return;

            var server = _dep.WhConfig.Servers[guildId];

            var filter = Net.Webhooks.WebhookController.FiltersCache[channel.GuildId][channel.Id];
            var filterPath = System.IO.Path.Combine(Strings.FiltersFolder, filter);
            var json = System.IO.File.ReadAllText(filterPath);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Alarms.Filters.Models.FilterPokemonObject>(json);

            await ctx.RespondEmbed($"Channel: {channel.Name} Available Pokemon: {string.Join(", ", obj.Pokemon)}");
        }

        [
            Command("add"),
            //Aliases("", ""),
            Description("")
        ]
        public async Task AddFiltersAsync(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_dep.WhConfig))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));
            if (!_dep.WhConfig.Servers.ContainsKey(guildId))
                return;

            var server = _dep.WhConfig.Servers[guildId];
        }

        [
            Command("remove"),
            //Aliases("", ""),
            Description("")
        ]
        public async Task RemoveFiltersAsync(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_dep.WhConfig))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));
            if (!_dep.WhConfig.Servers.ContainsKey(guildId))
                return;

            var server = _dep.WhConfig.Servers[guildId];
        }
    }
}