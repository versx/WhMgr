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
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly Dependencies _dep;

        public Nests(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("nests"),
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
                        Text = $"versx | {DateTime.Now}",
                        IconUrl = ctx.Guild?.IconUrl
                    }
                };

                var nests = GetNests(_dep.WhConfig.NestsConnectionString);
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
                        Text = $"versx | {DateTime.Now}",
                        IconUrl = ctx.Guild?.IconUrl
                    }
                };

                var nests = GetNests(_dep.WhConfig.NestsConnectionString)?.Where(x => x.Key == pokeId);
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

        public Dictionary<int, List<Nest>> GetNests(string nestsConnectionString = null)
        {
            if (string.IsNullOrEmpty(nestsConnectionString))
                return null;

            try
            {
                using (var db = DataAccessLayer.CreateFactory(nestsConnectionString).Open())
                {
                    var nests = db.LoadSelect<Nest>();
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
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }
    }
}