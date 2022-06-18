﻿namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    /// <summary>
    /// Raid egg filters
    /// </summary>
    public class WebhookFilterEgg
    {
        /// <summary>
        /// Enable egg filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Minimum raid egg level
        /// </summary>
        [JsonPropertyName("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Maximum raid egg level
        /// </summary>
        [JsonPropertyName("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Only ex-eligible raids
        /// </summary>
        [JsonPropertyName("only_ex")]
        public bool OnlyEx { get; set; }

        /// <summary>
        /// Pokemon Go Team
        /// </summary>
        [JsonPropertyName("team")]
        public PokemonTeam Team { get; set; }

        /// <summary>
        /// Gets or sets the Gym power level filtering options
        /// </summary>
        [JsonPropertyName("power_level")]
        public WebhookFilterGymLevel PowerLevel { get; set; } = new();

        /// <summary>
        /// Instantiate a new raid egg filter class.
        /// </summary>
        public WebhookFilterEgg()
        {
            MinimumLevel = 1;
            MaximumLevel = 8;

            Team = PokemonTeam.All;
        }
    }
}