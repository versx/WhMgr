namespace WhMgr.Services.Alarms.Filters.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    /// <summary>
    /// Raid boss filters
    /// </summary>
    public class WebhookFilterRaid : IWebhookFilterPokemonDetails
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable the raid boss filter
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// List of raid levels
        /// </summary>
        [JsonPropertyName("levels")]
        public IReadOnlyList<ushort> Levels { get; set; }

        /// <summary>
        /// Gets or sets the Raid boss pokedex ID list to filter against
        /// </summary>
        [JsonPropertyName("pokemon")]
        public IReadOnlyList<uint> Pokemon { get; set; }

        /// <summary>
        /// Gets or sets the list of Raid Boss Pokemon Form strings to filter against
        /// </summary>
        [JsonPropertyName("forms")]
        public IReadOnlyList<string> Forms { get; set; }

        /// <summary>
        /// Gets or sets the list of Raid Boss Pokemon costume strings to filter against
        /// </summary>
        [JsonPropertyName("costumes")]
        public IReadOnlyList<string> Costumes { get; set; }

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
        /// Gets or sets the Gym power level filtering options
        /// </summary>
        [JsonPropertyName("power_level")]
        public WebhookFilterGymLevel PowerLevel { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to ignore raids missing stats
        /// </summary>
        [JsonPropertyName("ignore_missing")]
        public bool IgnoreMissing { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="WebhookFilterRaid"/> class
        /// </summary>
        public WebhookFilterRaid()
        {
            Pokemon = new List<uint>();
            Forms = new List<string>();
            Costumes = new List<string>();
            Levels = Enumerable.Range(Strings.Defaults.MinimumRaidLevel, Strings.Defaults.MaximumRaidLevel)
                .Select(Convert.ToUInt16)
                .ToList();
            Team = PokemonTeam.All;
        }
    }
}