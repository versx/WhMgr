namespace WhMgr.Services.StaticMap
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaticMapType
    {
        Pokemon,
        Raids,
        Gyms,
        Quests,
        Invasions,
        Lures,
        Weather,
        Nests,
    }
}