namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    /// <summary>
    /// Gym filters
    /// </summary>
    public class FilterGymObject
    {
        /// <summary>
        /// Enable gym filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Filter only Gyms under attack
        /// </summary>
        [JsonProperty("underAttack")]
        public bool UnderAttack { get; set; }

        /// <summary>
        /// Filter by Pokemon Go Team
        /// </summary>
        [JsonProperty("team")]
        public PokemonTeam Team { get; set; }
    }
}