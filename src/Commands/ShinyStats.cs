namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    using ServiceStack.OrmLite;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Net.Models;

    public class ShinyStats
    {
        //TODO: Add possible shinies list to external file
        private static readonly List<int> _possiblyShinies = new List<int> { 1, 4, 7, 10, 16, 19, 23, 25, 27, 29, 32, 41, 50, 54, 56, 58, 60, 66, 74, 77, 81, 86, 88, 90, 92, 95, 96, 98, 103, 104, 109, 116, 123, 127, 128, 129, 131, 133, 138, 140, 142, 147, 152, 155, 158, 161, 177, 179, 190, 191, 193, 198, 200, 204, 207, 209, 213, 215, 220, 225, 228, 246, 252, 255, 258, 261, 263, 270, 276, 278, 280, 287, 296, 302, 303, 304, 307, 309, 311, 312, 315, 318, 320, 325, 327, 328, 333, 336, 337, 338, 339, 345, 347, 349, 351, 353, 355, 359, 361, 366, 370, 371, 374, 387, 403, 425, 427, 436, 504, 506 };
        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private readonly Dependencies _dep;

        public ShinyStats(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("shiny-stats"),
            Description("")
        ]
        public async Task GetShinyStats(CommandContext ctx)
        {
            if (!_dep.WhConfig.ShinyStats.Enabled)
                return;

            if (_dep.WhConfig.ShinyStats.ClearMessages)
            {
                await ctx.Message.DeleteAsync();
            }

            var statsChannel = await ctx.Client.GetChannelAsync(_dep.WhConfig.ShinyStats.ChannelId);
            if (statsChannel == null)
            {
                _logger.Warn($"Failed to get channel id {_dep.WhConfig.ShinyStats.ChannelId} to post shiny stats.");
            }
            else
            {
                if (_dep.WhConfig.ShinyStats.ClearMessages)
                {
                    await ctx.Client.DeleteMessages(_dep.WhConfig.ShinyStats.ChannelId);
                }

                await statsChannel.SendMessageAsync($"[**Shiny Pokemon stats for {DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString()}**]\r\n----------------------------------------------");
                var stats = await GetStats(_dep.WhConfig);
                var sorted = stats.Keys.ToList();
                sorted.Sort();

                foreach (var pokemon in sorted)
                {
                    if (pokemon == 0)
                        continue;

                    if (!Database.Instance.Pokemon.ContainsKey(pokemon))
                        continue;

                    var pkmn = Database.Instance.Pokemon[pokemon];
                    var pkmnStats = stats[pokemon];
                    var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                    var chanceMessage = chance == 0 ? null : $" with a **1/{chance}** ratio";
                    await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**  |  **{pkmnStats.Shiny.ToString("N0")}** shiny out of **{pkmnStats.Total.ToString("N0")}** total seen in the last 24 hours{chanceMessage}.");
                }

                var total = stats[0];
                var ratio = total.Shiny == 0 || total.Total == 0 ? null : $" with a **1/{Convert.ToInt32(total.Total / total.Shiny)}** ratio in total";
                await statsChannel.SendMessageAsync($"Found **{total.Shiny.ToString("N0")}** total shinies out of **{total.Total.ToString("N0")}** possiblities{ratio}.");
            }
        }

        public static Task<Dictionary<int, ShinyPokemonStats>> GetStats(WhConfig config)
        {
            var list = new Dictionary<int, ShinyPokemonStats>
            {
                { 0, new ShinyPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using (var db = DataAccessLayer.CreateFactory(config.ScannerConnectionString).Open())
                {
                    //var unixTimestamp = DateTimeOffset.ToUnixTimeSeconds();
                    var twentyFourHoursAgo = DateTime.Now.Subtract(TimeSpan.FromHours(24));
                    var pokemon = db.Select<PokemonData>().Where(x => x.Shiny.HasValue && x.Updated.FromUnix() > twentyFourHoursAgo).ToList();
                    for (var i = 0; i < pokemon.Count; i++)
                    {
                        var curPkmn = pokemon[i];
                        if (curPkmn.Id > 0 && _possiblyShinies.Contains(curPkmn.Id))
                        {
                            if (!list.ContainsKey(curPkmn.Id))
                            {
                                list.Add(curPkmn.Id, new ShinyPokemonStats { PokemonId = curPkmn.Id });
                            }

                            list[curPkmn.Id].PokemonId = curPkmn.Id;
                            list[curPkmn.Id].Shiny += curPkmn.Shiny.HasValue ? Convert.ToInt32(curPkmn.Shiny.Value) : 0;
                            list[curPkmn.Id].Total++;
                        }
                    }
                    list.ForEach((x, y) => list[0].Shiny += y.Shiny);
                    list.ForEach((x, y) => list[0].Total += y.Total);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return Task.FromResult(list);
        }
    }

    public class ShinyPokemonStat
    {
        public DateTime Date { get; set; }

        public ulong Total { get; set; }

        public ulong Shiny { get; set; }

        
        public uint PokemonId { get; set; }
    }

    public class ShinyPokemonStats
    {
        public int PokemonId { get; set; }

        public long Shiny { get; set; }

        public long Total { get; set; }
    }
}