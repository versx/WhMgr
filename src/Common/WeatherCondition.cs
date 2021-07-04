namespace WhMgr.Common
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeatherCondition
    {
        None = 0,
        Clear,
        Rainy,
        PartlyCloudy,
        Overcast,
        Windy,
        Snow,
        Fog,
    }
}