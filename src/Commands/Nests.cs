namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using Newtonsoft.Json;
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
        public async Task ListNestsAsync(CommandContext ctx, string pokemon)
        {
            var db = Database.Instance;
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
            var nests = GetNests(_dep.WhConfig.NestsConnectionString)?.Where(x => x.Value.PokemonId == pokeId);
            if (nests == null)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Could not get list of nests from SilphRoad atlas.");
                return;
            }

            var groupedNests = GroupNests(nests);
            foreach (var nest in groupedNests)
            {
                var sb = new StringBuilder();
                foreach (var gn in nest.Value)
                {
                    sb.AppendLine($"[{gn.Name}]({string.Format(Strings.GoogleMaps, gn.Latitude, gn.Longitude)})");
                }
                eb.AddField($"{nest.Key}", sb.ToString(), true);
            }

            if (eb.Fields.Count == 0)
            {
                eb.Description = $"{ctx.User.Username} Could not find any nests for {pkmn.Name}.";
            }

            await ctx.RespondAsync(ctx.User.Mention, false, eb);
        }

        [
             Command("nests-silph"),
             Description("Displays a list of local nests from https://thesilphroad.com/atlas")
         ]
        public async Task NestsAsync(CommandContext ctx, string pokemon)
        {
            var db = Database.Instance;
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
            var nests = GetSilphNests()?.Where(x => x.Value.PokemonId == pokeId);
            if (nests == null)
            {
                await ctx.RespondEmbed($"{ctx.User.Username} Could not get list of nests from SilphRoad atlas.");
                return;
            }

            var groupedNests = GroupNests(nests);
            foreach (var nest in groupedNests)
            {
                var sb = new StringBuilder();
                foreach (var gn in nest.Value)
                {
                    if (!gn.IsNest)
                        continue;

                    sb.AppendLine($"[Google Maps]({string.Format(Strings.GoogleMaps, gn.Latitude, gn.Longitude)}) {(gn.IsVerified ? "Confirmed" : "Unconfirmed")}");
                }
                eb.AddField($"{nest.Key}", sb.ToString(), true);
            }

            if (eb.Fields.Count == 0)
            {
                eb.Description = $"{ctx.User.Username} Could not find any nests for {pkmn.Name}.";
            }

            await ctx.RespondAsync(ctx.User.Mention, false, eb);
        }

        private Dictionary<string, List<LocalMarker>> GroupNests(IEnumerable<KeyValuePair<int, LocalMarker>> nests)
        {
            var dict = new Dictionary<string, List<LocalMarker>>();
            foreach (var nest in nests)
            {
                var geofences = _dep.Whm.Geofences.Values.ToList();
                var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest.Value.Latitude, nest.Value.Longitude));
                if (geofence == null)
                {
                    _logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                    continue;
                }

                if (dict.ContainsKey(geofence.Name))
                {
                    dict[geofence.Name].Add(nest.Value);
                }
                else
                {
                    dict.Add(geofence.Name, new List<LocalMarker> { nest.Value });
                }
            }
            return dict;
        }

        private Dictionary<string, List<Nest>> GroupNests(IEnumerable<KeyValuePair<int, Nest>> nests)
        {
            var dict = new Dictionary<string, List<Nest>>();
            foreach (var nest in nests)
            {
                var geofences = _dep.Whm.Geofences.Values.ToList();
                var geofence = _dep.Whm.GeofenceService.GetGeofence(geofences, new Location(nest.Value.Latitude, nest.Value.Longitude));
                if (geofence == null)
                {
                    _logger.Warn($"Failed to find geofence for nest {nest.Key}.");
                    continue;
                }

                if (dict.ContainsKey(geofence.Name))
                {
                    dict[geofence.Name].Add(nest.Value);
                }
                else
                {
                    dict.Add(geofence.Name, new List<Nest> { nest.Value });
                }
            }
            return dict;
        }

        private byte[] Get(string url, NameValueCollection options)
        {
            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                var data = wc.UploadValues(url, "POST", options);
                return data;
            }
        }

        public Dictionary<int, Nest> GetNests(string nestsConnectionString = null)
        {
            if (string.IsNullOrEmpty(nestsConnectionString))
                return null;

            try
            {
                using (var db = DataAccessLayer.CreateFactory(nestsConnectionString).Open())
                {
                    var nests = db.LoadSelect<Nest>();
                    var dict = nests.ToDictionary(x => x.PokemonId, x => x);
                    return dict;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        private Dictionary<int, LocalMarker> GetSilphNests()
        {
            var nvc = new NameValueCollection
            {
                { "lat1", "34.158245" }, //TODO: Add configurable nest coordinates
                { "lng1", "-117.773597" },
                { "lat2", "33.987064" },
                { "lng2", "-117.523522" },
                { "zoom", "1" },
                { "mapTypes", "1" }, // 1 = NestsH 2 = Historical sightings 3 = Habitats
                { "nestVerificationLevels", "1" }, // 1 = Verified 2 = 1 + Unverified 3 = 1 + 2 + Revoked 4 = Get all nests
                { "nestTypes", "-1" },
                { "center_lat", "34.067859" },
                { "center_lng", "-117.647063" }
            };

            var url = "https://thesilphroad.com/atlas/getLocalNests.json";
            var data = Get(url, nvc);
            if (data == null)
            {
                return null;
            }

            var json = Encoding.Default.GetString(data);
            var obj = JsonConvert.DeserializeObject<dynamic>(json);
            var localMarkers = obj.localMarkers;
            if (localMarkers != null)
            {
                json = JsonConvert.SerializeObject(localMarkers);
                var list = JsonConvert.DeserializeObject<Dictionary<int, LocalMarker>>(json);
                return list;
            }

            return null;
        }
    }

    public class LocalMarker
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("s")]
        public int VerificationLevel { get; set; }

        [JsonProperty("t")]
        public int Unknown { get; set; }

        [JsonProperty("c")]
        public string Unknown2 { get; set; }

        [JsonProperty("lt")]
        public double Latitude { get; set; }

        [JsonProperty("ln")]
        public double Longitude { get; set; }

        [JsonProperty("is_nest")]
        public bool IsNest { get; set; }

        public bool IsVerified => !IsNest || VerificationLevel == 1;
    }
}