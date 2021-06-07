namespace WhMgr.Services.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Pokemon filters
    /// </summary>
    public class WebhookFilterPokestop
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable the pokestop filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to only report lured Pokestops
        /// </summary>
        [JsonPropertyName("lured")]
        public bool Lured { get; set; }

        /// <summary>
        /// Gets or sets the Pokestop lure types to report
        /// </summary>
        [JsonPropertyName("lure_types")]
        public List<string> LureTypes { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to only report Team Rocket invasion Pokestops
        /// </summary>
        [JsonPropertyName("invasions")]
        public bool Invasions { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="WebhookFilterPokestop"/> class
        /// </summary>
        public WebhookFilterPokestop()
        {
            LureTypes = new List<string>();
        }
    }
}