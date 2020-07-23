namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    /// <summary>
    /// Raid boss filters
    /// </summary>
    public class FilterRaidObject
    {
        /// <summary>
        /// Enable raid boss filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Minimum raid level
        /// </summary>
        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Maximum raid level
        /// </summary>
        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Raid boss pokedex ID list to filter against
        /// </summary>
        //TODO: Allow pokemon names and ids for raid filter.
        [JsonProperty("pokemon")]
        public List<int> Pokemon { get; set; }

        /// <summary>
        /// Raid boss filter type
        /// </summary>
        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Only report ex-eligible raids
        /// </summary>
        [JsonProperty("onlyEx")]
        public bool OnlyEx { get; set; }

        /// <summary>
        /// Gym team control filter
        /// </summary>
        [JsonProperty("team")]
        public PokemonTeam Team { get; set; }

        /// <summary>
        /// Ignore raids missing stats
        /// </summary>
        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterRaidObject"/> class
        /// </summary>
        public FilterRaidObject()
        {
            MinimumLevel = 1;
            MaximumLevel = 5;
            Team = PokemonTeam.All;
        }
    }
}