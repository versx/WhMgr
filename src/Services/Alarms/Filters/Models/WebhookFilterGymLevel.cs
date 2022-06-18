﻿namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Text.Json.Serialization;

    public class WebhookFilterGymLevel
    {
        [JsonPropertyName("min_level")]
        public uint MinimumLevel { get; set; }

        [JsonPropertyName("max_level")]
        public uint MaximumLevel { get; set; }

        [JsonPropertyName("min_points")]
        public uint MinimumPoints { get; set; }

        [JsonPropertyName("max_points")]
        public uint MaximumPoints { get; set; }

        public WebhookFilterGymLevel()
        {
            MinimumLevel = 1;
            MaximumLevel = 3;
            MinimumPoints = 10;
            MaximumPoints = 250;
        }
    }
}