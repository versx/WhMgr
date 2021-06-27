namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Webhook.Models;

    /// <summary>
    /// Raid boss filters
    /// </summary>
    public class WebhookFilterRaid
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable the raid boss filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the minimum raid level
        /// </summary>
        [JsonPropertyName("min_lvl")]
        public uint MinimumLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum raid level
        /// </summary>
        [JsonPropertyName("max_lvl")]
        public uint MaximumLevel { get; set; }

        /// <summary>
        /// Gets or sets the Raid boss pokedex ID list to filter against
        /// </summary>
        //TODO: Allow pokemon names and ids for raid filter.
        [JsonPropertyName("pokemon")]
        public List<uint> Pokemon { get; set; }

        /// <summary>
        /// Gets or sets the list of Raid Boss Pokemon Form strings to filter against
        /// </summary>
        [JsonPropertyName("forms")]
        public List<string> Forms { get; set; }

        /// <summary>
        /// Gets or sets the list of Raid Boss Pokemon costume strings to filter against
        /// </summary>
        [JsonPropertyName("costumes")]
        public List<string> Costumes { get; set; }

        /// <summary>
        /// Gets or sets the Raid boss filter type
        /// </summary>
        [JsonPropertyName("type")]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to only report ex-eligible raids
        /// </summary>
        [JsonPropertyName("only_ex")]
        public bool OnlyEx { get; set; }

        /// <summary>
        /// Gets or sets the Gym team control filter
        /// </summary>
        [JsonPropertyName("team")]
        public PokemonTeam Team { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to ignore raids missing stats
        /// </summary>
        [JsonPropertyName("ignore_missing")]
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FilterRaidObject"/> class
        /// </summary>
        public WebhookFilterRaid()
        {
            Pokemon = new List<uint>();
            Forms = new List<string>();
            Costumes = new List<string>();
            MinimumLevel = 1;
            MaximumLevel = 5;
            Team = PokemonTeam.All;
        }
    }
}