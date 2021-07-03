namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    /// <summary>
    /// Pokemon filters
    /// </summary>
    public class WebhookFilterPokemon
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable the pokemon filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the list of pokemon pokedex IDs to filter against
        /// </summary>
        //TODO: Allow pokemon names and ids for pokemon filter.
        [JsonPropertyName("pokemon")]
        public List<uint> Pokemon { get; set; }

        /// <summary>
        /// Gets or sets the list of pokemon Form strings to filter against
        /// </summary>
        [JsonPropertyName("forms")]
        public List<string> Forms { get; set; }

        /// <summary>
        /// Gets or sets the list of Pokemon costume strings to filter against
        /// </summary>
        [JsonPropertyName("costumes")]
        public List<string> Costumes { get; set; }

        /// <summary>
        /// Gets or sets the minimum IV value to report
        /// </summary>
        [JsonPropertyName("min_iv")]
        public uint MinimumIV { get; set; }

        /// <summary>
        /// Gets or sets the maximum IV value to report
        /// </summary>
        [JsonPropertyName("max_iv")]
        public uint MaximumIV { get; set; }

        /// <summary>
        /// Gets or sets the minimum CP value to report
        /// </summary>
        [JsonPropertyName("min_cp")]
        public uint MinimumCP { get; set; }

        /// <summary>
        /// Gets or sets the maximum CP value to report
        /// </summary>
        [JsonPropertyName("max_cp")]
        public uint MaximumCP { get; set; }

        /// <summary>
        /// Gets or sets the minimum level value to report
        /// </summary>
        [JsonPropertyName("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum level value to report
        /// </summary>
        [JsonPropertyName("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Gets or sets the Pokemon gender to filter by
        /// </summary>
        [JsonPropertyName("gender")]
        public char Gender { get; set; }

        /// <summary>
        /// Gets or sets the Pokemon size to filter by
        /// </summary>
        [JsonPropertyName("size")]
        public PokemonSize? Size { get; set; }

        /// <summary>
        /// Gets or sets a value determining to filter only great league PvP eligible Pokemon
        /// </summary>
        [JsonPropertyName("great_league")]
        public bool IsPvpGreatLeague { get; set; }

        /// <summary>
        /// Gets or sets a value determining to filter only ultra league PvP eligible Pokemon
        /// </summary>
        [JsonPropertyName("ultra_league")]
        public bool IsPvpUltraLeague { get; set; }

        /// <summary>
        /// Gets or sets the minimum PvP rank to report
        /// </summary>
        [JsonPropertyName("min_rank")]
        public uint MinimumRank { get; set; }

        /// <summary>
        /// Gets or sets the maximum PvP rank to report
        /// </summary>
        [JsonPropertyName("max_rank")]
        public uint MaximumRank { get; set; }

        //TODO: Filter by move?

        /// <summary>
        /// Gets or sets the Pokemon filter type
        /// </summary>
        [JsonPropertyName("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether a Pokemon was checked with an event account
        /// </summary>
        [JsonPropertyName("is_event")]
        public bool IsEvent { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to ignore Pokemon missing stats
        /// </summary>
        [JsonPropertyName("ignore_missing")]
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterPokemonObject"/> class.
        /// </summary>
        public WebhookFilterPokemon()
        {
            Pokemon = new List<uint>();
            Forms = new List<string>();
            Costumes = new List<string>();
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