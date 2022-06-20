﻿namespace WhMgr.Commands.Discord
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
    using WhMgr.Extensions;
    using WhMgr.Localization;

    // TODO: Simplified IV stats postings via command with arg `list`
    // TODO: Get total IV found for IV stats
    // TODO: Include forms with shiny/iv stats

    public class DailyStats : BaseCommandModule
    {
        private readonly ConfigHolder _config;

        public DailyStats(ConfigHolder config)
        {
            _config = config;
        }

        [
            Command("shiny-stats"),
            RequirePermissions(Permissions.KickMembers),
        ]
        public async Task GetShinyStatsAsync(CommandContext ctx)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(guildId => _config.Instance.Servers.ContainsKey(guildId));

            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _config.Instance.Servers[guildId];
            if (!server.DailyStats.ShinyStats.Enabled)
                return;

            var statsChannel = await ctx.Client.GetChannelAsync(server.DailyStats.ShinyStats.ChannelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {server.DailyStats.ShinyStats.ChannelId} to post shiny stats.");
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("SHINY_STATS_INVALID_CHANNEL").FormatText(new { author = ctx.User.Username }), DiscordColor.Yellow);
                return;
            }

            if (server.DailyStats.ShinyStats.ClearMessages)
            {
                await ctx.Client.DeleteMessagesAsync(server.DailyStats.ShinyStats.ChannelId);
            }

            var stats = await GetShinyStats(_config.Instance.Database.Scanner.ToString());
            var sorted = stats.Keys.ToList();
            sorted.Sort();
            if (sorted.Count > 0)
            {
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TITLE").FormatText(new { date = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString() }));
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_NEWLINE"));
            }

            foreach (var pokemon in sorted)
            {
                if (pokemon == 0)
                    continue;

                if (!GameMaster.Instance.Pokedex.ContainsKey(pokemon))
                    continue;

                var pkmn = GameMaster.Instance.Pokedex[pokemon];
                var pkmnStats = stats[pokemon];
                var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                if (chance == 0)
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE").FormatText(new
                    {
                        pokemon = pkmn.Name,
                        id = pokemon,
                        shiny = pkmnStats.Shiny.ToString("N0"),
                        total = pkmnStats.Total.ToString("N0"),
                    }));
                }
                else
                {
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE_WITH_RATIO").FormatText(new
                    {
                        pokemon = pkmn.Name,
                        id = pokemon,
                        shiny = pkmnStats.Shiny.ToString("N0"),
                        total = pkmnStats.Total.ToString("N0"),
                        chance,
                    }));
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
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(new
                {
                    shiny = total.Shiny.ToString("N0"),
                    total = total.Total.ToString("N0"),
                    chance = totalRatio,
                }));
            }
        }

        [
            Command("iv-stats"),
            RequirePermissions(Permissions.KickMembers),
        ]
        public async Task GetIVStatsAsync(CommandContext ctx, uint minimumIV = 100)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(guildId => _config.Instance.Servers.ContainsKey(guildId));

            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _config.Instance.Servers[guildId];
            if (!server.DailyStats.IVStats.Enabled)
                return;

            var statsChannel = await ctx.Client.GetChannelAsync(server.DailyStats.IVStats.ChannelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {server.DailyStats.IVStats.ChannelId} to post shiny stats.");
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("SHINY_STATS_INVALID_CHANNEL").FormatText(ctx.User.Username), DiscordColor.Yellow);
                return;
            }

            if (server.DailyStats.IVStats.ClearMessages)
            {
                await ctx.Client.DeleteMessagesAsync(server.DailyStats.IVStats.ChannelId);
            }

            var stats = GetIvStats(_config.Instance.Database.Scanner.ToString(), minimumIV);

            var date = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString();
            // TODO: Localize IV stats
            await statsChannel.SendMessageAsync($"[**{minimumIV}% IV Pokemon stats for {date}**]");
            await statsChannel.SendMessageAsync("----------------------------------------------");

            //var sb = new System.Text.StringBuilder();
            var keys = stats.Keys.ToList();
            keys.Sort();
            //foreach (var (pokemonId, count) in stats)
            foreach (var key in keys)
            {
                var count = stats[key];
                var total = 0;
                var ratio = 0;
                var pkmn = GameMaster.GetPokemon(key);
                //sb.AppendLine($"- {pkmn.Name} (#{key}) {count:N0}");
                await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{key})**    |    **{count:N0}** out of **{total}** total seen in the last 24 hours with a **1/{ratio}** ratio.");
            }

            await statsChannel.SendMessageAsync($"Found **8,094** total {minimumIV}% IV Pokemon out of **4,050,641** possiblities with a **1/500** ratio in total.");
            /*
            var embed = new DiscordEmbedBuilder
            {
                Title = $"100% Pokemon Found (Last 24 Hours)",
                Description = sb.ToString(),
            };
            await ctx.RespondAsync(embed.Build());
            */
        }

        internal static async Task<Dictionary<uint, ShinyPokemonStats>> GetShinyStats(string scannerConnectionString)
        {
            var list = new Dictionary<uint, ShinyPokemonStats>
            {
                { 0, new ShinyPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using var ctx = DbContextFactory.CreateMapContext(scannerConnectionString);
                ctx.Database.SetCommandTimeout(TimeSpan.FromSeconds(30)); // 30 seconds timeout
                var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToString("yyyy/MM/dd");
                var pokemonShiny = (await ctx.PokemonStatsShiny.ToListAsync()).Where(stat => stat.Date.ToString("yyyy/MM/dd") == yesterday).ToList();
                var pokemonIV = (await ctx.PokemonStatsIV.ToListAsync()).Where(stat => stat.Date.ToString("yyyy/MM/dd") == yesterday)?.ToDictionary(stat => stat.PokemonId);
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
                list.Values.ToList().ForEach(stat =>
                {
                    list[0].Shiny += stat.Shiny;
                    list[0].Total += stat.Total;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return list;
        }

        internal static Dictionary<uint, int> GetIvStats(string scannerConnectionString, double minIV)
        {
            try
            {
                using var ctx = DbContextFactory.CreateMapContext(scannerConnectionString);
                ctx.Database.SetCommandTimeout(TimeSpan.FromSeconds(30)); // 30 seconds timeout
                var now = DateTime.UtcNow;
                var hoursAgo = TimeSpan.FromHours(24);
                var yesterday = Convert.ToInt64(Math.Round(now.Subtract(hoursAgo).GetUnixTimestamp()));
                // Checks within last 24 hours and 100% IV (or use statistics cache?)
                var pokemon = ctx.Pokemon
                    .AsEnumerable()
                    .Where(pokemon => pokemon.Attack != null && pokemon.Defense != null && pokemon.Stamina != null
                        && pokemon.DisappearTime > yesterday
                        && GetIV(pokemon.Attack, pokemon.Defense, pokemon.Stamina) >= minIV
                      //&& x.Attack == 15
                      //&& x.Defense == 15
                      //&& x.Stamina == 15
                      )
                    .AsEnumerable()
                    .GroupBy(x => x.PokemonId, y => y.IV)
                    .Select(g => new { name = g.Key, count = g.Count() })
                    .ToDictionary(x => x.name, y => y.count);
                return pokemon;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return null;
        }

        static double GetIV(ushort? attack, ushort? defense, ushort? stamina)
        {
            return Math.Round((attack ?? 0 + defense ?? 0 + stamina ?? 0) * 100.0 / 45.0, 1);
        }

        internal class ShinyPokemonStats
        {
            public uint PokemonId { get; set; }

            public long Shiny { get; set; }

            public long Total { get; set; }
        }

        internal class IvPokemonStats
        {
            public uint PokemonId { get; set; }

            public long Count { get; set; }

            public long Total { get; set; }
        }
    }
}