namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Text.Json.Serialization;

    using DSharpPlus.Entities;
    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;
    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Utilities;
    using System.Linq;
    using WhMgr.Configuration;

    [Table("pokemon")]
    public sealed class PokemonData : IWebhookData
    {
        #region Properties

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
        ]
        public uint Id { get; set; }

        [
            JsonPropertyName("cp"),
            Column("cp"),
        ]
        public uint? CP { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public string IV
        {
            get
            {
                if (Attack == null || Defense == null || Stamina == null)
                {
                    return "?";
                }
                return Math.Round(((Attack ?? 0) + (Defense ?? 0) + (Stamina ?? 0)) * 100.0 / 45.0, 1) + "%";
            }
        }

        [
            JsonIgnore,
            NotMapped,
        ]
        public string IVRounded
        {
            get
            {
                if (Attack == null || Defense == null || Stamina == null)
                {
                    return "?";
                }
                return Math.Round((double)((Attack ?? 0) + (Defense ?? 0) + (Stamina ?? 0)) * 100 / 45) + "%";
            }
        }

        [
            JsonPropertyName("individual_stamina"),
            Column("sta_iv"),
        ]
        public ushort? Stamina { get; set; }

        [
            JsonPropertyName("individual_attack"),
            Column("atk_iv"),
        ]
        public ushort? Attack { get; set; }

        [
            JsonPropertyName("individual_defense"),
            Column("def_iv"),
        ]
        public ushort? Defense { get; set; }

        [
            JsonPropertyName("gender"),
            Column("gender"),
        ]
        public Gender Gender { get; set; }

        [
            JsonPropertyName("costume"),
            Column("costume"),
        ]
        public uint Costume { get; set; }

        [
            JsonPropertyName("pokemon_level"),
            Column("level"),
        ]
        public ushort? Level { get; set; }

        [
            JsonPropertyName("latitude"),
            Column("lat"),
        ]
        public double Latitude { get; set; }

        [
            JsonPropertyName("longitude"),
            Column("lon"),
        ]
        public double Longitude { get; set; }

        [
            JsonPropertyName("move_1"),
            Column("move_1"),
        ]
        public ushort? FastMove { get; set; }

        [
            JsonPropertyName("move_2"),
            Column("move_2"),
        ]
        public ushort? ChargeMove { get; set; }

        [
            JsonPropertyName("height"),
            Column("size"),
        ]
        public double? Height { get; set; }

        [
            JsonPropertyName("weight"),
            Column("weight"),
        ]
        public double? Weight { get; set; }

        [
            JsonPropertyName("encounter_id"),
            Column("id"),
        ]
        public string EncounterId { get; set; }

        [
            JsonPropertyName("spawnpoint_id"),
            //Column("spawn_id"),
            NotMapped,
        ]
        public string SpawnpointId { get; set; }

        [
            JsonPropertyName("disappear_time"),
            Column("expire_timestamp"),
        ]
        public long DisappearTime { get; set; }

        [
            JsonPropertyName("disappear_time_verified"),
            Column("expire_timestamp_verified"),
        ]
        public bool DisappearTimeVerified { get; set; }

        [
            JsonPropertyName("first_seen"),
            Column("first_seen_timestamp"),
        ]
        public long FirstSeen { get; set; }

        [
            JsonPropertyName("last_modified_time"),
            Column("changed"),
        ]
        public long LastModified { get; set; }

        [
            JsonPropertyName("pokestop_id"),
            Column("pokestop_id"),
        ]
        public string PokestopId { get; set; }

        [
            JsonPropertyName("weather"),
            Column("weather"),
        ]
        public WeatherCondition? Weather { get; set; }

        [
            JsonPropertyName("form"),
            Column("form"),
        ]
        public uint FormId { get; set; }

        [
            JsonPropertyName("shiny"),
            Column("shiny"),
        ]
        public bool? Shiny { get; set; }

        [
            JsonPropertyName("username"),
            Column("username"),
        ]
        public string Username { get; set; }

        [
            JsonPropertyName("updated"),
            Column("updated"),
        ]
        public long Updated { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime DespawnTime { get; private set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public TimeSpan SecondsLeft { get; private set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime FirstSeenTime { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime LastModifiedTime { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime UpdatedTime { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public PokemonSize? Size
        {
            get
            {
                if (Height == null || Weight == null)
                {
                    return null;
                }
                return Id.GetSize(Height ?? 0, Weight ?? 0);
            }
        }

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool IsDitto => Id == 132;

        [
            JsonPropertyName("display_pokemon_id"),
            Column("display_pokemon_id"),
        ]
        public uint? DisplayPokemonId { get; set; }

        #region PvP

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool MatchesGreatLeague => GreatLeague?.Exists(x =>
            // Check if stat rank is less than or equal to the max great league rank stat desired
            x.Rank <= Strings.MaximumLeagueRank &&
            // Check if stat CP is greater than or equal to min great league CP
            x.CP >= Strings.MinimumGreatLeagueCP &&
            // Check if stat CP is less than or equal to max great league CP
            x.CP <= Strings.MaximumGreatLeagueCP
        ) ?? false;

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool MatchesUltraLeague => UltraLeague?.Exists(x =>
            // Check if stat rank is less than or equal to the max ultra league rank stat desired
            x.Rank <= Strings.MaximumLeagueRank &&
            // Check if stat CP is greater than or equal to min ultra league CP
            x.CP >= Strings.MinimumUltraLeagueCP &&
            // Check if stat CP is less than or equal to max ultra league CP
            x.CP <= Strings.MaximumUltraLeagueCP
        ) ?? false;


        [
            JsonPropertyName("pvp_rankings_great_league"),
            Column("pvp_rankings_great_league"),
        ]
        public List<PvpRankData> GreatLeague { get; set; }

        [
            JsonPropertyName("pvp_rankings_ultra_league"),
            Column("pvp_rankings_ultra_league"),
        ]
        public List<PvpRankData> UltraLeague { get; set; }

        #endregion

        #region Catch Rates

        [
            JsonPropertyName("capture_1"),
            Column("capture_1"),
        ]
        public double? CatchRate1 { get; set; }

        [
            JsonPropertyName("capture_2"),
            Column("capture_2"),
        ]
        public double? CatchRate2 { get; set; }

        [
            JsonPropertyName("capture_3"),
            Column("capture_3"),
        ]
        public double? CatchRate3 { get; set; }

        #endregion

        [
            JsonPropertyName("is_event"),
            NotMapped,
        ]
        public bool? IsEvent { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool IsMissingStats => Level == null;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="PokemonData"/> class.
        /// </summary>
        public PokemonData()
        {
            GreatLeague = new List<PvpRankData>();
            UltraLeague = new List<PvpRankData>();
        }

        #endregion

        /// <summary>
        /// Set despawn times because .NET doesn't support Unix timestamp
        /// deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetDespawnTime()
        {
            DespawnTime = DisappearTime
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            SecondsLeft = DespawnTime
                .Subtract(DateTime.UtcNow
                    .ConvertTimeFromCoordinates(Latitude, Longitude));

            FirstSeenTime = FirstSeen
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            LastModifiedTime = LastModified
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            UpdatedTime = Updated
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        /// <summary>
        /// Generate a Discord embed Pokemon message
        /// </summary>
        /// <param name="guildId">Guild the notification is for</param>
        /// <param name="client">Discord client</param>
        /// <param name="whConfig">Webhook config</param>
        /// <param name="alarm">Webhook alarm</param>
        /// <param name="city">City the Pokemon was found in</param>
        /// <returns>DiscordEmbedNotification object to send</returns>
        public DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings)
        {
            // If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = IsMissingStats
                ? EmbedMessageType.PokemonMissingStats
                : EmbedMessageType.Pokemon;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.DmEmbeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            settings.ImageUrl = IconFetcher.Instance.GetPokemonIcon(server.IconStyle, Id, FormId, 0, Gender, Costume, false);
            var properties = GetProperties(settings);
            var eb = new DiscordEmbedMessage
            {
                Title = TemplateRenderer.Parse(embed.Title, properties),
                Url = TemplateRenderer.Parse(embed.Url, properties),
                Image = new Discord.Models.DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.ImageUrl, properties),
                },
                Thumbnail = new Discord.Models.DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(embed.Content, properties),
                Color = (
                    MatchesGreatLeague || MatchesUltraLeague
                        ? GetPvPColor(GreatLeague, UltraLeague, MasterFile.Instance.DiscordEmbedColors)
                        : IV.BuildPokemonIVColor(MasterFile.Instance.DiscordEmbedColors)
                    ).Value,
                Footer = new Discord.Models.DiscordEmbedFooter
                {
                    Text = TemplateRenderer.Parse(embed.Footer?.Text, properties),
                    IconUrl = TemplateRenderer.Parse(embed.Footer?.IconUrl, properties)
                }
            };
            var username = TemplateRenderer.Parse(embed.Username, properties);
            var iconUrl = TemplateRenderer.Parse(embed.AvatarUrl, properties);
            var description = TemplateRenderer.Parse(settings.Alarm?.Description, properties);
            return new DiscordWebhookMessage
            {
                Username = username,
                AvatarUrl = iconUrl,
                Content = description,
                Embeds = new List<DiscordEmbedMessage> { eb },
            };
        }

        private dynamic GetProperties(AlarmMessageSettings properties)
        {
            var pkmnInfo = MasterFile.GetPokemon(Id, FormId);
            var pkmnName = Translator.Instance.GetPokemonName(Id);
            var form = Translator.Instance.GetFormName(FormId);
            var costume = Translator.Instance.GetCostumeName(Costume);
            var gender = Gender.GetPokemonGenderIcon();
            var genderEmoji = Gender.GetEmojiIcon("gender", true);
            var level = Level;
            var size = Size?.ToString();
            var weather = Weather?.ToString();
            var hasWeather = Weather.HasValue && Weather != WeatherCondition.None;
            var isWeatherBoosted = pkmnInfo?.IsWeatherBoosted(Weather ?? WeatherCondition.None);
            var weatherEmoji = Weather != WeatherCondition.None ? Weather.GetEmojiIcon("weather", false) : null;
            var move1 = FastMove != null ? Translator.Instance.GetMoveName(FastMove ?? 0) : "Unknown";
            var move2 = ChargeMove != null ? Translator.Instance.GetMoveName(ChargeMove ?? 0) : "Unknown";
            var type1 = pkmnInfo?.Types?[0];
            var type2 = pkmnInfo?.Types?.Count > 1 ? pkmnInfo.Types?[1] : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?[0].GetTypeEmojiIcons();
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? pkmnInfo?.Types?[1].GetTypeEmojiIcons() : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var catchPokemon = IsDitto ? Translator.Instance.GetPokemonName(DisplayPokemonId ?? Id) : pkmnName;
            var isShiny = Shiny ?? false;
            var height = Height != null ? Math.Round(Height ?? 0).ToString() : "";
            var weight = Weight != null ? Math.Round(Weight ?? 0).ToString() : "";

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfig = properties.Config.Instance.StaticMaps[StaticMapType.Pokemon];
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                TemplateName = staticMapConfig.TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = properties.ImageUrl,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? properties.MapDataCache.GetGymsNearby(Latitude, Longitude)
                    : new List<dynamic>(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? properties.MapDataCache.GetPokestopsNearby(Latitude, Longitude)
                    : new List<dynamic>(),
            });
            var staticMapLink = staticMap.GenerateLink();
            var gmapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, scannerMapsLink);
            var address = new Coordinate(properties.City, Latitude, Longitude).GetAddress(properties.Config.Instance);
            var pokestop = properties.MapDataCache.GetPokestop(PokestopId).ConfigureAwait(false)
                                                  .GetAwaiter()
                                                  .GetResult();

            var greatLeagueEmoji = PvpLeague.Great.GetEmojiIcon("league", true);
            var ultraLeagueEmoji = PvpLeague.Ultra.GetEmojiIcon("league", true);
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                pkmn_id = Convert.ToString(Id),
                pkmn_id_3 = Id.ToString("D3"),
                pkmn_name = pkmnName,
                pkmn_img_url = properties.ImageUrl,
                form,
                form_id = Convert.ToString(FormId),
                form_id_3 = FormId.ToString("D3"),
                costume = costume ?? defaultMissingValue,
                costume_id = Convert.ToString(Costume),
                costume_id_3 = Costume.ToString("D3"),
                cp = CP == null ? defaultMissingValue : Convert.ToString(CP),
                lvl = level == null ? defaultMissingValue : Convert.ToString(level),
                gender,
                gender_emoji = genderEmoji,
                size = size ?? defaultMissingValue,
                move_1 = move1 ?? defaultMissingValue,
                move_2 = move2 ?? defaultMissingValue,
                moveset = $"{move1}/{move2}",
                type_1 = type1?.ToString() ?? defaultMissingValue,
                type_2 = type2?.ToString() ?? defaultMissingValue,
                type_1_emoji = type1Emoji,
                type_2_emoji = type2Emoji,
                types = $"{type1} | {type2}",
                types_emoji = typeEmojis,
                atk_iv = Attack == null
                    ? defaultMissingValue
                    : Convert.ToString(Attack),
                def_iv = Defense == null
                    ? defaultMissingValue
                    : Convert.ToString(Defense.ToString()),
                sta_iv = Stamina == null
                    ? defaultMissingValue
                    : Convert.ToString(Stamina.ToString()),
                iv = IV ?? defaultMissingValue,
                iv_rnd = IVRounded ?? defaultMissingValue,
                is_shiny = isShiny,

                // Catch rate properties
                has_capture_rates = CatchRate1.HasValue && CatchRate2.HasValue && CatchRate3.HasValue,
                capture_1 =  CatchRate1.HasValue
                    ? Math.Round(CatchRate1.Value * 100).ToString()
                    : string.Empty,
                capture_2 = CatchRate2.HasValue
                    ? Math.Round(CatchRate2.Value * 100).ToString()
                    : string.Empty,
                capture_3 = CatchRate3.HasValue
                    ? Math.Round(CatchRate3.Value * 100).ToString()
                    : string.Empty,
                capture_1_emoji = CaptureRateType.PokeBall.GetEmojiIcon("capture", false),
                capture_2_emoji = CaptureRateType.GreatBall.GetEmojiIcon("capture", false),
                capture_3_emoji = CaptureRateType.UltraBall.GetEmojiIcon("capture", false),

                // PvP stat properties
                is_great = MatchesGreatLeague,
                is_ultra = MatchesUltraLeague,
                is_pvp = MatchesGreatLeague || MatchesUltraLeague,
                great_league_emoji = greatLeagueEmoji,
                ultra_league_emoji = ultraLeagueEmoji,
                great_league = GetLeagueRanks(PvpLeague.Great),
                ultra_league = GetLeagueRanks(PvpLeague.Ultra),

                // Other properties
                height = height ?? defaultMissingValue,
                weight = weight ?? defaultMissingValue,
                is_ditto = IsDitto,
                original_pkmn_id = Convert.ToString(DisplayPokemonId),
                original_pkmn_id_3 = (DisplayPokemonId ?? 0).ToString("D3"),
                original_pkmn_name = catchPokemon,
                is_weather_boosted = isWeatherBoosted,
                has_weather = hasWeather,
                weather = weather ?? defaultMissingValue,
                weather_emoji = weatherEmoji ?? defaultMissingValue,
                username = Username ?? defaultMissingValue,
                spawnpoint_id = SpawnpointId ?? defaultMissingValue,
                encounter_id = EncounterId ?? defaultMissingValue,

                // Time properties
                despawn_time = DespawnTime.ToString("hh:mm:ss tt"),
                despawn_time_24h = DespawnTime.ToString("HH:mm:ss"),
                despawn_time_verified = DisappearTimeVerified ? "" : "~",
                is_despawn_time_verified = DisappearTimeVerified,
                time_left = SecondsLeft.ToReadableString(true) ?? defaultMissingValue,

                // Location properties
                geofence = properties.City ?? defaultMissingValue,
                lat = Convert.ToString(Latitude),
                lng = Convert.ToString(Longitude),
                lat_5 = Latitude.ToString("0.00000"),
                lng_5 = Longitude.ToString("0.00000"),

                // Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLocationLink,
                applemaps_url = appleMapsLocationLink,
                wazemaps_url = wazeMapsLocationLink,
                scanmaps_url = scannerMapsLocationLink,

                address = address?.Address,

                // Pokestop properties
                near_pokestop = pokestop != null,
                pokestop_id = PokestopId ?? defaultMissingValue,
                pokestop_name = pokestop?.Name ?? defaultMissingValue,
                pokestop_url = pokestop?.Url ?? defaultMissingValue,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Event properties
                is_event = IsEvent.HasValue && IsEvent.Value,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }

        #region PvP

        private List<PvpRankData> GetLeagueRanks(PvpLeague league)
        {
            var list = new List<PvpRankData>();
            if (UltraLeague == null)
            {
                return list;
            }
            var pvpRanks = league == PvpLeague.Ultra ? UltraLeague : GreatLeague;
            var minCp = league == PvpLeague.Ultra ? Strings.MinimumUltraLeagueCP : Strings.MinimumGreatLeagueCP;
            var maxCp = league == PvpLeague.Ultra ? Strings.MaximumUltraLeagueCP : Strings.MaximumGreatLeagueCP;
            for (var i = 0; i < pvpRanks.Count; i++)
            {
                var pvp = pvpRanks[i];
                var withinCpRange = pvp.CP >= minCp && pvp.CP <= maxCp;
                var withinRankRange = pvp.Rank <= Strings.MaximumLeagueRank;
                if (pvp.Rank == 0 || (!withinCpRange && !withinRankRange))
                    continue;

                if (!MasterFile.Instance.Pokedex.ContainsKey(pvp.PokemonId))
                {
                    Console.WriteLine($"Pokemon database does not contain pokemon id {pvp.PokemonId}");
                    continue;
                }
                if (pvp.Rank.HasValue && pvp.Rank.Value <= Strings.MaximumLeagueRank &&
                    pvp.Percentage.HasValue &&
                    pvp.Level.HasValue &&
                    pvp.CP.HasValue && pvp.CP <= maxCp)
                {
                    var name = Translator.Instance.GetPokemonName(pvp.PokemonId);
                    var form = Translator.Instance.GetFormName(pvp.FormId);
                    var pkmnName = string.IsNullOrEmpty(form) ? name : $"{name} ({form})";
                    pvp.Percentage = Math.Round(pvp.Percentage.Value * 100, 2);
                    pvp.PokemonName = pkmnName;
                    list.Add(pvp);
                    //sb.AppendLine($"{rankText} #{pvp.Rank.Value} {pkmnName} {pvp.CP.Value}{cpText} @ L{pvp.Level.Value} {Math.Round(pvp.Percentage.Value * 100, 2)}%");
                }
            }
            list.Sort((a, b) => a.Rank.Value.CompareTo(b.Rank.Value));
            return list;
        }

        private static DiscordColor GetPvPColor(List<PvpRankData> greatLeague, List<PvpRankData> ultraLeague, DiscordEmbedColorsConfig config)
        {
            if (greatLeague != null)
                greatLeague.Sort((x, y) => (x.Rank ?? 0).CompareTo(y.Rank ?? 0));

            if (ultraLeague != null)
                ultraLeague.Sort((x, y) => (x.Rank ?? 0).CompareTo(y.Rank ?? 0));

            var greatRank = greatLeague.FirstOrDefault(x => x.Rank > 0 && x.Rank <= 25 && x.CP >= Strings.MinimumGreatLeagueCP && x.CP <= Strings.MaximumGreatLeagueCP);
            var ultraRank = ultraLeague.FirstOrDefault(x => x.Rank > 0 && x.Rank <= 25 && x.CP >= Strings.MinimumUltraLeagueCP && x.CP <= Strings.MaximumUltraLeagueCP);
            var color = config.Pokemon.PvP.FirstOrDefault(x => ((greatRank?.Rank ?? 0) >= x.Minimum && (greatRank?.Rank ?? 0) <= x.Maximum) || ((ultraRank?.Rank ?? 0) >= x.Minimum && (ultraRank?.Rank ?? 0) <= x.Maximum));
            if (color == null)
            {
                return DiscordColor.White;
            }
            return new DiscordColor(color.Color);
        }

        #endregion
    }
}