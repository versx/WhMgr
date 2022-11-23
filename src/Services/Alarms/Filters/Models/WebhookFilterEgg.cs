namespace WhMgr.Services.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// List of raid egg levels
        /// </summary>
        [JsonPropertyName("levels")]
        public IReadOnlyList<ushort> Levels { get; set; }

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
            Levels = Enumerable.Range(Strings.Defaults.MinimumRaidLevel, Strings.Defaults.MaximumRaidLevel)
                .Select(Convert.ToUInt16)
                .ToList();
            Team = PokemonTeam.All;
        }
    }
}