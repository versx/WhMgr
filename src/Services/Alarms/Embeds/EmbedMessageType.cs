namespace WhMgr.Services.Alarms.Embeds
{
    /// <summary>
    /// Discord alert message type
    /// </summary>
    public enum EmbedMessageType
    {
        /// <summary>
        /// Pokemon alert
        /// </summary>
        Pokemon = 0,

        /// <summary>
        /// Pokemon missing stats alert
        /// </summary>
        PokemonMissingStats,

        /// <summary>
        /// Gym alert
        /// </summary>
        Gyms,

        /// <summary>
        /// Raid alert
        /// </summary>
        Raids,

        /// <summary>
        /// Raid egg alert
        /// </summary>
        Eggs,

        /// <summary>
        /// Pokestop alert
        /// </summary>
        Pokestops,

        /// <summary>
        /// Field research quest alert
        /// </summary>
        Quests,

        /// <summary>
        /// Team Rocket invasion alert
        /// </summary>
        Invasions,

        /// <summary>
        /// Pokestop lure alert
        /// </summary>
        Lures,

        /// <summary>
        /// Pokemon nest alert
        /// </summary>
        Nests,

        /// <summary>
        /// Weather cell alert
        /// </summary>
        Weather
    }
}