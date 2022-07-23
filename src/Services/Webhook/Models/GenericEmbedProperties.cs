namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DSharpPlus.Entities;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Yourls;

    public class GenericEmbedProperties
    {
        public string GoogleMapsLocationLink { get; set; }

        public string AppleMapsLocationLink { get; set; }

        public string WazeMapsLocationLink { get; set; }

        public string ScannerMapsLocationLink { get; set; }

        public string Address { get; set; }

        public DiscordGuild Guild { get; set; }

        public DateTime Now { get; set; }


        public static GenericEmbedProperties Generate(Config config, IReadOnlyDictionary<ulong, DiscordGuild> guilds, ulong guildId, IWebhookPoint coord)
        {
            var data = GenerateAsync(config, guilds, guildId, coord).Result;
            return data;
        }

        /// <summary>
        /// Generate generic properties all embeds use/share to reduce code redundancies.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="guilds"></param>
        /// <param name="guildId"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static async Task<GenericEmbedProperties> GenerateAsync(Config config, IReadOnlyDictionary<ulong, DiscordGuild> guilds, ulong guildId, IWebhookPoint coord)
        {
            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, coord.Latitude, coord.Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, coord.Latitude, coord.Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, coord.Latitude, coord.Longitude);
            var scannerMapsLink = string.Format(config.Urls.ScannerMap, coord.Latitude, coord.Longitude);

            var urlShortener = new UrlShortener(config.ShortUrlApi);
            var gmapsLocationLink = await urlShortener.CreateAsync(gmapsLink);
            var appleMapsLocationLink = await urlShortener.CreateAsync(appleMapsLink);
            var wazeMapsLocationLink = await urlShortener.CreateAsync(wazeMapsLink);
            var scannerMapsLocationLink = await urlShortener.CreateAsync(scannerMapsLink);
            var address = await ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(coord));

            var now = DateTime.UtcNow.ConvertTimeFromCoordinates(coord);
            var guild = guilds?.ContainsKey(guildId) ?? false
                ? guilds[guildId]
                : null;

            return new GenericEmbedProperties
            {
                GoogleMapsLocationLink = gmapsLocationLink,
                AppleMapsLocationLink = appleMapsLocationLink,
                WazeMapsLocationLink = wazeMapsLocationLink,
                ScannerMapsLocationLink = scannerMapsLocationLink,
                Address = address ?? string.Empty,
                Guild = guild,
                Now = now,
            };
        }
    }
}