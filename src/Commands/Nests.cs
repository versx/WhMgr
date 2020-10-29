namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Geofence;
    using WhMgr.Net.Models;
    using WhMgr.Utilities;
    using WhMgr.Data.Factories;

    public class Nests
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("NESTS", Program.LogLevel);

        private readonly Dependencies _dep;

        public Nests(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("nests"),
            Description(""),
            RequirePermissions(Permissions.KickMembers)
        ]
        public async Task PostNestsAsync(CommandContext ctx)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));

            if (!_dep.WhConfig.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _dep.WhConfig.Servers[guildId];
            var channelId = server.NestsChannelId;
            var channel = await ctx.Client.GetChannelAsync(channelId);
            if (channel == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NESTS_DISABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var deleted = await ctx.Client.DeleteMessages(channelId);
            if (deleted.Item2 == 0)
            {
                _logger.Warn($"Failed to delete messages in channel: {channelId}");
            }

            var nests = GetNests(_dep.WhConfig.Database.Nests.ToString());
            if (nests == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username));
                return;
            }

            var cities = server.CityRoles.Select(x => x.ToLower());
            for (var i = 0; i < nests.Count; i++)
            {
                var nest = nests[i];
                if (nest.Average == 0)
                    continue;

                try
                {
                    var eb = GenerateNestMessage(guildId, ctx.Client, nest);
                    var geofences = _dep.Whm.Geofences.Values.ToList();
                    var geofence = GeofenceService.GetGeofence(geofences, new Location(nest.Latitude, nest.Longitude));
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                        continue;
                    }
                    if (!cities.Contains(geofence.Name.ToLower()))
                        continue;

                    await channel.SendMessageAsync(embed: eb);
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public DiscordEmbed GenerateNestMessage(ulong guildId, DiscordClient client, Nest nest)
        {
            var alertMessageType = AlertMessageType.Nests;
            var alertMessage = /*alarm?.Alerts[alertMessageType] ??*/ AlertMessage.Defaults[alertMessageType]; // TODO: Add nestAlert config option
            var server = _dep.WhConfig.Servers[guildId];
            var pokemonImageUrl = IconFetcher.Instance.GetPokemonIcon(server.IconStyle, nest.PokemonId);
            var properties = GetProperties(client.Guilds[guildId], nest, pokemonImageUrl);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alertMessage.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alertMessage.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alertMessage.ImageUrl, properties),
                ThumbnailUrl = DynamicReplacementEngine.ReplaceText(alertMessage.IconUrl, properties),
                Description = DynamicReplacementEngine.ReplaceText(alertMessage.Content, properties),
                Color = DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = DynamicReplacementEngine.ReplaceText(alertMessage.Footer?.Text ?? client.Guilds[guildId]?.Name ?? DateTime.Now.ToString(), properties),
                    IconUrl = DynamicReplacementEngine.ReplaceText(alertMessage.Footer?.IconUrl ?? client.Guilds[guildId]?.IconUrl ?? string.Empty, properties)
                }
            };
            return eb.Build();
        }

        public IReadOnlyDictionary<string, string> GetProperties(DiscordGuild guild, Nest nest, string pokemonImageUrl)
        {
            var pkmnInfo = MasterFile.GetPokemon(nest.PokemonId, 0);
            var pkmnImage = pokemonImageUrl;
            var nestName = nest.Name ?? "Unknown";
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons();
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons() : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var gmapsLink = string.Format(Strings.GoogleMaps, nest.Latitude, nest.Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, nest.Latitude, nest.Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, nest.Latitude, nest.Longitude);
            var scannerMapsLink = string.Format(_dep.WhConfig.Urls.ScannerMap, nest.Latitude, nest.Longitude);
            var templatePath = Path.Combine(_dep.WhConfig.StaticMaps.TemplatesFolder, _dep.WhConfig.StaticMaps.Nests.TemplateFile);
            var staticMapLink = Utils.GetStaticMapsUrl(templatePath, _dep.WhConfig.Urls.StaticMap, _dep.WhConfig.StaticMaps.Nests.ZoomLevel, nest.Latitude, nest.Longitude, pkmnImage, null, _dep.OsmManager.GetNest(nest.Name)?.FirstOrDefault());
            var geofences = _dep.Whm.Geofences.Values.ToList();
            var geofence = GeofenceService.GetGeofence(geofences, new Location(nest.Latitude, nest.Longitude));
            var city = geofence?.Name ?? "Unknown";
            Location address;
            if (!string.IsNullOrEmpty(_dep.WhConfig.GoogleMapsKey))
            {
                address = Utils.GetGoogleAddress(city, nest.Latitude, nest.Longitude, _dep.WhConfig.GoogleMapsKey);
            }
            else
            {
                address = Utils.GetNominatimAddress(city, nest.Latitude, nest.Longitude, _dep.WhConfig.NominatimEndpoint);
            }

            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "pkmn_id", Convert.ToString(nest.PokemonId) },
                { "pkmn_id_3", nest.PokemonId.ToString("D3") },
                { "pkmn_name", pkmnInfo?.Name },
                { "pkmn_img_url", pkmnImage },
                { "avg_spawns", Convert.ToString(nest.Average) },
                { "nest_name", nestName },
                { "type_1", Convert.ToString(type1) },
                { "type_2", Convert.ToString(type2) },
                { "type_1_emoji", type1Emoji },
                { "type_2_emoji", type2Emoji },
                { "types", $"{type1} | {type2}" },
                { "types_emojis", typeEmojis },

                //Location properties
                { "geofence", city },
                { "lat", Convert.ToString(nest.Latitude) },
                { "lng", Convert.ToString(nest.Longitude) },
                { "lat_5", Convert.ToString(Math.Round(nest.Latitude, 5)) },
                { "lng_5", Convert.ToString(Math.Round(nest.Longitude, 5)) },

                //Location links
                { "tilemaps_url", staticMapLink },
                { "gmaps_url", gmapsLink },
                { "applemaps_url", appleMapsLink },
                { "wazemaps_url", wazeMapsLink },
                { "scanmaps_url", scannerMapsLink },

                { "address", address?.Address },

                // Discord Guild properties
                { "guild_name", guild?.Name },
                { "guild_img_url", guild?.IconUrl },

                { "date_time", DateTime.Now.ToString() },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        [
            Command("list-nests"),
            Description("")
        ]
        public async Task ListNestsAsync(CommandContext ctx, string pokemon = null)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _dep.WhConfig.Servers.ContainsKey(x));

            var pokeId = pokemon.PokemonIdFromName();
            if (pokeId == 0)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_POKEMON_ID_OR_NAME").FormatText(ctx.User.Username, pokemon), DiscordColor.Red);
                return;
            }

            var pkmn = MasterFile.GetPokemon(pokeId, 0);
            var eb = new DiscordEmbedBuilder
            {
                Title = $"Local {pkmn.Name} Nests",
                Color = DiscordColor.Blurple,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };

            var nests = GetNestsByPokemon(_dep.WhConfig.Database.Nests.ToString())?.Where(x => x.Key == pokeId);
            if (nests == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var cities = _dep.WhConfig.Servers[guildId].CityRoles.Select(x => x.ToLower()).ToList();
            var groupedNests = GroupNests(nests);
            foreach (var nest in groupedNests)
            {
                var sb = new StringBuilder();
                foreach (var gn in nest.Value)
                {
                    if (gn.Average == 0)
                        continue;

                    var geofence = _dep.Whm.GetGeofence(gn.Latitude, gn.Longitude);
                    if (!cities.Contains(geofence?.Name))
                        continue;

                    sb.AppendLine($"[{gn.Name}]({string.Format(Strings.GoogleMaps, gn.Latitude, gn.Longitude)}) Avg/h: {gn.Average:N0}");
                }
                eb.AddField($"{nest.Key}", sb.ToString(), true);
            }
            if (eb.Fields.Count == 0)
            {
                eb.Description = $"{ctx.User.Username} No local nests found for `{pkmn.Name}`.";
                eb.Color = DiscordColor.Yellow;
            }

            await ctx.RespondAsync(embed: eb);
        }

        /// <summary>
        /// Group nests based on city name
        /// </summary>
        /// <param name="nests"></param>
        /// <returns></returns>
        private Dictionary<string, List<Nest>> GroupNests(IEnumerable<KeyValuePair<int, List<Nest>>> nests)
        {
            var dict = new Dictionary<string, List<Nest>>();
            foreach (var nest in nests)
            {
                foreach (var nest2 in nest.Value)
                {
                    var geofences = _dep.Whm.Geofences.Values.ToList();
                    var geofence = GeofenceService.GetGeofence(geofences, new Location(nest2.Latitude, nest2.Longitude));
                    if (geofence == null)
                    {
                        _logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                        continue;
                    }

                    if (dict.ContainsKey(geofence.Name))
                    {
                        dict[geofence.Name].Add(nest2);
                    }
                    else
                    {
                        dict.Add(geofence.Name, new List<Nest> { nest2 });
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Group nests based on Pokemon
        /// </summary>
        /// <param name="nestsConnectionString"></param>
        /// <returns></returns>
        public static Dictionary<int, List<Nest>> GetNestsByPokemon(string nestsConnectionString = null)
        {
            if (string.IsNullOrEmpty(nestsConnectionString))
                return null;

            try
            {
                var nests = GetNests(nestsConnectionString);
                var dict = new Dictionary<int, List<Nest>>();
                for (var i = 0; i < nests.Count; i++)
                {
                    var nest = nests[i];
                    if (dict.ContainsKey(nest.PokemonId))
                    {
                        dict[nest.PokemonId].Add(nest);
                        continue;
                    }

                    dict.Add(nest.PokemonId, new List<Nest> { nest });
                }
                //var dict = nests.ToDictionary(x => x.PokemonId, x => x);
                return dict;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        /// <summary>
        /// Get a list of nests from the database
        /// </summary>
        /// <param name="nestsConnectionString"></param>
        /// <returns></returns>
        public static List<Nest> GetNests(string nestsConnectionString = null)
        {
            if (string.IsNullOrEmpty(nestsConnectionString))
                return null;

            try
            {
                using (var db = DbContextFactory.CreateManualDbContext(nestsConnectionString))
                {
                    return db.Nests.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }
    }
}