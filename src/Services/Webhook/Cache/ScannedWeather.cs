namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using WhMgr.Common;
    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedWeather : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public WeatherCondition Condition { get; }

        public DateTime LastUpdated { get; set; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                var lastUpdated = LastUpdated.ConvertTimeFromCoordinates(Latitude, Longitude);
                // TODO: Check if last updated within the last 60 minutes
                return now > lastUpdated.Subtract(new TimeSpan(0, 60, 0));
            }
        }

        public ScannedWeather(WeatherData weather)
        {
            Latitude = weather.Latitude;
            Longitude = weather.Longitude;
            Condition = weather.GameplayCondition;
            LastUpdated = weather.UpdatedTime;
        }
    }
}
