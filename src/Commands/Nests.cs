namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using ServiceStack.OrmLite;

    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Geofence;
    using WhMgr.Net.Models;
    using WhMgr.Utilities;

    public class Nests
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("NESTS");

        private readonly Dependencies _dep;

        public Nests(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("nests"),
            Description("")
        ]
        public async Task PostNestsAsync(CommandContext ctx)
        {
            if (!_dep.WhConfig.Servers.ContainsKey(ctx.Guild.Id))
            {
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NOT_IN_DISCORD_SERVER"), DiscordColor.Red);
                return;
            }

            var server = _dep.WhConfig.Servers[ctx.Guild.Id];
            var channelId = server.NestsChannelId;
            var channel = await ctx.Client.GetChannelAsync(channelId);
            if (channel == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NESTS_DISABLED").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            if (true) //TODO: Config ClearMessages
            {
                var deleted = await ctx.Client.DeleteMessages(channelId);
                if (deleted.Item2 == 0)
                {
                    _logger.Warn($"Failed to delete messages in channel: {channelId}");
                }
            }

            var nests = GetNests(_dep.WhConfig.Database.Nests.ToString());
            if (nests == null)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username));
                return;
            }

            var cities = server.CityRoles.Select(x => x.ToLower());
            for (var i = 0; i < nests.Count; i++)
            {
                var nest = nests[i];
                if (nest.Average == 0)
                    continue;

                //var properties = GetProperties(ctx.Client, nest);
                //var eb = GenerateNestMessage(ctx.Client, _dep.WhConfig, nest, "");

                try
                {
                    var pkmn = MasterFile.GetPokemon(nest.PokemonId, 0);
                    var pkmnImage = nest.PokemonId.GetPokemonImage(_dep.WhConfig.IconStyles[server.IconStyle], PokemonGender.Unset, 0);
                    var type1 = pkmn?.Types?[0];
                    var type2 = pkmn?.Types?.Count > 1 ? pkmn.Types?[1] : PokemonType.None;
                    var type1Emoji = ctx.Client.Guilds.ContainsKey(server.EmojiGuildId) ?
                        pkmn?.Types?[0].GetTypeEmojiIcons(ctx.Client.Guilds[server.EmojiGuildId]) :
                        string.Empty;
                    var type2Emoji = ctx.Client.Guilds.ContainsKey(server.EmojiGuildId) && pkmn?.Types?.Count > 1 ?
                        pkmn?.Types?[1].GetTypeEmojiIcons(ctx.Client.Guilds[server.EmojiGuildId]) :
                        string.Empty;
                    var typeEmojis = $"{type1Emoji} {type2Emoji}";
                    var gmapsLink = string.Format(Strings.GoogleMaps, nest.Latitude, nest.Longitude);
                    var appleMapsLink = string.Format(Strings.AppleMaps, nest.Latitude, nest.Longitude);
                    var wazeMapsLink = string.Format(Strings.WazeMaps, nest.Latitude, nest.Longitude);
                    var staticMapLink = Utils.PrepareStaticMapUrl(_dep.WhConfig.Urls.StaticMap, pkmnImage, nest.Latitude, nest.Longitude, _dep.OsmManager.GetNest(nest.Name)?.FirstOrDefault());
                    var geofences = _dep.Whm.Geofences.Values.ToList();
                    var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest.Latitude, nest.Longitude));
                    if (geofence == null)
                    {
                        //_logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                        continue;
                    }
                    if (!cities.Contains(geofence.Name.ToLower()))
                        continue;


                    var eb = new DiscordEmbedBuilder
                    {
                        Title = $"{geofence?.Name ?? "Unknown"}: {nest.Name}",
                        Color = DiscordColor.Green,
                        Description = $"**Pokemon:** {pkmn.Name}\r\n**Average Spawns:** {nest.Average}/h | **Types:** {typeEmojis}\r\n**[[Google Maps]({gmapsLink})] [[Apple Maps]({appleMapsLink})] [[Waze Maps]({wazeMapsLink})]**",
                        ImageUrl = staticMapLink,
                        Url = gmapsLink,
                        ThumbnailUrl = pkmnImage,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{ctx.Guild?.Name ?? Strings.Creator} | {DateTime.Now}",
                            IconUrl = ctx.Guild?.IconUrl
                        }
                    };

                    await channel.SendMessageAsync(embed: eb);
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        //public DiscordEmbed GenerateNestMessage(DiscordClient client, WhConfig whConfig, Nest nest, /*AlarmObject alarm,*/ string pokemonImageUrl)
        //{
        //    //If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
        //    var alertMessageType = AlertMessageType.Nests;
        //    var alertMessage = alarm?.Alerts[alertMessageType] ?? AlertMessage.Defaults[alertMessageType];
        //    var properties = GetProperties(client, nest);
        //    var eb = new DiscordEmbedBuilder
        //    {
        //        Title = DynamicReplacementEngine.ReplaceText(alertMessage.Title, properties),
        //        Url = DynamicReplacementEngine.ReplaceText(alertMessage.Url, properties),
        //        ImageUrl = DynamicReplacementEngine.ReplaceText(alertMessage.ImageUrl, properties),
        //        ThumbnailUrl = pokemonImageUrl,
        //        Description = DynamicReplacementEngine.ReplaceText(alertMessage.Content, properties),
        //        Color = DiscordColor.Green,
        //        Footer = new DiscordEmbedBuilder.EmbedFooter
        //        {
        //            Text = $"{(client.Guilds.ContainsKey(whConfig.Discord.GuildId) ? client.Guilds[whConfig.Discord.GuildId]?.Name : Strings.Creator)} | {DateTime.Now}",
        //            IconUrl = client.Guilds.ContainsKey(whConfig.Discord.GuildId) ? client.Guilds[whConfig.Discord.GuildId]?.IconUrl : string.Empty
        //        }
        //    };
        //    return eb.Build();
        //}

        //public IReadOnlyDictionary<string, string> GetProperties(DiscordClient client, Nest nest)
        //{
        //    var pkmn = Database.Instance.Pokemon[nest.PokemonId];
        //    var pkmnImage = nest.PokemonId.GetPokemonImage(_dep.WhConfig.Urls.PokemonImage, PokemonGender.Unset, 0);
        //    var nestName = nest.Name ?? "Unknown";
        //    var type1 = pkmn?.Types?[0];
        //    var type2 = pkmn?.Types?.Count > 1 ? pkmn.Types?[1] : PokemonType.None;
        //    var type1Emoji = client.Guilds.ContainsKey(_dep.WhConfig.Discord.EmojiGuildId) ?
        //        pkmn?.Types?[0].GetTypeEmojiIcons(client.Guilds[_dep.WhConfig.Discord.EmojiGuildId]) :
        //        string.Empty;
        //    var type2Emoji = client.Guilds.ContainsKey(_dep.WhConfig.Discord.EmojiGuildId) && pkmn?.Types?.Count > 1 ?
        //        pkmn?.Types?[1].GetTypeEmojiIcons(client.Guilds[_dep.WhConfig.Discord.EmojiGuildId]) :
        //        string.Empty;
        //    var typeEmojis = $"{type1Emoji} {type2Emoji}";
        //    var gmapsLink = string.Format(Strings.GoogleMaps, nest.Latitude, nest.Longitude);
        //    var appleMapsLink = string.Format(Strings.AppleMaps, nest.Latitude, nest.Longitude);
        //    var wazeMapsLink = string.Format(Strings.WazeMaps, nest.Latitude, nest.Longitude);
        //    var staticMapLink = Utils.PrepareStaticMapUrl(_dep.WhConfig.Urls.StaticMap, pkmnImage, nest.Latitude, nest.Longitude);
        //    var geofences = _dep.Whm.Geofences.Values.ToList();
        //    var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest.Latitude, nest.Longitude));
        //    var city = geofence?.Name ?? "Unknown";

        //    var dict = new Dictionary<string, string>
        //    {
        //        //Main properties
        //        { "pkmn_id", Convert.ToString(nest.PokemonId) },
        //        { "pkmn_id_3", nest.PokemonId.ToString("D3") },
        //        { "pkmn_name", pkmn?.Name },
        //        { "avg_spawns", Convert.ToString(nest.Average) },
        //        { "nest_name", nestName },
        //        { "type_1", Convert.ToString(type1) },
        //        { "type_2", Convert.ToString(type2) },
        //        { "type_1_emoji", type1Emoji },
        //        { "type_2_emoji", type2Emoji },
        //        { "types", $"{type1} | {type2}" },
        //        { "types_emoji", typeEmojis },

        //        //Location properties
        //        { "geofence", city },
        //        { "lat", Convert.ToString(nest.Latitude) },
        //        { "lng", Convert.ToString(nest.Longitude) },
        //        { "lat_5", Convert.ToString(Math.Round(nest.Latitude, 5)) },
        //        { "lng_5", Convert.ToString(Math.Round(nest.Longitude, 5)) },

        //        //Location links
        //        { "tilemaps_url", staticMapLink },
        //        { "gmaps_url", gmapsLink },
        //        { "applemaps_url", appleMapsLink },
        //        { "wazemaps_url", wazeMapsLink },

        //        //Misc properties
        //        { "br", "\r\n" }
        //    };
        //    return dict;
        //}

        [
            Command("list-nests"),
            Description("")
        ]
        public async Task ListNestsAsync(CommandContext ctx, string pokemon = null)
        {
            var pokeId = pokemon.PokemonIdFromName();
            if (pokeId == 0)
            {
                await ctx.RespondEmbed(_dep.Language.Translate("NOTIFY_INVALID_POKEMON_ID_OR_NAME").FormatText(ctx.User.Username, pokemon), DiscordColor.Red);
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
                await ctx.RespondEmbed(_dep.Language.Translate("ERROR_NESTS_LIST").FormatText(ctx.User.Username), DiscordColor.Red);
                return;
            }

            var cities = _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.Select(x => x.ToLower()).ToList();
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

                    sb.AppendLine($"[{gn.Name}]({string.Format(Strings.GoogleMaps, gn.Latitude, gn.Longitude)}) Avg/h: {gn.Average.ToString("N0")}");
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

        private Dictionary<string, List<Nest>> GroupNests(IEnumerable<KeyValuePair<int, List<Nest>>> nests)
        {
            var dict = new Dictionary<string, List<Nest>>();
            foreach (var nest in nests)
            {
                foreach (var nest2 in nest.Value)
                {
                    var geofences = _dep.Whm.Geofences.Values.ToList();
                    var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest2.Latitude, nest2.Longitude));
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


        public Dictionary<int, List<Nest>> GetNestsByPokemon(string nestsConnectionString = null)
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

        public List<Nest> GetNests(string nestsConnectionString = null)
        {
            if (string.IsNullOrEmpty(nestsConnectionString))
                return null;

            try
            {
                using (var db = DataAccessLayer.CreateFactory(nestsConnectionString).Open())
                {
                    var nests = db.LoadSelect<Nest>();
                    return nests;
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