using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using WhMgr.Diagnostics;
using WhMgr.Extensions;
using WhMgr.Localization;
using WhMgr.Net.Webhooks;

namespace WhMgr.Commands
{
    public class Areas
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("AREAS", Program.LogLevel);

        private readonly Dependencies _dep;

        public Areas(Dependencies dep)
        {
            _dep = dep;

        }

        [
            Command("areas"),
            //Aliases("", ""),
            Description("Shows a list of areas")
        ]
        public async Task SendPaginated(CommandContext ctx)
        {
            if (!await ctx.IsDirectMessageSupported(_dep.WhConfig))
                return;

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));
            if (!_dep.WhConfig.Servers.ContainsKey(guildId))
                return;

            var server = _dep.WhConfig.Servers[guildId];

            var geofences = _dep.Whm.GetServerGeofences(guildId);

            List<string> areas = geofences.Select(geofence => geofence.Name).OrderBy(Name => Name).ToList();


            var interactivity = ctx.Client.GetInteractivityModule();
            List<Page> pages = new List<Page>();
            int pagelength = 0;
            var psb = new StringBuilder();
            int linesThisPage = 0;
            int num = 1;
            var title = string.Format("Page {0}", (object)num);
            foreach (var line in areas.Select(area => $"- {area}\n"))
            {
                var length = line.Length;
                var wouldGoOver = length + pagelength > 2000;

                if (wouldGoOver || linesThisPage >= 25)
                {
                    title = string.Format("Page {0}", (object)num);
                    var eb = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Blue,
                        Title = title,
                        Description = psb.ToString(),
                    };
                    pages.Add(new Page { Embed = eb });
                    psb.Clear();
                    pagelength = 0;
                    linesThisPage = 0;
                    ++num;
                }

                psb.Append(line);
                pagelength += length;
                linesThisPage++;
            }

            if (psb.Length > 0)
            {
                title = string.Format("Page {0}", (object)num);
                var eb = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = title,
                    Description = psb.ToString(),

                };
                pages.Add(new Page { Embed = eb });
            }

            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, pages, timeoutoverride: TimeSpan.FromMinutes(5));
        }

    }


}
