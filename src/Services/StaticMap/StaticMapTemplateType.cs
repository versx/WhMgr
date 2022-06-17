namespace WhMgr.Services.StaticMap
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaticMapTemplateType
    {
        StaticMap,
        MultiStaticMap,
    }
}