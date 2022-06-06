namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Alarms.Filters;

    public class EventPokemonConfig
    {
        /// <summary>
        /// Gets or sets the event Pokemon IDs list
        /// </summary>
        [JsonPropertyName("pokemonIds")]
        public List<uint> PokemonIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the minimum IV value for an event Pokemon to be to process
        /// for channel alarms or direct message subscriptions
        /// </summary>
        [JsonPropertyName("minimumIV")]
        public int MinimumIV { get; set; } = 90;

        /// <summary>
        /// Gets or sets the event pokemon filter type
        /// </summary>
        [JsonPropertyName("type")]
        public FilterType FilterType { get; set; }
    }
}