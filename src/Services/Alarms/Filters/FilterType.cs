namespace WhMgr.Services.Alarms.Filters
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Filter type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterType
    {
        /// <summary>
        /// Include filter type
        /// </summary>
        Include = 0,

        /// <summary>
        /// Exclude filter type
        /// </summary>
        Exclude
    }
}