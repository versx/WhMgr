namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    public class WebhookFilterInvasion
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Invasion types to report
        /// </summary>
        [JsonPropertyName("invasion_types")]
        public Dictionary<InvasionCharacter, bool> InvasionTypes { get; set; } = new();

        // TODO: Filter by invasion reward
    }
}