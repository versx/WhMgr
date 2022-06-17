﻿namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;

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
    using WhMgr.Services.StaticMap;
    using WhMgr.Utilities;

    public sealed class RaidData : IWebhookData
    {
        #region Properties

        [JsonPropertyName("gym_id")]
        public string GymId { get; set; }

        [JsonPropertyName("gym_name")]
        public string GymName { get; set; }

        [JsonPropertyName("gym_url")]
        public string GymUrl { get; set; }

        [JsonPropertyName("pokemon_id")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("team_id")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonPropertyName("level")]
        public ushort Level { get; set; }

        [JsonPropertyName("cp")]
        public uint CP { get; set; }

        [JsonPropertyName("move_1")]
        public uint FastMove { get; set; }

        [JsonPropertyName("move_2")]
        public uint ChargeMove { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("start")]
        public ulong Start { get; set; }

        [JsonPropertyName("end")]
        public ulong End { get; set; }

        [JsonPropertyName("ex_raid_eligible")]
        public bool IsExEligible { get; set; }

        [JsonPropertyName("is_exclusive")]
        public bool IsExclusive { get; set; }

        [JsonPropertyName("sponsor_id")]
        public uint? SponsorId { get; set; }

        [JsonPropertyName("form")]
        public uint Form { get; set; }

        [JsonPropertyName("costume")]
        public uint Costume { get; set; }

        [JsonPropertyName("evolution")]
        public uint Evolution { get; set; }

        [JsonPropertyName("gender")]
        public Gender Gender { get; set; }

        [JsonPropertyName("ar_scan_eligible")]
        public bool IsArScanEligible { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; private set; }

        [JsonIgnore]
        public DateTime EndTime { get; private set; }

        [JsonIgnore]
        public bool IsEgg => PokemonId == 0;

        [JsonIgnore]
        public bool IsMega => Level == 6;

        [JsonIgnore]
        public bool IsUltraBeast => Level >= 7;

        [JsonIgnore]
        public List<PokemonType> Weaknesses
        {
            get
            {
                if (!GameMaster.Instance.Pokedex.ContainsKey(PokemonId) || IsEgg)
                    return null;

                var pkmn = GameMaster.GetPokemon(PokemonId, Form);
                if (pkmn?.Types == null)
                    return null;

                var list = new List<PokemonType>();
                pkmn?.Types?.ForEach(x => list.AddRange(x.GetWeaknesses()));
                return list;
            }
        }

        [JsonIgnore]
        public bool IsMissingStats => FastMove == 0 || ChargeMove == 0;

        #endregion

        /// <summary>
        /// Instantiate a new <see cref="RaidData"/> class.
        /// </summary>
        public RaidData()
        {
            SetTimes();
        }

        /// <summary>
        /// Set expire times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetTimes()
        {
            StartTime = Start
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            EndTime = End
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        /// <summary>
        /// Generate a Discord embed Raid message
        /// </summary>
        /// <param name="guildId">Guild the notification is for</param>
        /// <param name="client">Discord client</param>
        /// <param name="whConfig">Webhook config</param>
        /// <param name="alarm">Webhook alarm</param>
        /// <param name="city">City the Raid was found in</param>
        /// <returns>DiscordEmbedNotification object to send</returns>
        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = PokemonId > 0
                ? EmbedMessageType.Raids
                : EmbedMessageType.Eggs;
            var embed = settings.Alarm?.Embeds[embedType]
                ?? server.Subscriptions?.Embeds?[embedType]
                ?? EmbedMessage.Defaults[embedType];
            var raidImageUrl = IsEgg
                ? UIconService.Instance.GetEggIcon(server.IconStyle, Level, false, IsExEligible)
                : UIconService.Instance.GetPokemonIcon(server.IconStyle, PokemonId, Form, Evolution, Gender, Costume, false);
            settings.ImageUrl = raidImageUrl;
            var properties = await GetPropertiesAsync(settings);
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
                Color = IsExEligible
                    ? 0 /* TODO: ex color */
                    : Level.BuildRaidColor(GameMaster.Instance.DiscordEmbedColors).Value,
                Footer = new DiscordEmbedFooter
                {
                    Text = TemplateRenderer.Parse(embed.Footer?.Text, properties),
                    IconUrl = TemplateRenderer.Parse(embed.Footer?.IconUrl, properties)
                },
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
            var pkmnInfo = GameMaster.GetPokemon(PokemonId, Form);
            var name = IsEgg
                ? Translator.Instance.Translate("EGG")
                : Translator.Instance.GetPokemonName(PokemonId);
            var form = Translator.Instance.GetFormName(Form);
            var costume = Translator.Instance.GetCostumeName(Costume);
            var evo = Translator.Instance.GetEvolutionName(Evolution);
            var gender = Gender.GetPokemonGenderIcon();
            var level = Level;
            var move1 = Translator.Instance.GetMoveName(FastMove);
            var move2 = Translator.Instance.GetMoveName(ChargeMove);
            var types = pkmnInfo?.Types;
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
            var weaknesses = Weaknesses == null
                ? string.Empty
                : string.Join(", ", Weaknesses);
            var weaknessesEmoji = types?.GetWeaknessEmojiIcons();
            var perfectRange = PokemonId.GetCpAtLevel(20, 15);
            var boostedRange = PokemonId.GetCpAtLevel(25, 15);
            var worstRange = PokemonId.GetCpAtLevel(20, 10);
            var worstBoosted = PokemonId.GetCpAtLevel(25, 10);
            var exEmojiId = GameMaster.Instance.Emojis.ContainsKey("ex")
                ? GameMaster.Instance.Emojis["ex"]
                : 0;
            var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
            var teamEmojiId = GameMaster.Instance.Emojis.ContainsKey(Team.ToString().ToLower())
                ? GameMaster.Instance.Emojis[Team.ToString().ToLower()]
                : 0;
            var teamEmoji = teamEmojiId > 0 ? $"<:{Team.ToString().ToLower()}:{teamEmojiId}>" : Team.ToString();

            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfig = properties.Config.Instance.StaticMaps;
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                MapType = StaticMapType.Raids,
                TemplateType = staticMapConfig.Type == StaticMapTemplateType.StaticMap
                    ? StaticMapTemplateType.StaticMap
                    : StaticMapTemplateType.MultiStaticMap,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = properties.ImageUrl,
                Team = Team,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache?.GetGymsNearby(Latitude, Longitude)
                    : new(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? await properties .MapDataCache?.GetPokestopsNearby(Latitude, Longitude)
                    : new(),
                Pregenerate = true,
                Regeneratable = true,
            });
            var staticMapLink = staticMap.GenerateLink();
            var urlShortener = new UrlShortener(properties.Config.Instance.ShortUrlApi);
            var gmapsLocationLink = await urlShortener.CreateAsync(gmapsLink);
            var appleMapsLocationLink = await urlShortener.CreateAsync(appleMapsLink);
            var wazeMapsLocationLink = await urlShortener.CreateAsync(wazeMapsLink);
            var scannerMapsLocationLink = await urlShortener.CreateAsync(scannerMapsLink);
            var address = await ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(Latitude, Longitude));

            var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
            var startTimeLeft = now.GetTimeRemaining(StartTime).ToReadableStringNoSeconds();
            var endTimeLeft = now.GetTimeRemaining(EndTime).ToReadableStringNoSeconds();
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Raid boss properties
                pkmn_id = PokemonId.ToString(),
                pkmn_id_3 = PokemonId.ToString("D3"),
                pkmn_name = name,
                pkmn_img_url = properties.ImageUrl,
                evolution = evo,
                evolution_id = Convert.ToInt32(Evolution).ToString(),
                evolution_id_3 = Evolution.ToString("D3"),
                form,
                form_id = Form.ToString(),
                form_id_3 = Form.ToString("D3"),
                costume,
                costume_id = Costume.ToString(),
                costume_id_3 = Costume.ToString("D3"),
                is_egg = IsEgg,
                is_ex = IsExEligible,
                is_ex_exclusive = IsExclusive,
                ex_emoji = exEmoji,
                team = Team.ToString(),
                team_id = Convert.ToInt32(Team).ToString(),
                team_emoji = teamEmoji,
                cp = CP,
                lvl = level,
                gender = gender ?? defaultMissingValue,
                move_1 = move1 ?? defaultMissingValue,
                move_2 = move2 ?? defaultMissingValue,
                moveset = $"{move1}/{move2}",
                type_1 = type1.ToString() ?? defaultMissingValue,
                type_2 = type2.ToString() ?? defaultMissingValue,
                type_1_emoji = type1Emoji,
                type_2_emoji = type2Emoji,
                types = $"{type1}/{type2}",
                types_emoji = typeEmojis,
                weaknesses,
                weaknesses_emoji = weaknessesEmoji,
                perfect_cp = perfectRange.ToString(),
                perfect_cp_boosted = boostedRange.ToString(),
                worst_cp = worstRange.ToString(),
                worst_cp_boosted = worstBoosted.ToString(),
                is_ar = IsArScanEligible,

                // Time properties
                start_time = StartTime.ToLongTimeString(),
                start_time_24h = StartTime.ToString("HH:mm:ss"),
                start_time_left = startTimeLeft,
                end_time = EndTime.ToLongTimeString(),
                end_time_24h = EndTime.ToString("HH:mm:ss"),
                end_time_left = endTimeLeft,

                // Location properties
                geofence = properties.City ?? defaultMissingValue,
                lat = Latitude.ToString(),
                lng = Longitude.ToString(),
                lat_5 = Latitude.ToString("0.00000"),
                lng_5 = Longitude.ToString("0.00000"),

                // Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLocationLink,
                applemaps_url = appleMapsLocationLink,
                wazemaps_url = wazeMapsLocationLink,
                scanmaps_url = scannerMapsLocationLink,

                address = address ?? string.Empty,

                // Gym properties
                gym_id = GymId,
                gym_name = GymName,
                gym_url = GymUrl,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }
    }
}