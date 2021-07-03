namespace WhMgr.Common
{
    /// <summary>
    /// Pokemon generation range class
    /// </summary>
    public class PokemonGenerationRange
    {
        /// <summary>
        /// Gets or sets the Pokemon generation number
        /// </summary>
        public int Generation { get; set; }

        /// <summary>
        /// Gets or sets the pokedex ID the generation starts at
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the pokedex ID the generation ends at
        /// </summary>
        public int End { get; set; }
    }
}