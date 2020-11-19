namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    /// <summary>
    /// Raid egg filters
    /// </summary>
    public class FilterEggObject
    {
        /// <summary>
        /// Enable egg filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Minimum raid egg level
        /// </summary>
        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Maximum raid egg level
        /// </summary>
        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Only ex-eligible raids
        /// </summary>
        [JsonProperty("onlyEx")]
        public bool OnlyEx { get; set; }

        /// <summary>
        /// Pokemon Go Team
        /// </summary>
        [JsonProperty("team")]
        public PokemonTeam Team { get; set; }

        /// <summary>
        /// Instantiate a new raid egg filter class.
        /// </summary>
        public FilterEggObject()
        {
            MinimumLevel = 1;
            MaximumLevel = 6;

            Team = PokemonTeam.All;
        }
    }
}
