namespace WhMgr.Services.Webhook.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Size of Pokemon
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PokemonSize : byte
    {
        /// <summary>
        /// All Pokemon sizes
        /// </summary>
        All = 0,

        /// <summary>
        /// Tiny or extra small sized Pokemon
        /// </summary>
        Tiny,

        /// <summary>
        /// Small sized Pokemon
        /// </summary>
        Small,

        /// <summary>
        /// Normal sized Pokemon
        /// </summary>
        Normal,

        /// <summary>
        /// Large sized Pokemon
        /// </summary>
        Large,

        /// <summary>
        /// Big or extra large sized Pokemon
        /// </summary>
        Big,
    }
}