namespace WhMgr.Commands.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;

    public class Areas : BaseCommandModule
    {
        private readonly ConfigHolder _config;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Areas(
            ConfigHolder config,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger(typeof(Feeds).FullName);
        }

        [
            Command("areas"),
            Description("Shows a list of available areas covered")
        ]
        public async Task SendPaginated(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupportedAsync(_config.Instance))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(guildId => _config.Instance.Servers.ContainsKey(guildId));
            if (!_config.Instance.Servers.ContainsKey(guildId))
                return;

            List<Geofence> GetServerGeofences(ulong guildId)
            {
                if (!_config.Instance.Servers.ContainsKey(guildId))
                {
                    _logger.Warning($"Failed to get geofences from guild: {guildId}");
                    return null;
                }
                return _config.Instance.Servers[guildId].Geofences;
            }

            var server = _config.Instance.Servers[guildId];
            var geofences = GetServerGeofences(guildId);
            var areas = geofences.Select(geofence => geofence.Name).OrderBy(Name => Name).ToList();

            var interactivity = ctx.Client.GetInteractivity();
            var pages = new List<Page>();
            var pageLength = 0;
            var psb = new StringBuilder();
            var linesThisPage = 0;
            var pageNum = 1;

            foreach (var line in areas.Select(area => $"- {area}\n"))
            {
                var length = line.Length;
                var wouldGoOver = length + pageLength > 2000;

                if (wouldGoOver || linesThisPage >= 25)
                {
                    var eb = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Blue,
                        Title = $"Page {pageNum}",
                        Description = psb.ToString(),
                    };
                    pages.Add(new Page { Embed = eb });
                    psb.Clear();
                    pageLength = 0;
                    linesThisPage = 0;
                    ++pageNum;
                }

                psb.Append(line);
                pageLength += length;
                linesThisPage++;
            }

            if (psb.Length > 0)
            {
                var eb = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = $"Page {pageNum}",
                    Description = psb.ToString(),

                };
                pages.Add(new Page { Embed = eb });
            }

            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);//, timeoutoverride: TimeSpan.FromMinutes(5));
        }
    }
}