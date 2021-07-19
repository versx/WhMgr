namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SubscriptionAccessType
    {
        Pokemon = 0,
        PvP,
        Raids,
        Quests,
        Invasions,
        Lures,
        Gyms
    }
}