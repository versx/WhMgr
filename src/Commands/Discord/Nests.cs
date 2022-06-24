namespace WhMgr.Commands.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Factories;
    using WhMgr.Data.Models;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Osm;
    using WhMgr.Services;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Icons;
    using WhMgr.Services.StaticMap;

    public class Nests : BaseCommandModule
    {
        private readonly ConfigHolder _config;
        private readonly OsmManager _osmManager;

        public Nests(
            ConfigHolder config,
            OsmManager osmManager)
        {
            _config = config;
            _osmManager = osmManager;
        }

        [
            Command("nests"),
            Description(""),
            RequirePermissions(Permissions.KickMembers)
        ]
        public async Task PostNestsAsync(CommandContext ctx,
            [Description("")] string args = null)
        {
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(guildId => _config.Instance.Servers.ContainsKey(guildId));
            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _config.Instance.Servers[guildId];
            // Check if nest posting is enabled
            if (!server.Nests.Enabled)
            {
                Console.WriteLine($"Nest reporting disabled...");
                return;
            }

            var channelId = server.Nests.ChannelId;
            var channel = await ctx.Client.GetChannelAsync(channelId);
            if (channel == null)
            {
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NESTS_DISABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var deleted = await ctx.Client.DeleteMessagesAsync(channelId);
            if (deleted.Item2 == 0)
            {
                Console.WriteLine($"Failed to delete messages in channel: {channelId}");
            }

            var nests = await GetNests();
            if (nests == null)
            {
                await ctx.RespondEmbedAsync(Translator.Instance.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username));
                return;
            }

            var postNestAsList = string.Compare(args, "list", true) == 0;
            if (postNestAsList)
            {
                var groupedNests = GroupNests(guildId, nests);
                groupedNests.ToImmutableSortedDictionary();
                var sortedKeys = groupedNests.Keys.ToList();
                sortedKeys.Sort();
                foreach (var key in sortedKeys)
                {
                    var eb = new DiscordEmbedBuilder
                    {
                        Title = key,
                        Description = string.Empty,
                        Color = DiscordColor.Green
                    };
                    var message = string.Empty;
                    foreach (var nest in groupedNests[key])
                    {
                        if (nest.Average < server.Nests.MinimumPerHour)
                            continue;

                        var pkmn = GameMaster.GetPokemon(nest.PokemonId);
                        var pkmnName = Translator.Instance.GetPokemonName(pkmn.PokedexId);
                        var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, nest.Latitude, nest.Longitude);
                        // TODO: Check if possible shiny (emoji)
                        message += $"[**{nest.Name}**]({gmapsLink}): {pkmnName} (#{nest.PokemonId}) {nest.Average:N0} per hour\n";
                        if (message.Length >= Strings.DiscordMaximumMessageLength)
                        {
                            eb.Description = message[..Math.Min(message.Length, Strings.DiscordMaximumMessageLength)];
                            message = string.Empty;
                            await channel.SendMessageAsync(embed: eb);
                            eb = new DiscordEmbedBuilder
                            {
                                Title = key,
                                Description = string.Empty,
                                Color = DiscordColor.Green
                            };
                        }
                    }
                    if (message.Length > 0)
                    {
                        eb.Description = message;
                        message = string.Empty;
                        await channel.SendMessageAsync(embed: eb);
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                var cities = server.Geofences.Select(geofence => geofence.Name.ToLower()).ToList();
                for (var i = 0; i < nests.Count; i++)
                {
                    var nest = nests[i];
                    if (nest.Average == 0)
                        continue;

                    try
                    {
                        var eb = GenerateEmbedMessage(guildId, ctx.Client, nest);
                        var geofence = GeofenceService.GetGeofence(server.Geofences, new Coordinate(nest));
                        if (geofence == null)
                        {
                            //_logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                            continue;
                        }

                        if (!cities.Contains(geofence.Name.ToLower()))
                            continue;

                        if (nest.Average < server.Nests.MinimumPerHour)
                            continue;

                        await channel.SendMessageAsync(embed: eb);
                        Thread.Sleep(200);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                    }
                }
            }
        }

        public DiscordEmbed GenerateEmbedMessage(ulong guildId, DiscordClient client, Nest nest)
        {
            var alertMessageType = EmbedMessageType.Nests;
            var alertMessage = /*alarm?.Embeds[alertMessageType] ??*/ EmbedMessage.Defaults[alertMessageType]; // TODO: Add nestEmbed config option
            var server = _config.Instance.Servers[guildId];
            var pokemonImageUrl = UIconService.Instance.GetPokemonIcon(server.IconStyle, nest.PokemonId);
            var properties = GetProperties(client.Guilds[guildId], nest, pokemonImageUrl);
            var eb = new DiscordEmbedBuilder
            {
                Title = TemplateRenderer.Parse(alertMessage.Title, properties),
                Url = TemplateRenderer.Parse(alertMessage.Url, properties),
                ImageUrl = TemplateRenderer.Parse(alertMessage.ImageUrl, properties),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = TemplateRenderer.Parse(alertMessage.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(alertMessage.Content, properties),
                Color = DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = TemplateRenderer.Parse(alertMessage.Footer?.Text, properties),
                    IconUrl = TemplateRenderer.Parse(alertMessage.Footer?.IconUrl, properties),
                },
            };
            return eb.Build();
        }

        public dynamic GetProperties(DiscordGuild guild, Nest nest, string pokemonImageUrl)
        {
            var config = _config.Instance;
            var pkmnInfo = GameMaster.GetPokemon(nest.PokemonId);
            var pkmnImage = pokemonImageUrl;
            var nestName = nest.Name ?? "Unknown";
            var types = pkmnInfo?.Types;
            var type1 = types?.Count >= 1
                ? types[0]
                : PokemonType.None;
            var type2 = types?.Count > 1
                ? types[1]
                : PokemonType.None;
            var typeEmojis = types?.GetTypeEmojiIcons() ?? string.Empty;
            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, nest.Latitude, nest.Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, nest.Latitude, nest.Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, nest.Latitude, nest.Longitude);
            var scannerMapsLink = string.Format(config.Urls.ScannerMap, nest.Latitude, nest.Longitude);
            var address = ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(nest)).Result;

            var osmNest = _osmManager.GetNest(nest.Name)?.FirstOrDefault();
            var polygonPath = OsmManager.MultiPolygonToLatLng(osmNest?.Geometry?.Coordinates, true);
            var staticMapLink = config.StaticMaps?.GenerateStaticMap(
                StaticMapType.Nests,
                nest,
                pokemonImageUrl,
                null,
                null,
                polygonPath
            );
            var geofence = GeofenceService.GetGeofence(config.Servers[guild.Id].Geofences, new Coordinate(nest));
            var city = geofence?.Name ?? "Unknown";

            var dict = new
            {
                // Main properties
                pkmn_id = nest.PokemonId,
                pkmn_id_3 = nest.PokemonId.ToString("D3"),
                pkmn_name = pkmnInfo?.Name,
                pkmn_img_url = pkmnImage,
                avg_spawns = nest.Average,
                nest_name = nestName,
                type_1 = Convert.ToString(type1),
                type_2 = Convert.ToString(type2),
                types = $"{type1} | {type2}",
                types_emojis = typeEmojis,

                // Location properties
                geofence = city,
                lat = nest.Latitude,
                lng = nest.Longitude,
                lon = nest.Longitude,
                lat_5 = Math.Round(nest.Latitude, 5),
                lng_5 = Math.Round(nest.Longitude, 5),
                lon_5 = Math.Round(nest.Longitude, 5),

                // Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLink,
                applemaps_url = appleMapsLink,
                wazemaps_url = wazeMapsLink,
                scanmaps_url = scannerMapsLink,

                address = address ?? string.Empty,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }

        private Dictionary<string, List<Nest>> GroupNests(ulong guildId, IEnumerable<Nest> nests)
        {
            var dict = new Dictionary<string, List<Nest>>();
            foreach (var nest in nests)
            {
                var geofence = GeofenceService.GetGeofence(
                    _config.Instance.Servers[guildId].Geofences,
                    new Coordinate(nest)
                );
                if (geofence == null)
                {
                    // _logger.LogWarn($"Failed to find geofence for nest {nest.Name}.");
                    continue;
                }
                var geofenceName = geofence.Name;
                var server = _config.Instance.Servers[guildId];
                var cities = server.Geofences.Select(geofence => geofence.Name.ToLower()).ToList();
                if (!cities.Contains(geofenceName.ToLower()))
                    continue;

                if (dict.ContainsKey(geofenceName))
                {
                    dict[geofenceName].Add(nest);
                }
                else
                {
                    dict.Add(geofenceName, new List<Nest> { nest });
                }
                dict[geofenceName].Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            return dict;
        }

        private async Task<List<Nest>> GetNests()
        {
            using var ctx = DbContextFactory.CreateManualContext(_config.Instance.Database.Nests.ToString());
            return await ctx.Nests.ToListAsync();
        }
    }
}