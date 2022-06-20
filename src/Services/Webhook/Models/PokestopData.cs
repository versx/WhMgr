namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using DSharpPlus.Entities;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

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

    /// <summary>
    /// RealDeviceMap Pokestop (lure/invasion) webhook model class.
    /// </summary>
    [Table("pokestop")]
    public sealed class PokestopData : IWebhookData, IWebhookPowerLevel, IWebhookPoint
    {
        #region Properties

        [
            JsonPropertyName("pokestop_id"),
            Column("id"),
            Key,
        ]
        public string PokestopId { get; set; }

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
            JsonPropertyName("name"),
            Column("name"),
        ]
        public string Name { get; set; } = "Unknown";

        [
            JsonPropertyName("url"),
            Column("url"),
        ]
        public string Url { get; set; }

        [
            JsonPropertyName("enabled"),
            Column("enabled"),
        ]
        public bool Enabled { get; set; }

        [
            JsonPropertyName("lure_expiration"),
            NotMapped,
        ]
        public long LureExpire { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime LureExpireTime { get; set; }

        [
            JsonPropertyName("lure_id"),
            NotMapped,
        ]
        public PokestopLureType LureType { get; set; }

        [
            JsonPropertyName("last_modified"),
            NotMapped,
        ]
        public ulong LastModified { get; set; }

        [
            JsonPropertyName("updated"),
            NotMapped,
        ]
        public ulong Updated { get; set; }

        [
            JsonPropertyName("power_up_points"),
            NotMapped,
        ]
        public uint PowerUpPoints { get; set; }

        [
            JsonPropertyName("power_up_level"),
            NotMapped,
        ]
        public ushort PowerUpLevel { get; set; }

        [
            JsonPropertyName("power_up_end_timestamp"),
            NotMapped,
        ]
        public ulong PowerUpEndTimestamp { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime PowerUpEndTime { get; private set; }

        [
            JsonPropertyName("ar_scan_eligible"),
            Column("ar_scan_eligible"),
        ]
        public bool? IsArScanEligible { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool HasLure => LureExpire > 0 && LureType != PokestopLureType.None && LureExpireTime > DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);

        [
            JsonPropertyName("incidents"),
            Column("incidents"),
        ]
        public ICollection<IncidentData> Incidents { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="PokestopData"/> class.
        /// </summary>
        public PokestopData()
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
            LureExpireTime = LureExpire
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            PowerUpEndTime = PowerUpEndTimestamp
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = HasLure
                ? EmbedMessageType.Lures
                : EmbedMessageType.Pokestops;
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
                Color = (
                    HasLure
                        ? LureType.BuildLureColor(GameMaster.Instance.DiscordEmbedColors)
                        : DiscordColor.CornflowerBlue
                    ).Value,
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
            var lureImageUrl = UIconService.Instance.GetPokestopIcon(server.IconStyle, LureType);
            var imageUrl = HasLure ? lureImageUrl : Url;
            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfigType = StaticMapType.Lures;
            /*
            var staticMapConfigType = HasLure
                ? StaticMapType.Lures
                : StaticMapType.Pokestop; // TODO: StaticMapType.Pokestops
            */
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

            var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
            var lureExpireTimeLeft = now.GetTimeRemaining(LureExpireTime).ToReadableStringNoSeconds();
            var powerUpEndTimeLeft = now.GetTimeRemaining(PowerUpEndTime).ToReadableStringNoSeconds();
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId)
                ? properties.Client.Guilds[properties.GuildId]
                : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                has_lure = HasLure,
                lure_type = LureType,
                lure_expire_time = LureExpireTime.ToLongTimeString(),
                lure_expire_time_24h = LureExpireTime.ToString("HH:mm:ss"),
                lure_expire_time_left = lureExpireTimeLeft,
                is_ar = IsArScanEligible ?? false,

                // Pokestop power up properties
                power_up_points = PowerUpPoints,
                power_up_level = PowerUpLevel,
                power_up_end_time = PowerUpEndTime.ToLongTimeString(),
                power_up_end_time_24h = PowerUpEndTime.ToString("HH:mm:ss"),
                power_up_end_time_left = powerUpEndTimeLeft,

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
                pokestop_name = Name ?? defaultMissingValue,
                pokestop_url = Url ?? defaultMissingValue,
                lure_img_url = lureImageUrl,

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

        #endregion
    }
}