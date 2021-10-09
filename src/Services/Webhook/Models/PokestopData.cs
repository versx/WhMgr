﻿namespace WhMgr.Services.Webhook.Models
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
    using WhMgr.Utilities;

    /// <summary>
    /// RealDeviceMap Pokestop (lure/invasion) webhook model class.
    /// </summary>
    [Table("pokestop")]
    public sealed class PokestopData : IWebhookData
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
            JsonPropertyName("incident_expire_timestamp"),
            NotMapped,
        ]
        public long IncidentExpire { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime InvasionExpireTime { get; set; }

        [
            JsonPropertyName("grunt_type"),
            NotMapped,
        ]
        public InvasionCharacter GruntType { get; set; }

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
            JsonPropertyName("ar_scan_eligible"),
            Column("ar_scan_eligible"),
        ]
        public bool IsArScanEligible { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool HasLure => LureExpire > 0 && LureType != PokestopLureType.None && LureExpireTime > DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);

        [
            JsonIgnore,
            NotMapped,
        ]
        public bool HasInvasion => GruntType != InvasionCharacter.CharacterUnset
            && IncidentExpire > 0
            && InvasionExpireTime > DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);

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

            InvasionExpireTime = IncidentExpire
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = HasInvasion
                ? EmbedMessageType.Invasions
                : HasLure
                    ? EmbedMessageType.Lures
                    : EmbedMessageType.Pokestops;
            var embed = settings.Alarm?.Embeds[embedType]
                    ?? server.DmEmbeds?[embedType]
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
                    HasInvasion
                    ? new DiscordColor(GameMaster.Instance.DiscordEmbedColors.Pokestops.Invasions)
                    : HasLure
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
            var invasionImageUrl = UIconService.Instance.GetInvasionIcon(server.IconStyle, GruntType);
            var imageUrl = HasInvasion
                ? invasionImageUrl
                : HasLure
                    ? lureImageUrl
                    : Url;
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfigType = HasInvasion
                    ? StaticMapType.Invasions
                    : HasLure
                        ? StaticMapType.Lures
                        : StaticMapType.Invasions; // TODO: Fix
            var staticMapConfig = properties.Config.Instance.StaticMaps[staticMapConfigType];
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                TemplateName = staticMapConfig.TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = imageUrl,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache?.GetGymsNearby(Latitude, Longitude)
                    : new List<dynamic>(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? await properties.MapDataCache?.GetPokestopsNearby(Latitude, Longitude)
                    : new List<dynamic>(),
            });
            var staticMapLink = staticMap.GenerateLink();
            var shortUrlApiUrl = properties.Config.Instance.ShortUrlApiUrl;
            var gmapsLocationLink = UrlShortener.Create(shortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.Create(shortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.Create(shortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.Create(shortUrlApiUrl, scannerMapsLink);
            var address = ReverseGeocodingLookup.Instance.GetAddress(new Coordinate(Latitude, Longitude));
            var invasion = GameMaster.Instance.GruntTypes.ContainsKey(GruntType) ? GameMaster.Instance.GruntTypes[GruntType] : null;
            var leaderString = Translator.Instance.GetGruntType(GruntType);
            var pokemonType = GameMaster.Instance.GruntTypes.ContainsKey(GruntType)
                ? GetPokemonTypeFromString(invasion?.Type)
                : PokemonType.None;
            var invasionTypeEmoji = pokemonType == PokemonType.None
                ? leaderString
                : pokemonType.GetTypeEmojiIcons();
            var invasionEncounters = GruntType > 0 ? invasion.GetPossibleInvasionEncounters() : new List<dynamic>();

            var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
            var lureExpireTimeLeft = now.GetTimeRemaining(LureExpireTime).ToReadableStringNoSeconds();
            var invasionExpireTimeLeft = now.GetTimeRemaining(InvasionExpireTime).ToReadableStringNoSeconds();
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                has_lure = HasLure,
                lure_type = LureType.ToString(),
                lure_expire_time = LureExpireTime.ToLongTimeString(),
                lure_expire_time_24h = LureExpireTime.ToString("HH:mm:ss"),
                lure_expire_time_left = lureExpireTimeLeft,
                has_invasion = HasInvasion,
                grunt_type = invasion?.Type,
                grunt_type_emoji = invasionTypeEmoji,
                grunt_gender = invasion?.Grunt,
                invasion_expire_time = InvasionExpireTime.ToLongTimeString(),
                invasion_expire_time_24h = InvasionExpireTime.ToString("HH:mm:ss"),
                invasion_expire_time_left = invasionExpireTimeLeft,
                invasion_encounters = invasionEncounters,
                is_ar = IsArScanEligible,

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

                // Pokestop properties
                pokestop_id = PokestopId ?? defaultMissingValue,
                pokestop_name = Name ?? defaultMissingValue,
                pokestop_url = Url ?? defaultMissingValue,
                lure_img_url = lureImageUrl,
                invasion_img_url = invasionImageUrl,

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

        public static PokemonType GetPokemonTypeFromString(string pokemonType)
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
    }
}