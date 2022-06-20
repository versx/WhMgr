﻿namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using DSharpPlus.Entities;

    using WhMgr.Common;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Icons;
    using WhMgr.Services.StaticMap;
    using WhMgr.Services.Yourls;

    [Table("gym")]
    public sealed class GymDetailsData : IWebhookData, IWebhookFort, IWebhookPowerLevel, IWebhookPoint
    {
        #region Properties

        [
            JsonPropertyName("id"),
            Column("id"),
            Key,
        ]
        public string FortId { get; set; }

        [
            JsonPropertyName("name"),
            Column("name"),
        ]
        public string FortName { get; set; } = "Unknown";

        [
            JsonPropertyName("url"),
            Column("url"),
        ]
        public string FortUrl { get; set; }

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
            JsonPropertyName("team"),
            Column("team_id"),
        ]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [
            JsonPropertyName("slots_available"),
            Column("available_slots"),
        ]
        public ushort SlotsAvailable { get; set; }

        [
            JsonPropertyName("ex_raid_eligible"),
            Column("ex_raid_eligible"),
        ]
        public bool IsExEligible { get; set; }

        [
            JsonPropertyName("sponsor_id"),
            Column("sponsor_id"),
        ]
        public uint? SponsorId { get; set; }

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
            JsonPropertyName("in_battle"),
            Column("in_battle"),
        ]
        public bool InBattle { get; set; }

        [
            JsonPropertyName("ar_scan_eligible"),
            Column("ar_scan_eligible"),
        ]
        public bool? IsArScanEligible { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime PowerUpEndTime { get; private set; }

        #endregion

        /// <summary>
        /// Set expire times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetTimes()
        {
            PowerUpEndTime = PowerUpEndTimestamp
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Gyms;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.Subscriptions?.Embeds?[embedType] ?? EmbedMessage.Defaults[embedType];
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
                    Team == PokemonTeam.Mystic
                    ? DiscordColor.Blue
                    : Team == PokemonTeam.Valor
                        ? DiscordColor.Red
                        : Team == PokemonTeam.Instinct
                            ? DiscordColor.Yellow
                            : DiscordColor.LightGray
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

        private async Task<dynamic> GetPropertiesAsync(AlarmMessageSettings properties)
        {
            // Get old gym from cache
            var oldGym = await properties.MapDataCache.GetGym(FortId).ConfigureAwait(false);
            var exEmoji = "ex".GetEmojiIcon(null, true);
            var teamEmoji = Team.GetEmojiIcon(null, true);
            var oldTeamEmoji = oldGym?.Team.GetEmojiIcon(null, true);

            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);
            var gymImageUrl = UIconService.Instance.GetGymIcon(properties.Config.Instance.Servers[properties.GuildId].IconStyle, Team);

            var staticMapConfig = properties.Config.Instance.StaticMaps;
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                MapType = StaticMapType.Gyms,
                TemplateType = staticMapConfig.Type,
                Latitude = Latitude,
                Longitude = Longitude,
                Team = Team,
                SecondaryImageUrl = gymImageUrl,
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
            var powerUpEndTimeLeft = now.GetTimeRemaining(PowerUpEndTime).ToReadableStringNoSeconds();
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId)
                ? properties.Client.Guilds[properties.GuildId]
                : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                gym_id = FortId,
                gym_name = FortName,
                gym_url = FortUrl,
                gym_team = Team,
                gym_team_id = Convert.ToInt32(Team),
                gym_team_emoji = teamEmoji,
                old_gym_team = oldGym?.Team,
                old_gym_team_id = Convert.ToInt32(oldGym?.Team ?? 0),
                old_gym_team_emoji = oldTeamEmoji,
                team_changed = oldGym?.Team != Team,
                in_battle = InBattle,
                under_attack = InBattle,
                is_ex = IsExEligible,
                sponsor_id = SponsorId,
                ex_emoji = exEmoji,
                slots_available = SlotsAvailable == 0
                    ? Translator.Instance.Translate("FULL")
                    : SlotsAvailable == 6
                        ? Translator.Instance.Translate("Empty")
                        : SlotsAvailable.ToString("N0"),
                is_ar = IsArScanEligible ?? false,

                // Gym power up properties
                power_up_points = PowerUpPoints,
                power_up_level = PowerUpLevel,
                power_up_end_time = PowerUpEndTime.ToLongTimeString(),
                power_up_end_time_24h = PowerUpEndTime.ToString("HH:mm:ss"),
                power_up_end_time_left = powerUpEndTimeLeft,

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