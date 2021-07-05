namespace WhMgr.Common
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PvpLeague
    {
        Other = 0,
        Great = 1500,
        Ultra = 2500,
        Master = 99999,
    }
}