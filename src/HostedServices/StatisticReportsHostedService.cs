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
    using WhMgr.Data.Factories;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Discord;

    public class StatisticReportsHostedService : IHostedService, IDisposable
    {
        // TODO: Make 'StatisticDataFormat' and 'StatHours' configurable
        private const string StatisticDateFormat = "yyyy/MM/dd";
        private const uint StatHours = 24; // Number of hours to check back and find data for statistics
        private const ushort MaxDatabaseTimeoutS = 30;

        #region Variables

        private readonly ILogger<StatisticReportsHostedService> _logger;
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;
        private readonly ConfigHolder _config;
        private readonly IDiscordClientService _discordService;

        #endregion

        #region Constructor

        public StatisticReportsHostedService(
            ILogger<StatisticReportsHostedService> logger,
            ConfigHolder config,
            IDiscordClientService discordService)
        {
            _logger = logger;
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
            _config = config;
            _discordService = discordService;
        }

        #endregion

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Starting daily statistic reporting hosted service...");

            var localZone = TimeZoneInfo.Local;
            var timezone = localZone.StandardName.ConvertIanaToWindowsTimeZone();

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

        public void Dispose()
        {
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        #endregion

        private async void OnMidnightTimerTimeReached(object sender, TimeReachedEventArgs e)
        {
            _logger.LogInformation($"Midnight timer triggered, starting statistics reporting...");

            foreach (var (guildId, guildConfig) in _config.Instance.Servers)
            {
                if (!_discordService.DiscordClients.ContainsKey(guildId))
                {
                    continue;
                }

                var client = _discordService.DiscordClients[guildId];
                var dailyStatsConfig = guildConfig.DailyStats;
                if (dailyStatsConfig?.ShinyStats?.Enabled ?? false)
                {
                    _logger.Information($"Starting daily shiny stats posting for guild '{guildId}'...");
                    await PostShinyStatsAsync(guildId, _config.Instance, client);
                    _logger.Information($"Finished daily shiny stats posting for guild '{guildId}'.");
                }

                if (dailyStatsConfig?.IVStats?.Enabled ?? false) // TODO: Rename to HundoStats
                {
                    _logger.Information($"Starting daily hundo stats posting for guild '{guildId}'...");
                    await PostHundoStatsAsync(guildId, _config.Instance, client);
                    _logger.Information($"Finished daily hundo stats posting for guild '{guildId}'.");
                }

                // TODO: Implement custom IV statistics reporting

                _logger.Information($"Finished daily stats posting for guild '{guildId}'...");
            }

            _logger.Information($"Finished daily stats reporting for all guilds.");
        }

        public static async Task PostShinyStatsAsync(ulong guildId, Config config, DiscordClient client)
        {
            if (!config.Servers.ContainsKey(guildId))
            {
                // Guild not configured
                Console.WriteLine(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"));
                return;
            }

            var server = config.Servers[guildId];
            var statsConfig = server.DailyStats?.ShinyStats;
            if (!(statsConfig?.Enabled ?? false))
            {
                // Shiny statistics reporting not enabled
                Console.WriteLine($"Skipping shiny stats posting for guild '{guildId}', reporting not enabled.");
                return;
            }

            if (!client.Guilds.ContainsKey(guildId))
            {
                // Discord client not in specified guild
                Console.WriteLine($"Discord client is not in guild '{guildId}'");
                return;
            }

            var guild = client.Guilds[guildId];
            var channelId = statsConfig?.ChannelId ?? 0;
            if (!guild.Channels.ContainsKey(channelId) || channelId == 0)
            {
                // Discord channel does not exist in guild
                Console.WriteLine($"Channel with ID '{channelId}' does not exist in guild '{guild.Name}' ({guildId})");
                return;
            }

            var statsChannel = await client.GetChannelAsync(channelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {channelId} to post shiny stats, are you sure it exists?");
                return;
            }

            if (statsConfig?.ClearMessages ?? false)
            {
                Console.WriteLine($"Starting shiny statistics channel message clearing for channel '{channelId}' in guild '{guildId}'...");
                await client.DeleteMessagesAsync(channelId);
            }

            var stats = await GetShinyStatsAsync(config.Database.Scanner.ToString());
            if ((stats?.Count ?? 0) == 0)
            {
                Console.WriteLine($"Failed to get shiny stats from database, returned 0 entries.");
                return;
            }

            var sorted = stats.Keys.ToList();
            sorted.Sort();
            if (sorted.Count > 0)
            {
                var date = DateTime.Now.Subtract(TimeSpan.FromHours(StatHours)).ToLongDateString();
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_TITLE").FormatText(new { date }));
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("SHINY_STATS_NEWLINE"));
            }

            foreach (var pokemonId in sorted)
            {
                if (pokemonId == 0)
                    continue;

                var pkmnName = Translator.Instance.GetPokemonName(pokemonId);
                var pkmnStats = stats[pokemonId];
                var chance = pkmnStats.Shiny == 0 || pkmnStats.Total == 0
                    ? 0
                    : Convert.ToInt32(pkmnStats.Total / pkmnStats.Shiny);
                var message = chance == 0
                    ? "SHINY_STATS_MESSAGE"
                    : "SHINY_STATS_MESSAGE_WITH_RATIO";
                await statsChannel.SendMessageAsync(Translator.Instance.Translate(message).FormatText(new
                {
                    pokemon = pkmnName,
                    id = pokemonId,
                    shiny = pkmnStats.Shiny.ToString("N0"),
                    total = pkmnStats.Total.ToString("N0"),
                    chance,
                }));
                Thread.Sleep(500);
            }

            var total = stats[0];
            var totalRatio = total.Shiny == 0 || total.Total == 0
                ? 0
                : Convert.ToInt32(total.Total / total.Shiny);

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
                Console.WriteLine(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"));
                return;
            }

            var server = config.Servers[guildId];
            var statsConfig = server.DailyStats?.IVStats; // TODO: Rename IVStats to HundoStats
            if (!(statsConfig?.Enabled ?? false))
            {
                // Hundo statistics reporting not enabled
                Console.WriteLine($"Skipping hundo stats posting for guild '{guildId}', reporting not enabled.");
                return;
            }

            if (!client.Guilds.ContainsKey(guildId))
            {
                // Discord client not in specified guild
                Console.WriteLine($"Discord client is not in guild '{guildId}'");
                return;
            }

            var guild = client.Guilds[guildId];
            var channelId = statsConfig.ChannelId;
            if (!guild.Channels.ContainsKey(channelId))
            {
                // Discord channel does not exist in guild
                Console.WriteLine($"Channel with ID '{channelId}' does not exist in guild '{guild.Name}' ({guildId})");
                return;
            }

            var statsChannel = await client.GetChannelAsync(channelId);
            if (statsChannel == null)
            {
                Console.WriteLine($"Failed to get channel id {channelId} to post hundo stats, are you sure it exists?");
                return;
            }

            if (statsConfig?.ClearMessages ?? false)
            {
                Console.WriteLine($"Starting hundo statistics channel message clearing for channel '{channelId}' in guild '{guildId}'...");
                await client.DeleteMessagesAsync(channelId);
            }

            var stats = await GetHundoStatsAsync(config.Database.Scanner.ToString());
            if ((stats?.Count ?? 0) == 0)
            {
                Console.WriteLine($"Failed to get hundo stats from database, returned 0 entries.");
                return;
            }

            var sorted = stats.Keys.ToList();
            sorted.Sort();
            if (sorted.Count > 0)
            {
                var date = DateTime.Now.Subtract(TimeSpan.FromHours(StatHours)).ToLongDateString();
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("HUNDO_STATS_TITLE").FormatText(new { date }));
                await statsChannel.SendMessageAsync(Translator.Instance.Translate("HUNDO_STATS_NEWLINE"));
            }

            foreach (var pokemonId in sorted)
            {
                if (pokemonId == 0)
                    continue;

                //var pkmn = GameMaster.Instance.Pokedex[pokemon];
                var pkmnName = Translator.Instance.GetPokemonName(pokemonId);
                var pkmnStats = stats[pokemonId];
                var chance = pkmnStats.Count == 0 || pkmnStats.Total == 0
                    ? 0
                    : Convert.ToInt32(pkmnStats.Total / pkmnStats.Count);
                var message = chance == 0
                    ? "HUNDO_STATS_MESSAGE"
                    : "HUNDO_STATS_MESSAGE_WITH_RATIO";
                await statsChannel.SendMessageAsync(Translator.Instance.Translate(message).FormatText(new
                {
                    pokemon = pkmnName,
                    id = pokemonId,
                    count = pkmnStats.Count.ToString("N0"),
                    total = pkmnStats.Total.ToString("N0"),
                    chance,
                }));
                Thread.Sleep(500);
            }

            var total = stats[0];
            var totalRatio = total.Count == 0 || total.Total == 0
                ? 0
                : Convert.ToInt32(total.Total / total.Count);

            await statsChannel.SendMessageAsync(Translator.Instance.Translate("HUNDO_STATS_TOTAL_MESSAGE_WITH_RATIO").FormatText(new
            {
                count = total.Count.ToString("N0"),
                total = total.Total.ToString("N0"),
                chance = totalRatio,
            }));
        }

        internal static async Task<Dictionary<uint, ShinyPokemonStats>> GetShinyStatsAsync(string connectionString)
        {
            var list = new Dictionary<uint, ShinyPokemonStats>
            {
                // Index 0 will hold our overall shiny statistics for the day
                { 0, new ShinyPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using var ctx = DbContextFactory.CreateMapContext(connectionString);
                ctx.Database.SetCommandTimeout(MaxDatabaseTimeoutS); // 30 seconds timeout
                var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(StatHours)).ToString(StatisticDateFormat);
                var pokemonShiny = (await ctx.PokemonStatsShiny.ToListAsync())
                    .Where(stat => stat.Date.ToString(StatisticDateFormat) == yesterday)
                    .ToList();
                var pokemonIV = (await ctx.PokemonStatsIV.ToListAsync())
                    .Where(stat => stat.Date.ToString(StatisticDateFormat) == yesterday)?
                    .ToDictionary(stat => stat.PokemonId);

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

        internal static async Task<Dictionary<uint, HundoPokemonStats>> GetHundoStatsAsync(string connectionString)
        {
            var list = new Dictionary<uint, HundoPokemonStats>
            {
                // Index 0 will hold our overall statistics for the day
                { 0, new HundoPokemonStats { PokemonId = 0 } }
            };
            try
            {
                using var ctx = DbContextFactory.CreateMapContext(connectionString);
                ctx.Database.SetCommandTimeout(MaxDatabaseTimeoutS);
                var yesterday = DateTime.Now.Subtract(TimeSpan.FromHours(StatHours)).ToString(StatisticDateFormat);
                var pokemonHundo = (await ctx.PokemonStatsHundo.ToListAsync())
                    .Where(stat => stat.Date.ToString(StatisticDateFormat) == yesterday)
                    .ToList();
                var pokemonIV = (await ctx.PokemonStatsIV.ToListAsync())
                    .Where(stat => stat.Date.ToString(StatisticDateFormat) == yesterday)?
                    .ToDictionary(stat => stat.PokemonId);

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