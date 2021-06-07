namespace WhMgr.Services.Webhook.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Gym team
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PokemonTeam
    {
        /// <summary>
        /// Neutral or Team Harmody
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// Team Mystic (Blue)
        /// </summary>
        Mystic,

        /// <summary>
        /// Team Valor (Red)
        /// </summary>
        Valor,

        /// <summary>
        /// Team Instinct (Yellow)
        /// </summary>
        Instinct,

        /// <summary>
        /// All gym teams
        /// </summary>
        All = ushort.MaxValue
    }
}