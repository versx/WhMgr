namespace WhMgr.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Factories;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Discord;

    public class StatisticReportsHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<QuestPurgeHostedService> _logger;
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;
        private readonly ConfigHolder _config;
        private readonly IDiscordClientService _discordService;

        public StatisticReportsHostedService(
            ILogger<QuestPurgeHostedService> logger,
            ConfigHolder config,
            IDiscordClientService discordService)
        {
            _logger = logger;
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
            _config = config;
            _discordService = discordService;
        }

        public void Dispose()
        {
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Starting daily statistic reporting hosted service...");

            var localZone = TimeZoneInfo.Local;
            var timezone = localZone.StandardName;

            var midnightTimer = new MidnightTimer(0, timezone);
            midnightTimer.TimeReached += OnMidnightTimerTimeReached;
            midnightTimer.Start();

            _tzMidnightTimers.Add(timezone, midnightTimer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Stopping daily statistic reporting hosted service...");

            foreach (var (_, midnightTimer) in _tzMidnightTimers)
            {
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        private async void OnMidnightTimerTimeReached(DateTime time, string timezone)
        {
            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                if (!_discordService.DiscordClients.ContainsKey(guildId))
                {
                    continue;
                }

                var client = _discordService.DiscordClients[guildId];
                if (guildConfig.DailyStats?.ShinyStats?.Enabled ?? false)
                {
                    _logger.Information($"Starting daily shiny stats posting for guild '{guildId}'...");
                    await PostShinyStatsAsync(guildId, _config.Instance, client);
                    _logger.Information($"Finished daily shiny stats posting for guild '{guildId}'.");
                }

                if (guildConfig.DailyStats?.IVStats?.Enabled ?? false)
                {
                    _logger.Information($"Starting daily hundo stats posting for guild '{guildId}'...");
                    await PostHundoStatsAsync(guildId, _config.Instance, client);
                    _logger.Information($"Finished daily hundo stats posting for guild '{guildId}'.");
                }

                _logger.Information($"Finished daily stats posting for guild '{guildId}'...");
            }

            _logger.Information($"Finished daily stats reporting for all guilds.");
        }

        public static async Task PostShinyStatsAsync(ulong guildId, Config config, DiscordClient client)
        {
            if (!config.Servers.ContainsKey(guildId))
            {
                // Guild not configured
                //await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = config.Servers[guildId];
            if (!(server.DailyStats?.ShinyStats?.Enabled ?? false))
            {
                // Shiny stats not enabled
                return;
            }

            if (!client.Guilds.ContainsKey(guildId))
            {
                // Discord client not in specified guild
                return;
            }

            var guild = client.Guilds[guildId];
            var channelId = server.DailyStats.ShinyStats.ChannelId;
            if (!guild.Channels.ContainsKey(channelId))
            {
                // Discord channel does not exist
                return;
            }

            var statsChannel = await client.GetChannelAsync(channelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {channelId} to post shiny stats.");
                //await ctx.RespondEmbedAsync(Translator.Instance.Translate("SHINY_STATS_INVALID_CHANNEL").FormatText(new { author = ctx.User.Username }), DiscordColor.Yellow);
                return;
            }

            if (server.DailyStats.ShinyStats.ClearMessages)
            {
                await client.DeleteMessagesAsync(channelId);
            }

            var stats = await GetShinyStatsAsync(config.Database.Scanner.ToString());
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

            await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(new
            {
                shiny = total.Shiny.ToString("N0"),
                total = total.Total.ToString("N0"),
                chance = totalRatio,
            }));
        }

        public static async Task PostHundoStatsAsync(ulong guildId, Config config, DiscordClient client)
        {
            if (!config.Servers.ContainsKey(guildId))
            {
                // Guild not configured
                //await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = config.Servers[guildId];
            if (!(server.DailyStats?.IVStats?.Enabled ?? false))
            {
                // Shiny stats not enabled
                return;
            }

            if (!client.Guilds.ContainsKey(guildId))
            {
                // Discord client not in specified guild
                return;
            }

            var guild = client.Guilds[guildId];
            var channelId = server.DailyStats.IVStats.ChannelId;
            if (!guild.Channels.ContainsKey(channelId))
            {
                // Discord channel does not exist
                return;
            }

            var statsChannel = await client.GetChannelAsync(channelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {channelId} to post hundo stats.");
                //await ctx.RespondEmbedAsync(Translator.Instance.Translate("SHINY_STATS_INVALID_CHANNEL").FormatText(new { author = ctx.User.Username }), DiscordColor.Yellow);
                return;
            }

            if (server.DailyStats.IVStats.ClearMessages)
            {
                await client.DeleteMessagesAsync(channelId);
            }

            var stats = await GetHundoStatsAsync(config.Database.Scanner.ToString());
            var sorted = stats.Keys.ToList();
            sorted.Sort();
            if (sorted.Count > 0)
            {
                //await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TITLE").FormatText(new { date = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString() }));
                //await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_NEWLINE"));
                var date = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToLongDateString();
                await statsChannel.SendMessageAsync($"[**Hundo Pokemon stats for {date}**]");
                await statsChannel.SendMessageAsync($"----------------------------------------------");
            }

            foreach (var pokemon in sorted)
            {
                if (pokemon == 0)
                    continue;

                if (!GameMaster.Instance.Pokedex.ContainsKey(pokemon))
                    continue;

                var pkmn = GameMaster.Instance.Pokedex[pokemon];
                var pkmnStats = stats[pokemon];
                var chance = pkmnStats.Count == 0 || pkmnStats.Total == 0 ? 0 : Convert.ToInt32(pkmnStats.Total / pkmnStats.Count);
                if (chance == 0)
                {
                    /*
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE").FormatText(new
                    {
                        pokemon = pkmn.Name,
                        id = pokemon,
                        shiny = pkmnStats.Count.ToString("N0"),
                        total = pkmnStats.Total.ToString("N0"),
                    }));
                    */
                    await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**    |    **{pkmnStats.Count:N0}** 100% IV out of **{pkmnStats.Total:N0}** total seen in the last 24 hours.");
                }
                else
                {
                    /*
                    await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_MESSAGE_WITH_RATIO").FormatText(new
                    {
                        pokemon = pkmn.Name,
                        id = pokemon,
                        shiny = pkmnStats.Count.ToString("N0"),
                        total = pkmnStats.Total.ToString("N0"),
                        chance,
                    }));
                    */
                    await statsChannel.SendMessageAsync($"**{pkmn.Name} (#{pokemon})**    |    **{pkmnStats.Count:N0}** 100% IV out of **{pkmnStats.Total:N0}** total seen in the last 24 hours with a **1/{chance}** ratio.");
                }
                Thread.Sleep(500);
            }

            var total = stats[0];
            var totalRatio = total.Count == 0 || total.Total == 0 ? 0 : Convert.ToInt32(total.Total / total.Count);

            /*
            await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(new
            {
                count = total.Count.ToString("N0"),
                total = total.Total.ToString("N0"),
                chance = totalRatio,
            }));
            */
            await statsChannel.SendMessageAsync($"Found **{total.Count:N0}** total hundos out of **{total.Total:N0}** possiblities with a **1/{totalRatio}** ratio in total.");
        }

        internal static async Task<Dictionary<uint, ShinyPokemonStats>> GetShinyStatsAsync(string scannerConnectionString)
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
                        list[curPkmn.PokemonId].Shiny += Convert.ToUInt64(curPkmn.Count);
                        list[curPkmn.PokemonId].Total += pokemonIV.ContainsKey(curPkmn.PokemonId)
                            ? Convert.ToUInt64(pokemonIV[curPkmn.PokemonId].Count)
                            : 0;
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

        internal static async Task<Dictionary<uint, HundoPokemonStats>> GetHundoStatsAsync(string scannerConnectionString)
        {
            var list = new Dictionary<uint, HundoPokemonStats>
            {
                { 0, new HundoPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using var ctx = DbContextFactory.CreateMapContext(scannerConnectionString);
                ctx.Database.SetCommandTimeout(TimeSpan.FromSeconds(30)); // 30 seconds timeout
                var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(24)).ToString("yyyy/MM/dd");
                var pokemonHundo = (await ctx.PokemonStatsHundo.ToListAsync()).Where(stat => stat.Date.ToString("yyyy/MM/dd") == yesterday).ToList();
                var pokemonIV = (await ctx.PokemonStatsIV.ToListAsync()).Where(stat => stat.Date.ToString("yyyy/MM/dd") == yesterday)?.ToDictionary(stat => stat.PokemonId);
                for (var i = 0; i < pokemonHundo.Count; i++)
                {
                    var curPkmn = pokemonHundo[i];
                    if (curPkmn.PokemonId > 0)
                    {
                        if (!list.ContainsKey(curPkmn.PokemonId))
                        {
                            list.Add(curPkmn.PokemonId, new HundoPokemonStats { PokemonId = curPkmn.PokemonId });
                        }

                        list[curPkmn.PokemonId].PokemonId = curPkmn.PokemonId;
                        list[curPkmn.PokemonId].Count += Convert.ToUInt64(curPkmn.Count);
                        list[curPkmn.PokemonId].Total += pokemonIV.ContainsKey(curPkmn.PokemonId)
                            ? Convert.ToUInt64(pokemonIV[curPkmn.PokemonId].Count)
                            : 0;
                    }
                }
                list.Values.ToList().ForEach(stat =>
                {
                    list[0].Count += stat.Count;
                    list[0].Total += stat.Total;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return list;
        }
    }

    internal class ShinyPokemonStats
    {
        public uint PokemonId { get; set; }

        public ulong Shiny { get; set; }

        public ulong Total { get; set; }
    }

    internal class IvPokemonStats
    {
        public uint PokemonId { get; set; }

        public ulong Count { get; set; }

        public ulong Total { get; set; }
    }

    internal class HundoPokemonStats
    {
        public uint PokemonId { get; set; }

        public ulong Count { get; set; }

        public ulong Total { get; set; }
    }
}