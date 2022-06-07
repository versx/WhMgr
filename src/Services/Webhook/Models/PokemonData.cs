namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

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
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Icons;
    using WhMgr.Utilities;

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
        public bool HasPvpRankings => PvpRankings?.Keys.Count > 0;

        [
            JsonPropertyName("pvp"),
            Column("pvp"),
        ]
        public Dictionary<PvpLeague, List<PvpRankData>> PvpRankings { get; set; }

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
            PvpRankings = new Dictionary<PvpLeague, List<PvpRankData>>();
        }

        #endregion

        /// <summary>
        /// Set despawn times because .NET doesn't support Unix timestamp
        /// deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetTimes()
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
        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            // If IV has value then use alarmText if not null otherwise use default. If no stats use default missing stats alarmText
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = IsMissingStats
                ? EmbedMessageType.PokemonMissingStats
                : EmbedMessageType.Pokemon;
            var embed = settings.Alarm?.Embeds[embedType]
                ?? server.Subscriptions?.Embeds?[embedType]
                ?? EmbedMessage.Defaults[embedType];
            settings.ImageUrl = UIconService.Instance.GetPokemonIcon(server.IconStyle, Id, FormId, 0, Gender, Costume, false);
            var properties = await GetPropertiesAsync(settings).ConfigureAwait(false);
            var eb = new DiscordEmbedMessage
            {
                Title = TemplateRenderer.Parse(embed.Title, properties),
                Url = TemplateRenderer.Parse(embed.Url, properties),
                Image = new DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.ImageUrl, properties),
                },
                Thumbnail = new DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(embed.Content, properties),
                Color = (
                    HasPvpRankings
                        ? GameMaster.Instance.DiscordEmbedColors.GetPvPColor(PvpRankings)
                        : IV.BuildPokemonIVColor(GameMaster.Instance.DiscordEmbedColors)
                    ).Value,
                Footer = new DiscordEmbedFooter
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

        private async Task<dynamic> GetPropertiesAsync(AlarmMessageSettings properties)
        {
            var pkmnInfo = GameMaster.GetPokemon(Id, FormId);
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
            var weatherEmoji = Weather != WeatherCondition.None
                ? Weather.GetEmojiIcon("weather", false)
                : null;
            var move1 = FastMove != null
                ? Translator.Instance.GetMoveName(FastMove ?? 0)
                : "Unknown";
            var move2 = ChargeMove != null
                ? Translator.Instance.GetMoveName(ChargeMove ?? 0)
                : "Unknown";

            var type1 = pkmnInfo?.Types?.Count >= 1
                ? pkmnInfo.Types[0]
                : PokemonType.None;
            var type2 = pkmnInfo?.Types?.Count > 1
                ? pkmnInfo.Types[1]
                : PokemonType.None;
            var type1Emoji = pkmnInfo?.Types?.Count >= 1
                ? type1.GetTypeEmojiIcons()
                : string.Empty;
            var type2Emoji = pkmnInfo?.Types?.Count > 1
                ? type2.GetTypeEmojiIcons()
                : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var catchPokemon = IsDitto
                ? Translator.Instance.GetPokemonName(DisplayPokemonId ?? Id)
                : pkmnName;
            var isShiny = Shiny ?? false;
            var height = Height != null
                ? Math.Round(Height ?? 0).ToString()
                : "";
            var weight = Weight != null
                ? Math.Round(Weight ?? 0).ToString()
                : "";

            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
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
                    ? await properties.MapDataCache?.GetGymsNearby(Latitude, Longitude)
                    : new(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? await properties.MapDataCache?.GetPokestopsNearby(Latitude, Longitude)
                    : new(),
            });
            var staticMapLink = staticMap.GenerateLink();
            var urlShortener = new UrlShortener(properties.Config.Instance.ShortUrlApi);
            var gmapsLocationLink = await urlShortener.CreateAsync(gmapsLink);
            var appleMapsLocationLink = await urlShortener.CreateAsync(appleMapsLink);
            var wazeMapsLocationLink = await urlShortener .CreateAsync(wazeMapsLink);
            var scannerMapsLocationLink = await urlShortener .CreateAsync(scannerMapsLink);
            var address = await ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(Latitude, Longitude));
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
                type_1 = type1.ToString() ?? defaultMissingValue,
                type_2 = type2.ToString() ?? defaultMissingValue,
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
                is_pvp = HasPvpRankings,
                great_league_emoji = greatLeagueEmoji,
                ultra_league_emoji = ultraLeagueEmoji,
                has_pvp = HasPvpRankings,
                // TODO: Filter pvp rankings using Strings.Defaults.Pvp settings to remove clutter/useless ranks
                pvp = PvpRankings,

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

                address = address ?? string.Empty,

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
    }
}