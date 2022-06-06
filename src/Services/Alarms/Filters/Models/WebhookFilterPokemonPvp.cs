namespace WhMgr.Services.Alarms.Filters.Models
{
    using System.Text.Json.Serialization;

    using static POGOProtos.Rpc.BelugaPokemonProto.Types;

    public class WebhookFilterPokemonPvp
    {
        /// <summary>
        /// Gets or sets the minimum PvP rank to report
        /// </summary>
        [JsonPropertyName("min_rank")]
        public ushort MinimumRank { get; set; }

        /// <summary>
        /// Gets or sets the maximum PvP rank to report
        /// </summary>
        [JsonPropertyName("max_rank")]
        public ushort MaximumRank { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("min_percent")]
        public double MinimumPercent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("max_percent")]
        public double MaximumPercent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("min_cp")]
        public uint MinimumCP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("max_cp")]
        public uint MaximumCP { get; set; }

        /// <summary>
        /// Gender requirement filter for PVP rank
        /// </summary>
        [JsonPropertyName("gender")]
        public char Gender { get; set; }

        public WebhookFilterPokemonPvp()
        {
            MinimumCP = 0;
            MaximumCP = 999999;
            MinimumRank = 1;
            MaximumRank = 100;
            // TODO: Double check percent logic
            MinimumPercent = 0;
            MaximumPercent = 100;
            Gender = '*';
        }
    }
}