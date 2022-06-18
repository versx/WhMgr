namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Alarms.Filters;

    public class EventPokemonConfig
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable the event Pokemon
        /// filtering.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the event Pokemon IDs list.
        /// </summary>
        [JsonPropertyName("pokemonIds")]
        public List<uint> PokemonIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the minimum IV value for an event Pokemon to be to process
        /// for channel alarms or direct message subscriptions.
        /// </summary>
        [JsonPropertyName("minimumIV")]
        public int MinimumIV { get; set; } = 90;

        // TODO: Add minimumRank EventPokemon config property

        /// <summary>
        /// Gets or sets the event pokemon filter type.
        /// 
        /// Explaination: Filtering type to use with deemed "event" Pokemon.
        /// Set to `Include` if you do not want the Pokemon reported unless
        /// it meets the minimumIV value set (or is 0% or has PvP ranks).
        /// Set to `Exclude` if you only want the Pokemon reported if it meets
        /// the minimum IV value set. No other Pokemon will be reported other
        /// than those in the event list. 
        ///
        /// </summary>
        [JsonPropertyName("type")]
        public FilterType FilterType { get; set; } = FilterType.Include;

        /// <summary>
        /// Gets or sets a value determining whether to ignore event Pokemon
        /// missing IV stats.
        /// </summary>
        [JsonPropertyName("ignoreMissingStats")]
        public bool IgnoreMissingStats { get; set; } = true;
}
}