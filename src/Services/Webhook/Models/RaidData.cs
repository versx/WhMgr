namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using DSharpPlus.Entities;
    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
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
        public ushort CP { get; set; }

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
        public uint SponsorId { get; set; }

        [JsonPropertyName("form")]
        public uint Form { get; set; }

        [JsonPropertyName("costume")]
        public uint Costume { get; set; }

        [JsonPropertyName("evolution")]
        public uint Evolution { get; set; }

        [JsonPropertyName("gender")]
        public Gender Gender { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; private set; }

        [JsonIgnore]
        public DateTime EndTime { get; private set; }

        [JsonIgnore]
        public bool IsEgg => PokemonId == 0;

        [JsonIgnore]
        public List<PokemonType> Weaknesses
        {
            get
            {
                if (MasterFile.Instance.Pokedex.ContainsKey(PokemonId) && !IsEgg)
                {
                    var list = new List<PokemonType>();
                    var types = MasterFile.GetPokemon(PokemonId, Form)?.Types;
                    if (types != null)
                    {
                        MasterFile.GetPokemon(PokemonId, Form)?.Types?.ForEach(x => list.AddRange(x.GetWeaknesses()));
                    }
                    return list;
                }

                return null;
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
        public DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = PokemonId > 0 ? EmbedMessageType.Raids : EmbedMessageType.Eggs;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.DmEmbeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            var raidImageUrl = IsEgg
                ? IconFetcher.Instance.GetRaidEggIcon(server.IconStyle, Convert.ToInt32(Level), false, IsExEligible)
                : IconFetcher.Instance.GetPokemonIcon(server.IconStyle, PokemonId, Form, Evolution, Gender, Costume, false);
            settings.ImageUrl = raidImageUrl;
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
                Color = IsExEligible ? 0 /*ex*/ : Level.BuildRaidColor(MasterFile.Instance.DiscordEmbedColors).Value,
                Footer = new Discord.Models.DiscordEmbedFooter
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

        private dynamic GetProperties(AlarmMessageSettings properties)
        {
            var pkmnInfo = MasterFile.GetPokemon(PokemonId, Form);
            var name = IsEgg ? "Egg" /*TODO: Localize*/ : Translator.Instance.GetPokemonName(PokemonId);
            var form = Translator.Instance.GetFormName(Form);
            var costume = Translator.Instance.GetCostumeName(Costume);
            var evo = Translator.Instance.GetEvolutionName(Evolution);
            var gender = Gender.GetPokemonGenderIcon();
            var level = Level;
            var move1 = Translator.Instance.GetMoveName(FastMove);
            var move2 = Translator.Instance.GetMoveName(ChargeMove);
            var types = pkmnInfo?.Types;
            var type1 = types?[0];
            var type2 = types?.Count > 1 ? types?[1] : PokemonType.None;
            var type1Emoji = types?[0].GetTypeEmojiIcons();
            var type2Emoji = pkmnInfo?.Types?.Count > 1 ? types?[1].GetTypeEmojiIcons() : string.Empty;
            var typeEmojis = $"{type1Emoji} {type2Emoji}";
            var weaknesses = Weaknesses == null ? string.Empty : string.Join(", ", Weaknesses);
            var weaknessesEmoji = types?.GetWeaknessEmojiIcons();
            var perfectRange = PokemonId.MaxCpAtLevel(20);
            var boostedRange = PokemonId.MaxCpAtLevel(25);
            var worstRange = PokemonId.MinCpAtLevel(20);
            var worstBoosted = PokemonId.MinCpAtLevel(25);
            var exEmojiId = MasterFile.Instance.Emojis["ex"];
            var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
            var teamEmojiId = MasterFile.Instance.Emojis[Team.ToString().ToLower()];
            var teamEmoji = teamEmojiId > 0 ? $"<:{Team.ToString().ToLower()}:{teamEmojiId}>" : Team.ToString();

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfig = properties.Config.Instance.StaticMaps[StaticMapType.Raids];
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                TemplateName = staticMapConfig.TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = properties.ImageUrl,
                Team = Team,
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
                type_1 = type1?.ToString() ?? defaultMissingValue,
                type_2 = type2?.ToString() ?? defaultMissingValue,
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

                address = address?.Address,

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