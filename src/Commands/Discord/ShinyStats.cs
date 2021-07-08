namespace WhMgr.Commands.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Factories;
    using WhMgr.Data.Models;
    using WhMgr.Extensions;
    using WhMgr.Localization;

    public class ShinyStats : BaseCommandModule
    {
        private readonly ConfigHolder _config;

        public ShinyStats(ConfigHolder config)
        {
            _config = config;
        }

        [
            Command("shiny-stats"),
            RequirePermissions(Permissions.KickMembers),
        ]
        public async Task GetShinyStatsAsync(CommandContext ctx)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x));

            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _config.Instance.Servers[guildId];
            if (!server.ShinyStats.Enabled)
                return;

            var statsChannel = await ctx.Client.GetChannelAsync(server.ShinyStats.ChannelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {server.ShinyStats.ChannelId} to post shiny stats.");
                await ctx.RespondEmbed(Translator.Instance.Translate("SHINY_STATS_INVALID_CHANNEL").FormatText(ctx.User.Username), DiscordColor.Yellow);
                return;
            }

            if (server.ShinyStats.ClearMessages)
            {
                await ctx.Client.DeleteMessages(server.ShinyStats.ChannelId);
            }

            var stats = await GetShinyStats(_config.Instance.Database.Scanner.ToString());
            var sorted = stats.Keys.ToList();
            sorted.Sort();
            if (sorted.Count > 0)
            {
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TITLE").FormatText(DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString()));
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_NEWLINE"));
            }

            foreach (var pokemon in sorted)
            {
                if (pokemon == 0)
                    continue;

                if (!MasterFile.Instance.Pokedex.ContainsKey(pokemon))
                    continue;

                var pkmn = MasterFile.Instance.Pokedex[pokemon];
                var pkmnStats = stats[pokemon];
                var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                if (chance == 0)
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE").FormatText(pkmn.Name, pokemon, pkmnStats.Shiny.ToString("N0"), pkmnStats.Total.ToString("N0")));
                }
                else
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE_WITH_RATIO").FormatText(pkmn.Name, pokemon, pkmnStats.Shiny.ToString("N0"), pkmnStats.Total.ToString("N0"), chance));
                }
                Thread.Sleep(500);
            }

            var total = stats[0];
            var totalRatio = total.Shiny == 0 || total.Total == 0 ? 0 : Convert.ToInt32(total.Total / total.Shiny);
            if (totalRatio == 0)
            {
                //await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE").FormatText(total.Shiny.ToString("N0"), total.Total.ToString("N0")));
                // Error, try again
                await GetShinyStatsAsync(ctx);
            }
            else
            {
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(total.Shiny.ToString("N0"), total.Total.ToString("N0"), totalRatio));
            }
        }

        [
            Command("iv-stats"),
            RequirePermissions(Permissions.KickMembers),
        ]
        public async Task GetIVStatsAsync(CommandContext ctx)
        {
            // TODO: IV stats postings
            await Task.CompletedTask;
        }

        internal static async Task<Dictionary<uint, ShinyPokemonStats>> GetShinyStats(string scannerConnectionString)
        {
            var list = new Dictionary<uint, ShinyPokemonStats>
            {
                { 0, new ShinyPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using (var ctx = DbContextFactory.CreateMapContext(scannerConnectionString))
                {
                    ctx.Database.SetCommandTimeout(TimeSpan.FromSeconds(30)); // 30 seconds timeout
                    var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToString("yyyy/MM/dd");
                    var pokemonShiny = (await ctx.PokemonStatsShiny.ToListAsync()).Where(x => x.Date.ToString("yyyy/MM/dd") == yesterday).ToList();
                    var pokemonIV = (await ctx.PokemonStatsIV.ToListAsync()).Where(x => x.Date.ToString("yyyy/MM/dd") == yesterday)?.ToDictionary(x => x.PokemonId);
                    for (var i = 0; i < pokemonShiny.Count; i++)
                    {
                        var curPkmn = pokemonShiny[i];
                        if (curPkmn.PokemonId > 0)
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
                    list.Values.ToList().ForEach(x =>
                    {
                        list[0].Shiny += x.Shiny;
                        list[0].Total += x.Total;
                    });
                    var test = list;
                    Console.WriteLine($"Shiny list: {list}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return list;
        }

        internal class ShinyPokemonStats
        {
            public uint PokemonId { get; set; }

            public long Shiny { get; set; }

            public long Total { get; set; }
        }
    }
}