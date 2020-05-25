namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

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

    [Alias("pokemon")]
    public sealed class PokemonData
    {
        public const string WebHookHeader = "pokemon";
        public const int TopPvPRanks = 25;

        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("POKEMONDATA");
        private static readonly KeyValuePair<int, double> DefaultRank = new KeyValuePair<int, double>(4096, 0.0);

        private readonly PvpRankCalculator _pvpCalc = new PvpRankCalculator();
        private List<PvPCP> _possibleGreatLeagueCPs;
        private List<PvPCP> _possibleUltraLeagueCPs;

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
                /*
                var iv = GetIV(Attack, Defense, Stamina);
                return iv == -1 ? "?" : iv + "%";
                */
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
        //{
        //    get
        //    {
        //        //if (!int.TryParse(Level, out var level))
        //        //    return false;

        //        var isUnderLevel6 = IsUnderLevel(6);
        //        var isStatsUnder4 = IsStatsBelow4;
        //        //var isUnderLevel31 = IsUnderLevel(31);
        //        //var isAboveLevel30 = IsAboveLevel(30);
        //        //var isWindOrRain = Weather == WeatherType.Windy || Weather == WeatherType.Rain;
        //        //var isWindOrCloudy = Weather == WeatherType.Windy || Weather == WeatherType.Cloudy;
        //        //var isNotBoosted = Weather == WeatherType.None && isAboveLevel30;

        //        var check1 = Weather != WeatherType.None &&
        //               (isUnderLevel6 || isStatsUnder4) &&
        //               //isUnderLevel6 &&
        //               //isStatsUnder4 &&
        //               DittoDisguises.Contains(Id);
        //        //var check2 = (Id == 193 && (isNotBoosted || (isWindOrRain && isUnderLevel6)));
        //        //var check3 = (Id == 193 && isWindOrRain && isUnderLevel31 && IsStatsBelow4);
        //        //var check4 = (Id == 41 && ((Weather == WeatherType.None && isAboveLevel30) || (isWindOrCloudy && isUnderLevel6)));
        //        //var check5 = (Id == 41 && isWindOrCloudy && isUnderLevel31 && IsStatsBelow4);
        //        //var check6 = (Id == 16 || Id == 163 || Id == 276) && Weather == WeatherType.Windy && (isUnderLevel6 || (isUnderLevel31 && IsStatsBelow4));
        //        if (check1)
        //        {
        //            FormId = 0;
        //        }

        //        return check1;// || check2 || check3 || check4 || check5 || check6;

        //        /*
        //        if pokemon_id == 193:
        //            if (is_no_weather_boost and is_above_level_30) or ((is_windy or is_raining) and is_below_level_6):
        //                make_ditto()
        //                return
        //            if (is_windy or is_raining) and is_below_level_31 and stat_below_4:
        //                make_ditto()
        //                return
        //        if pokemon_id == 41:
        //            if (is_no_weather_boost and is_above_level_30) or ((is_windy or is_cloudy) and is_below_level_6):
        //                make_ditto()
        //            elif (is_windy or is_cloudy) and is_below_level_31 and stat_below_4:
        //                make_ditto()
        //        if (pokemon_id == 16 or pokemon_id == 163 or pokemon_id == 276) and is_windy:
        //            if is_below_level_6 or (is_below_level_31 and stat_below_4):
        //                make_ditto()
        //         */
        //    }
        //}

        [
            JsonProperty("display_pokemon_id"),
            Alias("display_pokemon_id")
        ]
        public int? DisplayPokemonId { get; set; }

        #region PvP

        private bool _greatLeagueRankSet = false;
        private KeyValuePair<int, double> _greatLeagueRank = DefaultRank;
        [
            JsonIgnore,
            Ignore
        ]
        public KeyValuePair<int, double> GreatLeagueRank
        {
            get
            {
                if (!_greatLeagueRankSet)
                {
                    if (!int.TryParse(Attack, out var atk))
                        return DefaultRank;

                    if (!int.TryParse(Defense, out var def))
                        return DefaultRank;

                    if (!int.TryParse(Stamina, out var sta))
                        return DefaultRank;

                    if (!int.TryParse(CP, out var cp))
                        return DefaultRank;

                    var best = _pvpCalc.CalculateBestPvPStat(Id, FormId, atk, def, sta, 1500).GetAwaiter().GetResult();
                    if (best == null)
                        return DefaultRank;

                    var rank = _pvpCalc.GetRank(Id, FormId, cp, best).GetAwaiter().GetResult();
                    _greatLeagueRank = rank;
                    _greatLeagueRankSet = true;
                }
                return _greatLeagueRank;
            }
        }

        private bool? _matchesGreatLeague;
        [
            JsonIgnore,
            Ignore
        ]
        public bool MatchesGreatLeague
        {
            get
            {
                if (_matchesGreatLeague == null)
                {
                    var passes = false;
                    for (var i = 0; i < PossibleGreatLeagueCPs?.Count; i++)
                    {
                        var pvpCP = PossibleGreatLeagueCPs[i];
                        var best = _pvpCalc.CalculateBestPvPStat(pvpCP.PokemonId, pvpCP.FormId, pvpCP.Attack, pvpCP.Defense, pvpCP.Stamina, 1500).GetAwaiter().GetResult();
                        if (best == null)
                            continue;

                        var rank = _pvpCalc.GetRank(pvpCP.PokemonId, pvpCP.FormId, pvpCP.CP, best).GetAwaiter().GetResult();
                        if (rank.Key <= TopPvPRanks && rank.Key > 0)
                        {
                            passes = true;
                            break;
                        }
                    }
                    _matchesGreatLeague = passes;
                }
                return _matchesGreatLeague.Value;
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public List<PvPCP> PossibleGreatLeagueCPs
        {
            get
            {
                if (_possibleGreatLeagueCPs == null)
                {
                    if (!int.TryParse(Attack, out var atk))
                        return null;

                    if (!int.TryParse(Defense, out var def))
                        return null;

                    if (!int.TryParse(Stamina, out var sta))
                        return null;

                    if (!int.TryParse(Level, out var lvl))
                        return null;

                    _possibleGreatLeagueCPs = _pvpCalc.CalculatePossibleCPs(Id, FormId, atk, def, sta, lvl, Gender, 1490, 1500).GetAwaiter().GetResult();
                }
                return _possibleGreatLeagueCPs;
            }
        }


        private bool _ultraLeagueRankSet = false;
        private KeyValuePair<int, double> _ultraLeagueRank = DefaultRank;
        [
            JsonIgnore,
            Ignore
        ]
        public KeyValuePair<int, double> UltraLeagueRank
        {
            get
            {
                if (!_ultraLeagueRankSet)
                {
                    if (!int.TryParse(Attack, out var atk))
                        return DefaultRank;

                    if (!int.TryParse(Defense, out var def))
                        return DefaultRank;

                    if (!int.TryParse(Stamina, out var sta))
                        return DefaultRank;

                    if (!int.TryParse(CP, out var cp))
                        return DefaultRank;

                    var best = _pvpCalc.CalculateBestPvPStat(Id, FormId, atk, def, sta, 2500).GetAwaiter().GetResult();
                    if (best == null)
                        return DefaultRank;

                    var rank = _pvpCalc.GetRank(Id, FormId, cp, best).GetAwaiter().GetResult();
                    _ultraLeagueRank = rank;
                    _ultraLeagueRankSet = true;
                }
                return _ultraLeagueRank;
            }
        }

        private bool? _matchesUltraLeague;
        [
            JsonIgnore,
            Ignore
        ]
        public bool MatchesUltraLeague
        {
            get
            {
                if (_matchesUltraLeague == null)
                {
                    var passes = false;
                    for (var i = 0; i < PossibleUltraLeagueCPs?.Count; i++)
                    {
                        var pvpCP = PossibleUltraLeagueCPs[i];
                        var best = _pvpCalc.CalculateBestPvPStat(pvpCP.PokemonId, pvpCP.FormId, pvpCP.Attack, pvpCP.Defense, pvpCP.Stamina, 2500).GetAwaiter().GetResult();
                        if (best == null)
                            continue;

                        var rank = _pvpCalc.GetRank(pvpCP.PokemonId, pvpCP.FormId, pvpCP.CP, best).GetAwaiter().GetResult();
                        if (rank.Key <= TopPvPRanks && rank.Key > 0)
                        {
                            passes = true;
                            break;
                        }
                    }
                    _matchesUltraLeague = passes;
                }
                return _matchesUltraLeague.Value;
            }
        }

        [
            JsonIgnore,
            Ignore
        ]
        public List<PvPCP> PossibleUltraLeagueCPs
        {
            get
            {
                if (_possibleUltraLeagueCPs == null)
                {
                    if (!int.TryParse(Attack, out var atk))
                        return null;

                    if (!int.TryParse(Defense, out var def))
                        return null;

                    if (!int.TryParse(Stamina, out var sta))
                        return null;

                    if (!int.TryParse(Level, out var lvl))
                        return null;

                    _possibleUltraLeagueCPs = _pvpCalc.CalculatePossibleCPs(Id, FormId, atk, def, sta, lvl, Gender, 2490, 2500).GetAwaiter().GetResult();
                }
                return _possibleUltraLeagueCPs;
            }
        }

        #endregion

        [
            JsonIgnore,
            Ignore
        ]
        public bool IsMissingStats => string.IsNullOrEmpty(Level);

        #endregion

        #region Constructor

        public PokemonData()
        {
            //SetDespawnTime(false, false);
            //_top100GreatLeagueRanks = _pvpCalc.CalculateTopRanks(Id, FormId, 1500, TopPvPRanks).GetAwaiter().GetResult();
            //_top100UltraLeagueRanks = _pvpCalc.CalculateTopRanks(Id, FormId, 2500, TopPvPRanks).GetAwaiter().GetResult();
        }

        #endregion

        #region Public Methods

        public void SetDespawnTime(bool enableDST, bool enableLeapYear)
        {
            //TODO: DST config option

            DespawnTime = DisappearTime.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(DespawnTime))
            {
                DespawnTime = DespawnTime.AddHours(1); //DST
            }
            SecondsLeft = DespawnTime.Subtract(DateTime.Now);

            FirstSeenTime = FirstSeen.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(FirstSeenTime))
            {
                FirstSeenTime = FirstSeenTime.AddHours(1); //DST
            }

            LastModifiedTime = LastModified.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(LastModifiedTime))
            {
                LastModifiedTime = LastModifiedTime.AddHours(1);
            }

            UpdatedTime = Updated.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(UpdatedTime))
            {
                UpdatedTime = UpdatedTime.AddHours(1);
            }

            if (enableLeapYear)
            {
                DespawnTime = DespawnTime.Subtract(TimeSpan.FromHours(24));
                FirstSeenTime = FirstSeenTime.Subtract(TimeSpan.FromHours(24));
                LastModifiedTime = LastModifiedTime.Subtract(TimeSpan.FromHours(24));
                UpdatedTime = UpdatedTime.Subtract(TimeSpan.FromHours(24));
                SecondsLeft = SecondsLeft.Subtract(TimeSpan.FromHours(24));
            }
        }

        //public bool IsUnderLevel(int targetLevel)
        //{
        //    return int.TryParse(Level, out var level) && level < targetLevel;
        //}

        //public bool IsAboveLevel(int targetLevel)
        //{
        //    return int.TryParse(Level, out var level) && level >= targetLevel;
        //}

        public async Task<DiscordEmbed> GeneratePokemonMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city, string pokemonImageUrl)
        {
            //If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
            var alertType = IsMissingStats ? AlertMessageType.PokemonMissingStats : AlertMessageType.Pokemon;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = await GetProperties(guildId, client, whConfig, city, pokemonImageUrl);
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
                    Text = $"{(client.Guilds.ContainsKey(guildId) ? client.Guilds[guildId]?.Name : Strings.Creator)} | {DateTime.Now}",
                    IconUrl = client.Guilds.ContainsKey(guildId) ? client.Guilds[guildId]?.IconUrl : string.Empty
                }
            };
            return await Task.FromResult(eb.Build());
        }

        #endregion

        private async Task<IReadOnlyDictionary<string, string>> GetProperties(ulong guildId, DiscordClient client, WhConfig whConfig, string city, string pokemonImageUrl)
        {
            //TODO: Check whConfig.Servers[guildId]

            var pkmnInfo = MasterFile.GetPokemon(Id, FormId);
            var form = Id.GetPokemonForm(FormId.ToString());
            var costume = Id.GetCostume(Costume.ToString());
            var gender = Gender.GetPokemonGenderIcon();
            var level = Level;
            var size = Size?.ToString();
            var weather = Weather?.ToString();
            var weatherEmoji = string.Empty;
            var hasWeather = Weather.HasValue && Weather != WeatherType.None;
            var isWeatherBoosted = pkmnInfo?.IsWeatherBoosted(Weather ?? WeatherType.None);
            if (Weather.HasValue && Strings.WeatherEmojis.ContainsKey(Weather.Value) && Weather != WeatherType.None)
            {
                if (client.Guilds.ContainsKey(guildId) && whConfig.Servers.ContainsKey(guildId))
                {
                    weatherEmoji = Weather.Value.GetWeatherEmojiIcon(client.Guilds[whConfig.Servers[guildId].EmojiGuildId]);//Strings.WeatherEmojis[Weather.Value];
                }
            }
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
            var type1Emoji = client.Guilds.ContainsKey(whConfig.Servers[guildId].EmojiGuildId) ?
                pkmnInfo?.Types?[0].GetTypeEmojiIcons(client.Guilds[whConfig.Servers[guildId].EmojiGuildId]) :
                string.Empty;
            var type2Emoji = client.Guilds.ContainsKey(whConfig.Servers[guildId].EmojiGuildId) && pkmnInfo?.Types?.Count > 1 ?
                pkmnInfo?.Types?[1].GetTypeEmojiIcons(client.Guilds[whConfig.Servers[guildId].EmojiGuildId]) :
                string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var catchPokemon = IsDitto ? MasterFile.Instance.Pokedex[DisplayPokemonId ?? Id] : pkmnInfo;
            var isShiny = Shiny.HasValue ? Shiny.Value : false;
            var height = double.TryParse(Height, out var realHeight) ? Math.Round(realHeight).ToString() : "";
            var weight = double.TryParse(Weight, out var realWeight) ? Math.Round(realWeight).ToString() : "";

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var staticMapLink = Utils.PrepareStaticMapUrl(whConfig.Urls.StaticMap, pokemonImageUrl, Latitude, Longitude);
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
                //Main properties
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

                //PvP stat properties
                { "is_great", Convert.ToString(isGreat) },
                { "is_ultra", Convert.ToString(isUltra) },
                { "is_pvp", Convert.ToString(isPvP) },
                //{ "great_league_stats", greatLeagueStats },
                //{ "ultra_league_stats", ultraLeagueStats },
                { "pvp_stats", pvpStats },

                //Other properties
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

                //Time properties
                { "despawn_time", DespawnTime.ToString("hh:mm:ss tt") },
                { "despawn_time_verified", DisappearTimeVerified ? "" : "~" },
                { "is_despawn_time_verified", Convert.ToString(DisappearTimeVerified) },
                { "time_left", SecondsLeft.ToReadableString(true) ?? defaultMissingValue },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Convert.ToString(Latitude) },
                { "lng", Convert.ToString(Longitude) },
                { "lat_5", Convert.ToString(Math.Round(Latitude, 5)) },
                { "lng_5", Convert.ToString(Math.Round(Longitude, 5)) },

                //Location links
                { "tilemaps_url", staticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },
                { "wazemaps_url", wazeMapsLocationLink },

                //Pokestop properties
                { "near_pokestop", Convert.ToString(pokestop != null) },
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", pokestop?.Name ?? defaultMissingValue },
                { "pokestop_url", pokestop?.Url ?? defaultMissingValue },

                //Misc properties
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
            const int MaxCP = 1500;
            var sb = new StringBuilder();
            var cps = PossibleGreatLeagueCPs;
            if (cps == null)
                return null;
            foreach (var pvpCP in cps)
            {
                var best = await _pvpCalc.CalculateBestPvPStat(pvpCP.PokemonId, pvpCP.FormId, pvpCP.Attack, pvpCP.Defense, pvpCP.Stamina, MaxCP);
                var rank = await _pvpCalc.GetRank(pvpCP.PokemonId, pvpCP.FormId, pvpCP.CP, best);
                if (rank.Key > TopPvPRanks || rank.Key == 0)
                    continue;

                if (!MasterFile.Instance.Pokedex.ContainsKey(pvpCP.PokemonId))
                {
                    _logger.Warn($"Pokemon database doesn't contain pokemon id {pvpCP.PokemonId}");
                    continue;
                }
                var pkmn = MasterFile.Instance.Pokedex[pvpCP.PokemonId];
                var form = pkmn.Forms.ContainsKey(FormId) ? " (" + pkmn.Forms[pvpCP.FormId].Name + ")" : string.Empty;
                sb.AppendLine($"Rank #{rank.Key} {pkmn.Name}{form} {pvpCP.CP}CP @ L{pvpCP.Level} {rank.Value}%");
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
            const int MaxCP = 2500;
            var sb = new StringBuilder();
            var cps = PossibleUltraLeagueCPs;
            if (cps == null)
                return null;
            foreach (var pvpCP in cps)
            {
                var best = await _pvpCalc.CalculateBestPvPStat(pvpCP.PokemonId, pvpCP.FormId, pvpCP.Attack, pvpCP.Defense, pvpCP.Stamina, MaxCP); //Changed to pvpCP from Id, FormId, atk, def, sta
                var rank = await _pvpCalc.GetRank(pvpCP.PokemonId, pvpCP.FormId, pvpCP.CP, best);
                if (rank.Key > TopPvPRanks || rank.Key == 0)
                    continue;

                if (!MasterFile.Instance.Pokedex.ContainsKey(pvpCP.PokemonId))
                {
                    _logger.Warn($"Pokemon database doesn't contain pokemon id {pvpCP.PokemonId}");
                    continue;
                }
                var pkmn = MasterFile.Instance.Pokedex[pvpCP.PokemonId];
                var form = pkmn.Forms.ContainsKey(FormId) ? " (" + pkmn.Forms[pvpCP.FormId].Name + ")" : string.Empty;
                sb.AppendLine($"Rank #{rank.Key} {pkmn.Name}{form} {pvpCP.CP}CP @ L{pvpCP.Level} {rank.Value}%");
            }
            var result = sb.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "**Ultra League:**\r\n" + result;
            }
            return await Task.FromResult(result);
        }

        //private async Task<string> GetLeagueStats(int minCP = 1490, int maxCP = 2500)
        //{
        //    if (!int.TryParse(Attack, out var atk))
        //        return string.Empty;
        //    if (!int.TryParse(Defense, out var def))
        //        return string.Empty;
        //    if (!int.TryParse(Stamina, out var sta))
        //        return string.Empty;
        //    if (!int.TryParse(Level, out var lvl))
        //        return string.Empty;

        //    var sb = new StringBuilder();
        //    var pvpCPs = await _pvpCalc.CalculatePossibleCPs(Id, FormId, atk, def, sta, lvl, Gender.ToString(), minCP, maxCP);
        //    foreach (var pvpCP in pvpCPs)
        //    {
        //        var best = await _pvpCalc.CalculateBestPvPStat(Id, FormId, atk, def, sta, maxCP);
        //        var rank = await _pvpCalc.GetRank(pvpCP.PokemonId, pvpCP.FormId, pvpCP.CP, best);
        //        if (rank.Key > TopPvPRanks)
        //            continue;

        //        if (!MasterFile.Instance.Pokedex.ContainsKey(pvpCP.PokemonId))
        //        {
        //            _logger.Warn($"Pokemon database doesn't contain pokemon id {pvpCP.PokemonId}");
        //            continue;
        //        }
        //        var pkmn = MasterFile.Instance.Pokedex[pvpCP.PokemonId];
        //        var form = pkmn.Forms.ContainsKey(FormId) ? " (" + pkmn.Forms[pvpCP.FormId].Name + ")" : string.Empty;
        //        sb.AppendLine($"Rank #{rank.Key} {pkmn.Name}{form} {pvpCP.CP}CP @ L{pvpCP.Level} {rank.Value}%");
        //    }
        //    var result = sb.ToString();
        //    if (!string.IsNullOrEmpty(result))
        //    {
        //        result = (maxCP == 2500 ? "**Ultra League:**\r\n" : "**Great League:**\r\n") + result;
        //    }
        //    return await Task.FromResult(result);
        //}

        #endregion
    }
}