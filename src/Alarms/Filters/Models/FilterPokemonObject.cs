namespace WhMgr.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    /// <summary>
    /// Pokemon filters
    /// </summary>
    public class FilterPokemonObject
    {
        /// <summary>
        /// Enable pokemon filter
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// List of pokemon pokedex IDs to filter against
        /// </summary>
        //TODO: Allow pokemon names and ids for pokemon filter.
        [JsonProperty("pokemon")]
        public List<int> Pokemon { get; set; }

        /// <summary>
        /// Minimum IV value to report
        /// </summary>
        [JsonProperty("min_iv")]
        public uint MinimumIV { get; set; }

        /// <summary>
        /// Maximum IV value to report
        /// </summary>
        [JsonProperty("max_iv")]
        public uint MaximumIV { get; set; }

        /// <summary>
        /// Minimum CP value to report
        /// </summary>
        [JsonProperty("min_cp")]
        public uint MinimumCP { get; set; }

        /// <summary>
        /// Maximum CP value to report
        /// </summary>
        [JsonProperty("max_cp")]
        public uint MaximumCP { get; set; }

        /// <summary>
        /// Minimum level value to report
        /// </summary>
        [JsonProperty("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Maximum level value to report
        /// </summary>
        [JsonProperty("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Pokemon gender to filter by
        /// </summary>
        [JsonProperty("gender")]
        public char Gender { get; set; }

        /// <summary>
        /// Pokemon size to filter by
        /// </summary>
        [JsonProperty("size")]
        public PokemonSize? Size { get; set; }

        /// <summary>
        /// Only great league PvP eligible Pokemon
        /// </summary>
        [JsonProperty("great_league")]
        public bool IsPvpGreatLeague { get; set; }

        /// <summary>
        /// Only ultra league PvP eligible Pokemon
        /// </summary>
        [JsonProperty("ultra_league")]
        public bool IsPvpUltraLeague { get; set; }

        /// <summary>
        /// Minimum PvP rank to report
        /// </summary>
        [JsonProperty("min_rank")]
        public uint MinimumRank { get; set; }

        /// <summary>
        /// Maximum PvP rank to report
        /// </summary>
        [JsonProperty("max_rank")]
        public uint MaximumRank { get; set; }

        //TODO: Filter by move?

        /// <summary>
        /// Pokemon filter type
        /// </summary>
        [JsonProperty("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Event Pokemon flag indicating it was checked with an event account
        /// </summary>
        [JsonProperty("is_event")]
        public bool IsEvent { get; set; }

        /// <summary>
        /// Ignore Pokemon missing stats
        /// </summary>
        [JsonProperty("ignoreMissing")]
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterPokemonObject"/> class.
        /// </summary>
        public FilterPokemonObject()
        {
            MinimumIV = 0;
            MaximumIV = 100;
            MinimumCP = 0;
            MaximumCP = 999999;
            MinimumLevel = 0;
            MaximumLevel = 100; //Support for when they increase level cap. :wink:
            MinimumRank = 0;
            MaximumRank = 4096;
            Gender = '*';
            Size = null;
        }
    }
}