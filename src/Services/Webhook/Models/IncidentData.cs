namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using DSharpPlus.Entities;
    using POGOProtos.Rpc;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using PokemonGender = POGOProtos.Rpc.BelugaPokemonProto.Types.PokemonGender;
    using PokestopStyle = POGOProtos.Rpc.EnumWrapper.Types.PokestopStyle;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Icons;
    using WhMgr.Services.StaticMap;
    using WhMgr.Services.Yourls;

    [Table("incident")]
    public sealed class IncidentData : IWebhookData, IWebhookPoint
    {
        #region Properties

        [
            JsonPropertyName("pokestop_id"),
            Column("pokestop_id"),
            ForeignKey("pokestop_id"),
        ]
        public string PokestopId { get; set; }

        [JsonIgnore]
        public PokestopData Pokestop { get; set; }

        [
            JsonPropertyName("id"),
            Column("id"),
            Key,
        ]
        public string Id { get; set; }

        /*
        [
            JsonPropertyName("pokestop_id"),
            Column("pokestop_id"),
        ]
        public string PokestopId { get; set; }
        */

        [
            JsonPropertyName("pokestop_name"),
            NotMapped,
        ]
        public string PokestopName { get; set; } = "Unknown";

        [
            JsonPropertyName("url"),
            NotMapped,
        ]
        public string Url { get; set; }

        [
            JsonPropertyName("latitude"),
            NotMapped,
        ]
        public double Latitude { get; set; }

        [
            JsonPropertyName("longitude"),
            NotMapped,
        ]
        public double Longitude { get; set; }

        [
            JsonPropertyName("enabled"),
            NotMapped,
        ]
        public bool Enabled { get; set; }

        [
            JsonPropertyName("display_type"),
            Column("display_type"),
        ]
        public IncidentDisplayType DisplayType { get; set; }

        [
            JsonPropertyName("style"),
            Column("style"),
        ]
        public PokestopStyle Style { get; set; }

        [
            JsonPropertyName("character"),
            Column("character"),
        ]
        public InvasionCharacter Character { get; set; }

        [
            JsonPropertyName("start"),
            Column("start"),
        ]
        public long Start { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime StartTime { get; set; }

        [
            JsonPropertyName("expiration"),
            Column("expiration"),
        ]
        public long Expiration { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime ExpirationTime { get; set; }

        [
            JsonPropertyName("updated"),
            Column("updated"),
        ]
        public ulong Updated { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime UpdatedTime { get; set; }

        #endregion

        #region Constructor

        public IncidentData()
        {
            SetTimes();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set expire times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetTimes()
        {
            StartTime = Start
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            ExpirationTime = Expiration
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            UpdatedTime = Updated
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Invasions;
            var embed = settings.Alarm?.Embeds[embedType]
                    ?? server.Subscriptions?.Embeds?[embedType]
                    ?? EmbedMessage.Defaults[embedType];
            var properties = await GetPropertiesAsync(settings).ConfigureAwait(false);
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
                Color = new DiscordColor(GameMaster.Instance.DiscordEmbedColors.Pokestops.Invasions).Value,
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

        #endregion

        #region Private Methods

        private async Task<dynamic> GetPropertiesAsync(AlarmMessageSettings properties)
        {
            var server = properties.Config.Instance.Servers[properties.GuildId];
            var imageUrl = UIconService.Instance.GetInvasionIcon(server.IconStyle, Character);
            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfigType = StaticMapType.Invasions;
            var staticMapConfig = properties.Config.Instance.StaticMaps;
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                MapType = staticMapConfigType,
                TemplateType = staticMapConfig.Type,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = imageUrl,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache?.GetGymsNearby(Latitude, Longitude)
                    : new(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? await properties.MapDataCache?.GetPokestopsNearby(Latitude, Longitude)
                    : new(),
                Pregenerate = staticMapConfig.Pregenerate,
                Regeneratable = true,
            });
            var staticMapLink = staticMap.GenerateLink();
            var urlShortener = new UrlShortener(properties.Config.Instance.ShortUrlApi);
            var gmapsLocationLink = await urlShortener.CreateAsync(gmapsLink);
            var appleMapsLocationLink = await urlShortener.CreateAsync(appleMapsLink);
            var wazeMapsLocationLink = await urlShortener.CreateAsync(wazeMapsLink);
            var scannerMapsLocationLink = await urlShortener.CreateAsync(scannerMapsLink);
            var address = await ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(Latitude, Longitude));

            var invasion = GameMaster.Instance.GruntTypes.ContainsKey(Character)
                ? GameMaster.Instance.GruntTypes[Character]
                : null;
            var leaderString = Translator.Instance.GetGruntType(Character);
            var pokemonType = GameMaster.Instance.GruntTypes.ContainsKey(Character)
                ? GetPokemonTypeFromString(invasion?.Type)
                : PokemonType.None;
            var invasionTypeEmoji = pokemonType == PokemonType.None
                ? leaderString
                : pokemonType.GetTypeEmojiIcons();
            var invasionEncounters = Character > 0
                ? invasion.GetPossibleInvasionEncounters()
                : new List<dynamic>();

            var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
            var invasionExpireTimeLeft = now.GetTimeRemaining(ExpirationTime).ToReadableStringNoSeconds();
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId)
                ? properties.Client.Guilds[properties.GuildId]
                : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                has_invasion = true,
                grunt_type = invasion?.Type,
                character = invasion?.Type,
                display_type = DisplayType,
                display_type_id = Convert.ToInt32(DisplayType),
                style = Style,
                style_id = Convert.ToInt32(Style),
                grunt_type_emoji = invasionTypeEmoji,
                grunt_gender = invasion?.Gender,
                grunt_gender_id = Convert.ToInt32(invasion?.Gender ?? PokemonGender.GenderUnset),
                invasion_expire_time = ExpirationTime.ToLongTimeString(),
                invasion_expire_time_24h = ExpirationTime.ToString("HH:mm:ss"),
                invasion_expire_time_left = invasionExpireTimeLeft,
                invasion_encounters = invasionEncounters,

                // Location properties
                geofence = properties.City ?? defaultMissingValue,
                lat = Latitude,
                lng = Longitude,
                lat_5 = Latitude.ToString("0.00000"),
                lng_5 = Longitude.ToString("0.00000"),

                // Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLocationLink,
                applemaps_url = appleMapsLocationLink,
                wazemaps_url = wazeMapsLocationLink,
                scanmaps_url = scannerMapsLocationLink,

                // Pokestop properties
                pokestop_id = PokestopId ?? defaultMissingValue,
                pokestop_name = PokestopName ?? defaultMissingValue,
                pokestop_url = Url ?? defaultMissingValue,
                invasion_img_url = imageUrl,

                address = address ?? string.Empty,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }

        private static PokemonType GetPokemonTypeFromString(string pokemonType)
        {
            var type = pokemonType.ToLower();
            if (type.Contains("bug"))
                return PokemonType.Bug;
            else if (type.Contains("dark"))
                return PokemonType.Dark;
            else if (type.Contains("dragon"))
                return PokemonType.Dragon;
            else if (type.Contains("electric"))
                return PokemonType.Electric;
            else if (type.Contains("fairy"))
                return PokemonType.Fairy;
            else if (type.Contains("fighting") || type.Contains("fight"))
                return PokemonType.Fighting;
            else if (type.Contains("fire"))
                return PokemonType.Fire;
            else if (type.Contains("flying") || type.Contains("fly"))
                return PokemonType.Flying;
            else if (type.Contains("ghost"))
                return PokemonType.Ghost;
            else if (type.Contains("grass"))
                return PokemonType.Grass;
            else if (type.Contains("ground"))
                return PokemonType.Ground;
            else if (type.Contains("ice"))
                return PokemonType.Ice;
            //else if (type.Contains("tierii") || type.Contains("none") || type.Contains("tier2") || type.Contains("t2"))
            //    return PokemonType.None;
            else if (type.Contains("normal"))
                return PokemonType.Normal;
            else if (type.Contains("poison"))
                return PokemonType.Poison;
            else if (type.Contains("psychic"))
                return PokemonType.Psychic;
            else if (type.Contains("rock"))
                return PokemonType.Rock;
            else if (type.Contains("steel"))
                return PokemonType.Steel;
            else if (type.Contains("water"))
                return PokemonType.Water;
            else
                return PokemonType.None;
        }

        #endregion
    }
}