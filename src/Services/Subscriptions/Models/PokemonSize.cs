namespace WhMgr.Services.Subscriptions.Models
{
    /// <summary>
    /// Size of Pokemon
    /// </summary>
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