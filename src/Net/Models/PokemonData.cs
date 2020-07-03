namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using DotLiquid.Tags;
    using DSharpPlus;
    using DSharpPlus.Entities;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    /// <summary>
    /// RealDeviceMap Pokemon webhook and database model class.
    /// </summary>
    [Alias("pokemon")]
    public sealed class PokemonData
    {
        public const string WebHookHeader = "pokemon";
        public const int MaximumRankPVP = 500;

        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("POKEMONDATA");

        #endregion

        #region Properties

        [
            JsonProperty("pokemon_id"),
            Alias("pokemon_id")
        ]
        public int Id { get; set; }

        [
            JsonProperty("cp"),
            Alias("cp")
        ]
        public string CP { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public string IV
        {
            get
            {
                if (!int.TryParse(Stamina, out int sta) ||
                    !int.TryParse(Attack, out int atk) ||
                    !int.TryParse(Defense, out int def))
                {
                    return "?";
                }

                return Math.Round((sta + atk + def) * 100.0 / 45.0, 1) + "%";
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public string IVRounded
        {
            get
            {
                if (!int.TryParse(Stamina, out int sta) ||
                    !int.TryParse(Attack, out int atk) ||
                    !int.TryParse(Defense, out int def))
                {
                    return "?";
                }

                return Math.Round((double)(sta + atk + def) * 100 / 45) + "%";
            }
        }

        [
            JsonProperty("individual_stamina"),
            Alias("sta_iv")
        ]
        public string Stamina { get; set; }

        [
            JsonProperty("individual_attack"),
            Alias("atk_iv")
        ]
        public string Attack { get; set; }

        [
            JsonProperty("individual_defense"),
            Alias("def_iv")
        ]
        public string Defense { get; set; }

        [
            JsonProperty("gender"),
            Alias("gender")
        ]
        public PokemonGender Gender { get; set; }

        [
            JsonProperty("costume"),
            Alias("costume")
        ]
        public int Costume { get; set; }

        [
            JsonProperty("pokemon_level"),
            Alias("level")
        ]
        public string Level { get; set; }

        [
            JsonProperty("latitude"),
            Alias("lat")
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Alias("lon")
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("move_1"),
            Alias("move_1")
        ]
        public string FastMove { get; set; }

        [
            JsonProperty("move_2"),
            Alias("move_2")
        ]
        public string ChargeMove { get; set; }

        [
            JsonProperty("height"),
            Alias("size")
        ]
        public string Height { get; set; }

        [
            JsonProperty("weight"),
            Alias("weight")
        ]
        public string Weight { get; set; }

        [
            JsonProperty("encounter_id"),
            Alias("id")
        ]
        public string EncounterId { get; set; }

        [
            JsonProperty("spawnpoint_id"),
            Alias("spawn_id")
        ]
        public string SpawnpointId { get; set; }

        [
            JsonProperty("disappear_time"),
            Alias("expire_timestamp")
        ]
        public long DisappearTime { get; set; }

        [
            JsonProperty("disappear_time_verified"),
            Alias("expire_timestamp_verified")
        ]
        public bool DisappearTimeVerified { get; set; }

        [
            JsonProperty("first_seen"),
            Alias("first_seen_timestamp")
        ]
        public long FirstSeen { get; set; }

        [
            JsonProperty("last_modified_time"),
            Alias("changed")
        ]
        public long LastModified { get; set; }

        [
            JsonProperty("pokestop_id"),
            Alias("pokestop_id")
        ]
        public string PokestopId { get; set; }

        [
            JsonProperty("weather"),
            Alias("weather")
        ]
        public WeatherType? Weather { get; set; }

        [
            JsonProperty("form"),
            Alias("form")
        ]
        public int FormId { get; set; }

        [
            JsonProperty("shiny"),
            Alias("shiny")
        ]
        public bool? Shiny { get; set; }

        [
            JsonProperty("username"),
            Alias("username")
        ]
        public string Username { get; set; }

        [
            JsonProperty("updated"),
            Alias("updated")
        ]
        public long Updated { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime DespawnTime { get; private set; }

        [
            JsonIgnore,
            Ignore
        ]
        public TimeSpan SecondsLeft { get; private set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime FirstSeenTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime LastModifiedTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public DateTime UpdatedTime { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public PokemonSize? Size
        {
            get
            {
                if (float.TryParse(Height, out var height) && float.TryParse(Weight, out var weight))
                {
                    return Id.GetSize(height, weight);
                }
                return null;
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public bool IsDitto => Id == 132;

        [
            JsonProperty("display_pokemon_id"),
            Alias("display_pokemon_id")
        ]
        public int? DisplayPokemonId { get; set; }

        #region PvP

        [
            JsonIgnore,
            Ignore
        ]
        public bool MatchesGreatLeague => GreatLeague?.Exists(x => x.Rank <= MaximumRankPVP && x.CP >= Strings.MinimumGreatLeagueCP && x.CP <= Strings.MaximumGreatLeagueCP) ?? false;

        [
            JsonIgnore,
            Ignore
        ]
        public bool MatchesUltraLeague => UltraLeague?.Exists(x => x.Rank <= MaximumRankPVP && x.CP >= Strings.MinimumUltraLeagueCP && x.CP <= Strings.MaximumUltraLeagueCP) ?? false;


        [
            JsonProperty("pvp_rankings_great_league"),
            Ignore
        ]
        public List<PVPRank> GreatLeague { get; set; }

        [
            JsonProperty("pvp_rankings_ultra_league"),
            Ignore
        ]
        public List<PVPRank> UltraLeague { get; set; }

        #endregion

        #region Catch Rates

        [
            JsonProperty("capture_1"),
            Alias("capture_1")
        ]
        public double? CatchRate1 { get; set; }

        [
            JsonProperty("capture_2"),
            Alias("capture_2")
        ]
        public double? CatchRate2 { get; set; }

        [
            JsonProperty("capture_3"),
            Alias("capture_3")
        ]
        public double? CatchRate3 { get; set; }

        #endregion

        [
            JsonIgnore,
            Ignore
        ]
        public bool IsMissingStats => string.IsNullOrEmpty(Level);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="PokemonData"/> class.
        /// </summary>
        public PokemonData()
        {
            GreatLeague = new List<PVPRank>();
            UltraLeague = new List<PVPRank>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set despawn times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        /// <param name="enableDST">Enable Day Light Savings time adjustment.</param>
        /// <param name="enableLeapYear">Enable leap year time adjustment.</param>
        public void SetDespawnTime()
        {

            DespawnTime = DisappearTime.FromUnix();

            SecondsLeft = DespawnTime.Subtract(DateTime.Now);

            FirstSeenTime = FirstSeen.FromUnix();

            LastModifiedTime = LastModified.FromUnix();

        }

        public async Task<DiscordEmbed> GeneratePokemonMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city, string pokemonImageUrl)
        {
            //If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
            var alertType = IsMissingStats ? AlertMessageType.PokemonMissingStats : AlertMessageType.Pokemon;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = await GetProperties(whConfig, city, pokemonImageUrl);
            var mention = DynamicReplacementEngine.ReplaceText(alarm?.Mentions ?? string.Empty, properties);
            var description = DynamicReplacementEngine.ReplaceText(alert.Content, properties);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alert.ImageUrl, properties),
                ThumbnailUrl = pokemonImageUrl,
                Description = mention + description,
                Color = IV.BuildColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{(client.Guilds?[guildId]?.Name ?? Strings.Creator)} | {DateTime.Now}",
                    IconUrl = client.Guilds?[guildId]?.IconUrl ?? string.Empty
                }
            };
            return await Task.FromResult(eb.Build());
        }

        #endregion

        private async Task<IReadOnlyDictionary<string, string>> GetProperties(WhConfig whConfig, string city, string pokemonImageUrl)
        {
            //TODO: Check whConfig.Servers[guildId]

            var pkmnInfo = MasterFile.GetPokemon(Id, FormId);
            var form = Id.GetPokemonForm(FormId.ToString());
            var costume = Id.GetCostume(Costume.ToString());
            var gender = Gender.GetPokemonGenderIcon();
            var level = Level;
            var size = Size?.ToString();
            var weather = Weather?.ToString();
            var hasWeather = Weather.HasValue && Weather != WeatherType.None;
            var isWeatherBoosted = pkmnInfo?.IsWeatherBoosted(Weather ?? WeatherType.None);
            var weatherKey = $"weather_{Convert.ToInt32(Weather ?? WeatherType.None)}";
            var weatherEmoji = MasterFile.Instance.Emojis.ContainsKey(weatherKey) && Weather != WeatherType.None ? (Weather ?? WeatherType.None).GetWeatherEmojiIcon() : string.Empty;
            var move1 = string.Empty;
            var move2 = string.Empty;
            if (int.TryParse(FastMove, out var fastMoveId) && MasterFile.Instance.Movesets.ContainsKey(fastMoveId))
            {
                move1 = MasterFile.Instance.Movesets[fastMoveId].Name;
            }
            if (int.TryParse(ChargeMove, out var chargeMoveId) && MasterFile.Instance.Movesets.ContainsKey(chargeMoveId))
            {
                move2 = MasterFile.Instance.Movesets[chargeMoveId].Name;
            }
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons();
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons() : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var catchPokemon = IsDitto ? MasterFile.Instance.Pokedex[DisplayPokemonId ?? Id] : pkmnInfo;
            var isShiny = Shiny ?? false;
            var height = double.TryParse(Height, out var realHeight) ? Math.Round(realHeight).ToString() : "";
            var weight = double.TryParse(Weight, out var realWeight) ? Math.Round(realWeight).ToString() : "";

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var templatePath = Path.Combine(whConfig.StaticMaps.TemplatesFolder, whConfig.StaticMaps.PokemonTemplateFile);
            var staticMapLink = Utils.GetStaticMapsUrl(templatePath, whConfig.Urls.StaticMap, Latitude, Longitude, pokemonImageUrl);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? wazeMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, wazeMapsLink);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);
            var pokestop = Pokestop.Pokestops.ContainsKey(PokestopId) ? Pokestop.Pokestops[PokestopId] : null;

            var isGreat = MatchesGreatLeague;
            var isUltra = MatchesUltraLeague;
            var isPvP = isGreat || isUltra;
            var pvpStats = await GetPvP();

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                // Main properties
                { "pkmn_id", Convert.ToString(Id) },
                { "pkmn_name", pkmnInfo.Name },
                { "pkmn_img_url", pokemonImageUrl },
                { "form", form },
                { "form_id", Convert.ToString(FormId) },
                { "form_id_3", FormId.ToString("D3") },
                { "costume", costume ?? defaultMissingValue },
                { "costume_id", Convert.ToString(Costume) },
                { "costume_id_3", Costume.ToString("D3") },
                { "cp", CP ?? defaultMissingValue },
                { "lvl", level ?? defaultMissingValue },
                { "gender", gender },
                { "size", size ?? defaultMissingValue },
                { "move_1", move1 ?? defaultMissingValue },
                { "move_2", move2 ?? defaultMissingValue },
                { "moveset", $"{move1}/{move2}" },
                { "type_1", type1?.ToString() ?? defaultMissingValue },
                { "type_2", type2?.ToString() ?? defaultMissingValue },
                { "type_1_emoji", type1Emoji },
                { "type_2_emoji", type2Emoji },
                { "types", $"{type1} | {type2}" },
                { "types_emoji", typeEmojis },
                { "atk_iv", Attack ?? defaultMissingValue },
                { "def_iv", Defense ?? defaultMissingValue },
                { "sta_iv", Stamina ?? defaultMissingValue },
                { "iv", IV ?? defaultMissingValue },
                { "iv_rnd", IVRounded ?? defaultMissingValue },
                { "is_shiny", Convert.ToString(isShiny) },

                // Catch rate properties
                { "has_capture_rates",  Convert.ToString(CatchRate1.HasValue && CatchRate2.HasValue && CatchRate3.HasValue) },
                { "capture_1", CatchRate1.HasValue ? Math.Round(CatchRate1.Value * 100, 2).ToString() : string.Empty },
                { "capture_2", CatchRate2.HasValue ? Math.Round(CatchRate2.Value * 100, 2).ToString() : string.Empty },
                { "capture_3", CatchRate3.HasValue ? Math.Round(CatchRate3.Value * 100, 2).ToString() : string.Empty },
                { "capture_1_emoji", CaptureRateType.PokeBall.GetCaptureRateEmojiIcon() },
                { "capture_2_emoji", CaptureRateType.GreatBall.GetCaptureRateEmojiIcon() },
                { "capture_3_emoji", CaptureRateType.UltraBall.GetCaptureRateEmojiIcon() },

                // PvP stat properties
                { "is_great", Convert.ToString(isGreat) },
                { "is_ultra", Convert.ToString(isUltra) },
                { "is_pvp", Convert.ToString(isPvP) },
                //{ "great_league_stats", greatLeagueStats },
                //{ "ultra_league_stats", ultraLeagueStats },
                { "pvp_stats", pvpStats },

                // Other properties
                { "height", height ?? defaultMissingValue },
                { "weight", weight ?? defaultMissingValue },
                { "is_ditto", Convert.ToString(IsDitto) },
                { "original_pkmn_id", Convert.ToString(DisplayPokemonId) },
                { "original_pkmn_id_3", (DisplayPokemonId ?? 0).ToString("D3") },
                { "original_pkmn_name", catchPokemon?.Name },
                { "is_weather_boosted", Convert.ToString(isWeatherBoosted) },
                { "has_weather", Convert.ToString(hasWeather) },
                { "weather", weather ?? defaultMissingValue },
                { "weather_emoji", weatherEmoji ?? defaultMissingValue },
                { "username", Username ?? defaultMissingValue },
                { "spawnpoint_id", SpawnpointId ?? defaultMissingValue },
                { "encounter_id", EncounterId ?? defaultMissingValue },

                // Time properties
                { "despawn_time", DespawnTime.ToString("hh:mm:ss tt") },
                { "despawn_time_verified", DisappearTimeVerified ? "" : "~" },
                { "is_despawn_time_verified", Convert.ToString(DisappearTimeVerified) },
                { "time_left", SecondsLeft.ToReadableString(true) ?? defaultMissingValue },

                // Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Convert.ToString(Latitude) },
                { "lng", Convert.ToString(Longitude) },
                { "lat_5", Convert.ToString(Math.Round(Latitude, 5)) },
                { "lng_5", Convert.ToString(Math.Round(Longitude, 5)) },

                // Location links
                { "tilemaps_url", staticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },
                { "wazemaps_url", wazeMapsLocationLink },

                // Pokestop properties
                { "near_pokestop", Convert.ToString(pokestop != null) },
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", pokestop?.Name ?? defaultMissingValue },
                { "pokestop_url", pokestop?.Url ?? defaultMissingValue },

                // Misc properties
                { "br", "\r\n" }
            };
            return await Task.FromResult(dict);
        }

        public static double GetIV(string attack, string defense, string stamina)
        {
            if (!int.TryParse(attack, out int atk) ||
                !int.TryParse(defense, out int def) ||
                !int.TryParse(stamina, out int sta))
            {
                return -1;
            }

            return Math.Round((double)(sta + atk + def) * 100 / 45);
        }

        #region PvP

        private async Task<string> GetPvP()
        {
            var great = await GetGreatLeague();
            var ultra = await GetUltraLeague();
            if (!string.IsNullOrEmpty(great) || !string.IsNullOrEmpty(ultra))
            {
                var header = "__**PvP Rank Statistics**__\r\n";
                return await Task.FromResult(header + great + ultra);
            }
            return null;
        }

        private async Task<string> GetGreatLeague()
        {
            var sb = new StringBuilder();
            if (GreatLeague != null)
            {
                for (var i = 0; i < GreatLeague.Count; i++)
                {
                    var pvp = GreatLeague[i];
                    var withinCpRange = pvp.CP >= Strings.MinimumGreatLeagueCP && pvp.CP <= Strings.MaximumGreatLeagueCP;
                    var withinRankRange = pvp.Rank <= MaximumRankPVP;
                    if (pvp.Rank == 0 || (!withinCpRange && !withinRankRange))
                        continue;

                    if (!MasterFile.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                    {
                        _logger.Error($"Pokemon database doesn't contain pokemon id {pvp.PokemonId}");
                        continue;
                    }
                    var pkmn = MasterFile.Instance.Pokedex[pvp.PokemonId];
                    var form = pkmn.Forms.ContainsKey(FormId) ? " (" + pkmn.Forms[pvp.FormId].Name + ")" : string.Empty;
                    if ((pvp.Rank.HasValue && pvp.Rank.Value <= MaximumRankPVP) && pvp.Percentage.HasValue && pvp.Level.HasValue && pvp.CP.HasValue && pvp.CP <= Strings.MaximumGreatLeagueCP)
                    {
                        sb.AppendLine($"Rank #{pvp.Rank.Value} {pkmn.Name}{form} {pvp.CP.Value}CP @ L{pvp.Level.Value} {Math.Round(pvp.Percentage.Value * 100, 2)}%");
                    }
                }
            }
            var result = sb.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "**Great League:**\r\n" + result;
            }
            return await Task.FromResult(result);
        }

        private async Task<string> GetUltraLeague()
        {
            var sb = new StringBuilder();
            if (UltraLeague != null)
            {
                for (var i = 0; i < UltraLeague.Count; i++)
                {
                    var pvp = UltraLeague[i];
                    var withinCpRange = pvp.CP >= Strings.MinimumUltraLeagueCP && pvp.CP <= Strings.MaximumUltraLeagueCP;
                    var withinRankRange = pvp.Rank <= MaximumRankPVP;
                    if (pvp.Rank == 0 || (!withinCpRange && !withinRankRange))
                        continue;

                    if (!MasterFile.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                    {
                        _logger.Warn($"Pokemon database doesn't contain pokemon id {pvp.PokemonId}");
                        continue;
                    }
                    var pkmn = MasterFile.Instance.Pokedex[pvp.PokemonId];
                    var form = pkmn.Forms.ContainsKey(FormId) ? " (" + pkmn.Forms[pvp.FormId].Name + ")" : string.Empty;
                    if ((pvp.Rank.HasValue && pvp.Rank.Value <= MaximumRankPVP) && pvp.Percentage.HasValue && pvp.Level.HasValue && pvp.CP.HasValue && pvp.CP <= Strings.MaximumUltraLeagueCP)
                    {
                        sb.AppendLine($"Rank #{pvp.Rank.Value} {pkmn.Name}{form} {pvp.CP.Value}CP @ L{pvp.Level.Value} {Math.Round(pvp.Percentage.Value * 100, 2)}%");
                    }
                }
            }
            var result = sb.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "**Ultra League:**\r\n" + result;
            }
            return await Task.FromResult(result);
        }

        #endregion
    }

    public enum CaptureRateType
    {
        PokeBall = 1,
        GreatBall,
        UltraBall,
    }
}
