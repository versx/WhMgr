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
    using WhMgr.Utilities;

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
            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x));
            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _config.Instance.Servers[guildId];
            var channelId = server.Nests.ChannelId;
            var channel = await ctx.Client.GetChannelAsync(channelId);
            if (channel == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NESTS_DISABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            /*
            var deleted = await ctx.Client.DeleteMessages(channelId);
            if (deleted.Item2 == 0)
            {
                Console.WriteLine($"Failed to delete messages in channel: {channelId}");
            }
            */

            var nests = await GetNests();
            if (nests == null)
            {
                await ctx.RespondEmbed(Translator.Instance.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username));
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

                        var pkmn = MasterFile.GetPokemon(nest.PokemonId, 0);
                        var pkmnName = Translator.Instance.GetPokemonName(pkmn.PokedexId);
                        var gmapsLink = string.Format(Strings.GoogleMaps, nest.Latitude, nest.Longitude);
                        // TODO: Check if possible shiny (emoji)
                        message += $"[**{nest.Name}**]({gmapsLink}): {pkmnName} (#{nest.PokemonId}) {nest.Average:N0} per hour\n";
                        if (message.Length >= Strings.DiscordMaximumMessageLength)
                        {
                            eb.Description = message.Substring(0, Math.Min(message.Length, Strings.DiscordMaximumMessageLength));
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
                var cities = server.Geofences.Select(x => x.Name.ToLower()).ToList();
                for (var i = 0; i < nests.Count; i++)
                {
                    var nest = nests[i];
                    if (nest.Average == 0)
                        continue;

                    try
                    {
                        var eb = GenerateEmbedMessage(guildId, ctx.Client, nest);
                        var geofence = GeofenceService.GetGeofence(server.Geofences, new Coordinate(nest.Latitude, nest.Longitude));
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
            var alertMessage = /*alarm?.Alerts[alertMessageType] ??*/ EmbedMessage.Defaults[alertMessageType]; // TODO: Add nestAlert config option
            var server = _config.Instance.Servers[guildId];
            var pokemonImageUrl = IconFetcher.Instance.GetPokemonIcon(server.IconStyle, nest.PokemonId);
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
                    IconUrl = TemplateRenderer.Parse(alertMessage.Footer?.IconUrl, properties)
                }
            };
            return eb.Build();
        }

        public dynamic GetProperties(DiscordGuild guild, Nest nest, string pokemonImageUrl)
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
            var scannerMapsLink = string.Format(_config.Instance.Urls.ScannerMap, nest.Latitude, nest.Longitude);

            //pkmnImage,
            var osmNest = _osmManager.GetNest(nest.Name)?.FirstOrDefault();
            var polygonPath = OsmManager.MultiPolygonToLatLng(osmNest?.Geometry?.Coordinates, true);
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = _config.Instance.StaticMaps[StaticMapType.Nests].Url,
                TemplateName = _config.Instance.StaticMaps[StaticMapType.Nests].TemplateName,
                Latitude = nest.Latitude,
                Longitude = nest.Longitude,
                SecondaryImageUrl = pokemonImageUrl,
                PolygonPath = polygonPath,
            });
            var staticMapLink = staticMap.GenerateLink();
            var geofence = GeofenceService.GetGeofence(_config.Instance.Servers[guild.Id].Geofences, new Coordinate(nest.Latitude, nest.Longitude));
            var city = geofence?.Name ?? "Unknown";
            //var address = new Coordinate(city, nest.Latitude, nest.Longitude).GetAddress(_config.Instance);

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
                type_1_emoji = type1Emoji,
                type_2_emoji = type2Emoji,
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

                //address = address?.Address,

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
                    new Coordinate(nest.Latitude, nest.Longitude)
                );
                if (geofence == null)
                {
                    // _logger.LogWarn($"Failed to find geofence for nest {nest.Name}.");
                    continue;
                }
                var geofenceName = geofence.Name;
                var server = _config.Instance.Servers[guildId];
                var cities = server.Geofences.Select(x => x.Name.ToLower()).ToList();
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
            using (var ctx = DbContextFactory.CreateManualContext(_config.Instance.Database.Nests.ToString()))
            {
                return await ctx.Nests.ToListAsync();
            }
        }
    }
}