namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using ServiceStack;
    using ServiceStack.DataAnnotations;
    using ServiceStack.OrmLite;

    using WhMgr.Data;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    public class ShinyStats
    {
        //TODO: Add possible shinies list to external file
        private static readonly List<int> _possiblyShinies = new List<int> {
            //1st Generation
            1, 4, 7, 10, 16, 19, 23, 25, 27, 29, 32, 41, 43, 50, 52, 54, 56, 58, 60, 66, 72, 74, 77, 81, 86, 88, 90, 92, 95, 96, 98, 103, 104, 109, 116, 123, 127, 128, 129, 131, 133, 138, 140, 142, 147,
            //2nd Generation
            152, 155, 158, 161, 177, 179, 190, 191, 193, 198, 200, 204, 207, 209, 213, 215, 220, 225, 227, 228, 246,
            //3rd Generation
            252, 255, 258, 261, 263, 270, 276, 278, 280, 287, 296, 302, 303, 304, 307, 309, 311, 312, 315, 318, 320, 325, 327, 328, 333, 336, 337, 338, 339, 345, 347, 349, 351, 353, 355, 359, 361, 366, 370, 371, 374,
            //4rd Generation
            387, 403, 412, 425, 427, 436,
            //5th Generation
            504, 506, 562
        };
        private static readonly IEventLogger _logger = EventLogger.GetLogger("SHINY_STATS");
        private readonly Dependencies _dep;

        public ShinyStats(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("shiny-stats"),
            DSharpPlus.CommandsNext.Attributes.Description(""),
            RequireOwner
        ]
        public async Task GetShinyStatsAsync(CommandContext ctx)
        {
            if (!_dep.WhConfig.Servers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondEmbed("Must be in Discord server.", DiscordColor.Red);
                return;
            }

            var server = _dep.WhConfig.Servers[ctx.Guild.Id];
            if (!server.ShinyStats.Enabled)
                return;

            var statsChannel = await ctx.Client.GetChannelAsync(server.ShinyStats.ChannelId);
            if (statsChannel == null)
            {
                _logger.Warn($"Failed to get channel id {server.ShinyStats.ChannelId} to post shiny stats.");
                await ctx.RespondAsync($"{ctx.User.Username} Shiny stats channel does not exist.");
                return;
            }

            if (server.ShinyStats.ClearMessages)
            {
                await ctx.Client.DeleteMessages(server.ShinyStats.ChannelId);
            }

            await statsChannel.SendMessageAsync($"[**Shiny Pokemon stats for {DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString()}**]\r\n----------------------------------------------");
            var stats = await GetShinyStats(_dep.WhConfig.Database.Scanner.ToString());
            var sorted = stats.Keys.ToList();
            sorted.Sort();

            foreach (var pokemon in sorted)
            {
                if (pokemon == 0)
                    continue;

                if (!MasterFile.Instance.Pokedex.ContainsKey((int)pokemon))
                    continue;

                var pkmn = MasterFile.Instance.Pokedex[(int)pokemon];
                var pkmnStats = stats[pokemon];
                var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                var chanceMessage = chance == 0 ? null : $" with a **1/{chance}** ratio";
                await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**  |  **{pkmnStats.Shiny.ToString("N0")}** shiny out of **{pkmnStats.Total.ToString("N0")}** total seen in the last 24 hours{chanceMessage}.");
            }

            var total = stats[0];
            var ratio = total.Shiny == 0 || total.Total == 0 ? null : $" with a **1/{Convert.ToInt32(total.Total / total.Shiny)}** ratio in total";
            await statsChannel.SendMessageAsync($"Found **{total.Shiny.ToString("N0")}** total shinies out of **{total.Total.ToString("N0")}** possiblities{ratio}.");
        }

        public static Task<Dictionary<uint, ShinyPokemonStats>> GetShinyStats(string scannerConnectionString)
        {
            var list = new Dictionary<uint, ShinyPokemonStats>
            {
                { 0, new ShinyPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using (var db = DataAccessLayer.CreateFactory(scannerConnectionString).Open())
                {
                    db.SetCommandTimeout(300);
                    var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToString("yyyy/MM/dd");
                    var pokemonShiny = db.Select<PokemonStatsShiny>().Where(x => string.Compare(x.Date.ToString("yyyy/MM/dd"), yesterday, true) == 0).ToList();
                    var pokemonIV = db.Select<PokemonStatsIV>().Where(x => string.Compare(x.Date.ToString("yyyy/MM/dd"), yesterday, true) == 0)?.ToDictionary(x => x.PokemonId);
                    for (var i = 0; i < pokemonShiny.Count; i++)
                    {
                        var curPkmn = pokemonShiny[i];
                        if (curPkmn.PokemonId > 0 && _possiblyShinies.Contains((int)curPkmn.PokemonId))
                        {
                            if (!list.ContainsKey(curPkmn.PokemonId))
                            {
                                list.Add(curPkmn.PokemonId, new ShinyPokemonStats { PokemonId = curPkmn.PokemonId });
                            }

                            list[curPkmn.PokemonId].PokemonId = curPkmn.PokemonId;
                            list[curPkmn.PokemonId].Shiny += Convert.ToInt32(curPkmn.Count);
                            list[curPkmn.PokemonId].Total += pokemonIV.ContainsKey(curPkmn.PokemonId) ? Convert.ToInt32(pokemonIV[curPkmn.PokemonId].Count) : 0;
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

        [Alias("pokemon_iv_stats")]
        public class PokemonStatsIV
        {
            [Alias("date")]
            public DateTime Date { get; set; }

            [Alias("pokemon_id")]
            public uint PokemonId { get; set; }

            [Alias("count")]
            public ulong Count { get; set; }
        }

        [Alias("pokemon_shiny_stats")]
        public class PokemonStatsShiny
        {
            [Alias("date")]
            public DateTime Date { get; set; }

            [Alias("pokemon_id")]
            public uint PokemonId { get; set; }

            [Alias("count")]
            public ulong Count { get; set; }
        }

        public class ShinyPokemonStats
        {
            public uint PokemonId { get; set; }

            public long Shiny { get; set; }

            public long Total { get; set; }
        }
    }
}