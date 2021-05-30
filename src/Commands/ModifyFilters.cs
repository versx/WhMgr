namespace WhMgr.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    using WhMgr.Configuration;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    [Group("filters")]
    public class ModifyFilters : BaseCommandModule
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("FILTERS", Program.LogLevel);

        private readonly WhConfig _config;

        public ModifyFilters(WhConfig config)
        {
            _config = config;
        }

        [
            Command("add"),
            //Aliases("", ""),
            Description("")
        ]
        public async Task AddFilters(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_config))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Servers.ContainsKey(x));
            if (!_config.Servers.ContainsKey(guildId))
                return;

            var server = _config.Servers[guildId];
        }

        [
            Command("edit"),
            //Aliases("", ""),
            Description("")
        ]
        public async Task EditFilters(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_config))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Servers.ContainsKey(x));
            if (!_config.Servers.ContainsKey(guildId))
                return;

            var server = _config.Servers[guildId];
        }
    }
}