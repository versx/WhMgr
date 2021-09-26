namespace WhMgr.Services.Geofence.Geocoding
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReverseGeocodingProvider
    {
        Osm, // OpenStreetMap,
        GMaps, // GoogleMaps,
    }
}