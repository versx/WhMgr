namespace WhMgr.Services.Webhook.Models
{
    /// <summary>
    /// Pokestop lure type
    /// </summary>
    public enum PokestopLureType : ushort
    {
        /// <summary>
        /// No Pokestop lure deployed
        /// </summary>
        None = 0,

        /// <summary>
        /// Normal Pokestop lure deployed
        /// </summary>
        Normal = 501,

        /// <summary>
        /// Glacial Pokestop lure deployed
        /// </summary>
        Glacial = 502,

        /// <summary>
        /// Mossy Pokestop lure deployed
        /// </summary>
        Mossy = 503,

        /// <summary>
        /// Magnetic Pokestop lure deployed
        /// </summary>
        Magnetic = 504,

        /// <summary>
        /// Rainy Pokestop lure deployed
        /// </summary>
        Rainy = 505,
    }
}