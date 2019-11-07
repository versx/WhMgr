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
            //TODO: If delete existing, delete existing

            var channel = await ctx.Client.GetChannelAsync(_dep.WhConfig.NestsChannelId);
            if (channel == null)
            {
                await ctx.RespondAsync($"{ctx.User.Username} Nests disabled.");
                return;
            }

            if (true) //Clear Message
            {
                var deleted = await ctx.Client.DeleteMessages(_dep.WhConfig.NestsChannelId);
            }

            var nests = GetNests(_dep.WhConfig.ConnectionStrings.Nests);
            if (nests == null)
            {
                await ctx.RespondAsync($"{ctx.User.Username} Failed to get nest list.");
                return;
            }

            for (var i = 0; i < nests.Count; i++)
            {
                var nest = nests[i];
                if (nest.Average == 0)
                    continue;

                var pkmn = Database.Instance.Pokemon[nest.PokemonId];
                var pkmnImage = nest.PokemonId.GetPokemonImage(_dep.WhConfig.Urls.PokemonImage, Net.Models.PokemonGender.Unset, 0);
                var type1Emoji = ctx.Client.Guilds.ContainsKey(_dep.WhConfig.Discord.EmojiGuildId) ?
                    pkmn?.Types?[0].GetTypeEmojiIcons(ctx.Client.Guilds[_dep.WhConfig.Discord.EmojiGuildId]) :
                    string.Empty;
                var type2Emoji = ctx.Client.Guilds.ContainsKey(_dep.WhConfig.Discord.EmojiGuildId) && pkmn?.Types?.Count > 1 ?
                    pkmn?.Types?[1].GetTypeEmojiIcons(ctx.Client.Guilds[_dep.WhConfig.Discord.EmojiGuildId]) :
                    string.Empty;
                var typeEmojis = $"{type1Emoji} {type2Emoji}";
                var googleMapsLink = string.Format(Strings.GoogleMaps, nest.Latitude, nest.Longitude);
                var appleMapsLink = string.Format(Strings.AppleMaps, nest.Latitude, nest.Longitude);
                var wazeMapsLink = string.Format(Strings.WazeMaps, nest.Latitude, nest.Longitude);
                var staticMapLink = Utilities.Utils.PrepareStaticMapUrl(_dep.WhConfig.Urls.StaticMap, pkmnImage, nest.Latitude, nest.Longitude);
                var geofences = _dep.Whm.Geofences.Values.ToList();
                var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest.Latitude, nest.Longitude));
                if (geofence == null)
                {
                    //_logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                    continue;
                }

                var eb = new DiscordEmbedBuilder
                {
                    Title = $"{geofence?.Name ?? "Unknown"}: {nest.Name}",
                    Color = DiscordColor.Green,
                    Description = $"**Pokemon:** {pkmn.Name}\r\n**Average Spawns:** {nest.Average}/h | **Types:** {typeEmojis}\r\n**[[Google Maps]({googleMapsLink})] [[Apple Maps]({appleMapsLink})] [[Waze Maps]({wazeMapsLink})]**",
                    ImageUrl = staticMapLink,
                    Url = googleMapsLink,
                    ThumbnailUrl = pkmnImage,                    
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                        IconUrl = ctx.Guild?.IconUrl
                    }
                };

                await channel.SendMessageAsync(null, false, eb);
                System.Threading.Thread.Sleep(100);
            }
        }

        [
            Command("list-nests"),
            Description("")
        ]
        public async Task ListNestsAsync(CommandContext ctx, string pokemon = null)
        {
            var db = Database.Instance;

            if (string.IsNullOrEmpty(pokemon))
            {
                var eb = new DiscordEmbedBuilder
                {
                    Title = $"Local Nests",
                    Color = DiscordColor.Blurple,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                        IconUrl = ctx.Guild?.IconUrl
                    }
                };

                var nests = GetNestsByPokemon(_dep.WhConfig.ConnectionStrings.Nests);
                if (nests == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} Could not get list of nests from nest database.");
                    return;
                }

                var groupedNests = GroupNests(nests);
                foreach (var nest in groupedNests)
                {
                    var sb = new StringBuilder();
                    foreach (var gn in nest.Value)
                    {
                        if (gn.Average == 0)
                            continue;

                        var pkmn = db.Pokemon[gn.PokemonId];
                        sb.AppendLine($"{pkmn.Name} [{gn.Name}]({string.Format(Strings.GoogleMaps, gn.Latitude, gn.Longitude)}) Avg/h: {gn.Average.ToString("N0")}");
                    }
                    var total = sb.ToString();
                    if (eb.Fields.Count < 26)
                    {
                        eb.AddField($"{nest.Key}", total.Substring(0, Math.Min(1024, total.Length)), true);
                    }
                }

                if (eb.Fields.Count == 0)
                {
                    eb.Description = $"{ctx.User.Username} No local nests were found.";
                    eb.Color = DiscordColor.Yellow;
                }

                await ctx.RespondAsync(string.Empty, false, eb);
            }
            else
            {
                var pokeId = pokemon.PokemonIdFromName();
                if (pokeId == 0)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} {pokemon} is not a valid Pokemon id or name.");
                    return;
                }

                var pkmn = db.Pokemon[pokeId];
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

                var nests = GetNestsByPokemon(_dep.WhConfig.ConnectionStrings.Nests)?.Where(x => x.Key == pokeId);
                if (nests == null)
                {
                    await ctx.RespondEmbed($"{ctx.User.Username} Could not get list of nests from nest database.");
                    return;
                }

                var groupedNests = GroupNests(nests);
                foreach (var nest in groupedNests)
                {
                    var sb = new StringBuilder();
                    foreach (var gn in nest.Value)
                    {
                        if (gn.Average == 0)
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

                await ctx.RespondAsync(string.Empty, false, eb);
            }
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
                        //_logger.Warn($"Failed to find geofence for nest {nest.Key}.");
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