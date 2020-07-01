namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Osm.Models;
    using WhMgr.Utilities;

    public enum WeatherSeverity
    {
        None = 0,
        Moderate,
        Extreme
    }

    /// <summary>
    /// RealDeviceMap Weather webhook model class.
    /// </summary>
    [Alias("weather")]
    public sealed class WeatherData
    {
        public const string WebHookHeader = "weather";

        //private static readonly IEventLogger _logger = EventLogger.GetLogger("WEATHERDATA");

        #region Properties

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("polygon")]
        public MultiPolygon Polygon { get; set; }

        [JsonProperty("gameplay_condition")]
        public WeatherType GameplayCondition { get; set; }

        [JsonProperty("wind_direction")]
        public int WindDirection { get; set; }

        [JsonProperty("cloud_level")]
        public ushort CloudLevel { get; set; }

        [JsonProperty("rain_level")]
        public ushort RainLevel { get; set; }

        [JsonProperty("wind_level")]
        public ushort WindLevel { get; set; }

        [JsonProperty("snow_level")]
        public ushort SnowLevel { get; set; }

        [JsonProperty("fog_level")]
        public ushort FogLevel { get; set; }

        [JsonProperty("special_effect_level")]
        public ushort SpecialEffectLevel { get; set; }

        [JsonProperty("severity")]
        public WeatherSeverity? Severity { get; set; }

        [JsonProperty("warn_weather")]
        public bool? WarnWeather { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonIgnore]
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
            UpdatedTime = Updated.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(Updated))
            //{
            //    UpdatedTime = UpdatedTime.AddHours(1);
            //}
        }

        public DiscordEmbed GenerateWeatherMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city)
        {
            var alertType = AlertMessageType.Weather;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(whConfig, city);
            var mention = DynamicReplacementEngine.ReplaceText(alarm.Mentions, properties);
            var description = DynamicReplacementEngine.ReplaceText(alert.Content, properties);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alert.ImageUrl, properties),
                ThumbnailUrl = DynamicReplacementEngine.ReplaceText(alert.IconUrl, properties),
                Description = mention + description,
                Color = GameplayCondition.BuildWeatherColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{(client.Guilds?[guildId]?.Name ?? Strings.Creator)} | {DateTime.Now}",
                    IconUrl = client.Guilds?[guildId]?.IconUrl ?? string.Empty
                }
            };
            return eb.Build();
        }

        private IReadOnlyDictionary<string, string> GetProperties(WhConfig whConfig, string city)
        {
            var weather = GameplayCondition.ToString();
            var weatherEmoji = string.Empty;
            var hasWeather = GameplayCondition != WeatherType.None;
            if (Strings.WeatherEmojis.ContainsKey(GameplayCondition) && GameplayCondition != WeatherType.None)
            {
                weatherEmoji = GameplayCondition.GetWeatherEmojiIcon();
            }
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            // TODO: Weather icon
            var staticMapLink = Utils.PrepareWeatherStaticMapUrl(whConfig.Urls.StaticMap.Replace("/15/", "/11/"), "https://image.flaticon.com/icons/png/512/169/169367.png", Latitude, Longitude, FixWeatherPolygon(Polygon));
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? wazeMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, wazeMapsLink);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "id", Id.ToString() },
                { "weather_condition", weather },
                { "has_weather", Convert.ToString(hasWeather) },
                { "weather", weather ?? defaultMissingValue },
                { "weather_emoji", weatherEmoji ?? defaultMissingValue },

                { "wind_direction", WindDirection.ToString() },
                { "wind_level", WindLevel.ToString() },
                { "raid_level", RainLevel.ToString() },
                { "cloud_level", CloudLevel.ToString() },
                { "fog_level", FogLevel.ToString() },
                { "snow_level", SnowLevel.ToString() },
                { "warn_weather", Convert.ToString(WarnWeather ?? false) },
                { "special_effect_level", SpecialEffectLevel.ToString() },
                { "severity", Severity.ToString() },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Latitude.ToString() },
                { "lng", Longitude.ToString() },
                { "lat_5", Math.Round(Latitude, 5).ToString() },
                { "lng_5", Math.Round(Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", staticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },
                { "wazemaps_url", wazeMapsLocationLink },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        public static MultiPolygon FixWeatherPolygon(MultiPolygon multiPolygon)
        {
            var newMultiPolygon = new MultiPolygon();
            if (multiPolygon.Count == 0 || multiPolygon == null)
                return newMultiPolygon;

            multiPolygon.ForEach(x => newMultiPolygon.Add(new Polygon { x[1], x[0] }));
            newMultiPolygon.Add(newMultiPolygon[newMultiPolygon.Count - 1]);
            return newMultiPolygon;
        }

        #endregion
    }
}