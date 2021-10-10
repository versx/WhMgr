namespace WhMgr.Services.Icons
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IconType
    {
        Base = 0,
        Pokemon,
        Raid,
        Egg,
        Gym,
        Pokestop,
        Reward,
        Invasion,
        Type,
        Nest,
        Team,
        Weather,
        Misc,
    }
}