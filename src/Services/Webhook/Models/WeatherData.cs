namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using DSharpPlus.Entities;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Osm;
    using WhMgr.Osm.Models;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Utilities;

    [Table("weather")]
    public class WeatherData : IWebhookData
    {
        #region Properties

        [
            JsonPropertyName("s2_cell_id"),
            Column("id"),
        ]
        public long Id { get; set; }

        [
            JsonPropertyName("latitude"),
            Column("latitude"),
        ]
        public double Latitude { get; set; }

        [
            JsonPropertyName("longitude"),
            Column("longitude"),
        ]
        public double Longitude { get; set; }

        [
            JsonPropertyName("polygon"),
            NotMapped,
        ]
        public MultiPolygon Polygon { get; set; }

        [
            JsonPropertyName("gameplay_condition"),
            Column("gameplay_condition"),
        ]
        public WeatherCondition GameplayCondition { get; set; }

        [
            JsonPropertyName("wind_direction"),
            NotMapped,
        ]
        public int WindDirection { get; set; }

        [
            JsonPropertyName("cloud_level"),
            NotMapped,
        ]
        public ushort CloudLevel { get; set; }

        [
            JsonPropertyName("rain_level"),
            NotMapped,
        ]
        public ushort RainLevel { get; set; }

        [
            JsonPropertyName("wind_level"),
            NotMapped,
        ]
        public ushort WindLevel { get; set; }

        [
            JsonPropertyName("snow_level"),
            NotMapped,
        ]
        public ushort SnowLevel { get; set; }

        [
            JsonPropertyName("fog_level"),
            NotMapped,
        ]
        public ushort FogLevel { get; set; }

        [
            JsonPropertyName("special_effect_level"),
            NotMapped,
        ]
        public ushort SpecialEffectLevel { get; set; }

        [
            JsonPropertyName("severity"),
            NotMapped,
        ]
        public WeatherSeverity? Severity { get; set; }

        [
            JsonPropertyName("warn_weather"),
            NotMapped,
        ]
        public bool? WarnWeather { get; set; }

        [
            JsonPropertyName("updated"),
            Column("updated"),
        ]
        public long Updated { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public DateTime UpdatedTime { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="WeatherData"/> class.
        /// </summary>
        public WeatherData()
        {
            SetTimes();
        }

        #endregion

        #region Public Methods

        public void SetTimes()
        {
            UpdatedTime = Updated
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Weather;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.DmEmbeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            var weatherImageUrl = IconFetcher.Instance.GetWeatherIcon(server.IconStyle, GameplayCondition);
            //settings.ImageUrl = weatherImageUrl;
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
                Color = GameplayCondition.BuildWeatherColor(MasterFile.Instance.DiscordEmbedColors).Value,
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

        #endregion

        #region Private Methods

        private dynamic GetProperties(AlarmMessageSettings properties)
        {
            var weather = Translator.Instance.GetWeather(GameplayCondition);
            var weatherEmoji = GameplayCondition != WeatherCondition.None ? GameplayCondition.GetEmojiIcon("weather", false) : string.Empty;
            var hasWeather = GameplayCondition != WeatherCondition.None;
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var polygonPath = OsmManager.MultiPolygonToLatLng(new List<MultiPolygon> { Polygon }, false);
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = properties.Config.Instance.StaticMaps[StaticMapType.Weather].Url,
                TemplateName = properties.Config.Instance.StaticMaps[StaticMapType.Weather].TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = properties.ImageUrl,
                PolygonPath = polygonPath,
            });
            var staticMapLink = staticMap.GenerateLink();
            var gmapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, scannerMapsLink);
            var address = new Coordinate(properties.City, Latitude, Longitude).GetAddress(properties.Config.Instance);
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                id = Id.ToString(),
                weather_condition = weather,
                has_weather = hasWeather,
                weather = weather ?? defaultMissingValue,
                weather_emoji = weatherEmoji ?? defaultMissingValue,
                weather_img_url = properties.ImageUrl,//weatherImageUrl,

                wind_direction = WindDirection.ToString(),
                wind_level = WindLevel.ToString(),
                raid_level = RainLevel.ToString(),
                cloud_level = CloudLevel.ToString(),
                fog_level = FogLevel.ToString(),
                snow_level = SnowLevel.ToString(),
                warn_weather = WarnWeather ?? false,
                special_effect_level = SpecialEffectLevel.ToString(),
                severity = Severity.ToString(),

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

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }

        public static MultiPolygon FixWeatherPolygon(MultiPolygon multiPolygon)
        {
            var newMultiPolygon = new MultiPolygon();
            if (multiPolygon.Count == 0 || multiPolygon == null)
                return newMultiPolygon;

            multiPolygon.ForEach(x => newMultiPolygon.Add(new Polygon { x[1], x[0] }));
            newMultiPolygon.Add(newMultiPolygon[^1]);
            return newMultiPolygon;
        }

        #endregion
    }
}