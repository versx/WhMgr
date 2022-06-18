namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    /// <summary>
    /// Gym filters
    /// </summary>
    public class WebhookFilterGym
    {
        /// <summary>
        /// Enable gym filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Filter only Gyms under attack
        /// </summary>
        [JsonPropertyName("under_attack")]
        public bool UnderAttack { get; set; }

        /// <summary>
        /// Filter by Pokemon Go Team
        /// </summary>
        [JsonPropertyName("team")]
        public PokemonTeam Team { get; set; }

        /// <summary>
        /// Gets or sets the Gym power level filtering options
        /// </summary>
        [JsonPropertyName("power_level")]
        public WebhookFilterGymLevel PowerLevel { get; set; } = new();
    }
}