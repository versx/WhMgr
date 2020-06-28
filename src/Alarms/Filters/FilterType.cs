namespace WhMgr.Alarms.Filters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Filter type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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