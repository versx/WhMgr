namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Extensions;

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

        //[JsonProperty("polygon")]
        [JsonIgnore] //TODO: Implement weather polygons
        public List<List<double>> Polygon { get; set; }

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
        public ushort? Severity { get; set; }

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

        #endregion
    }
}