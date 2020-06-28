namespace WhMgr.Net.Models
{
    /// <summary>
    /// Gym team
    /// </summary>
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