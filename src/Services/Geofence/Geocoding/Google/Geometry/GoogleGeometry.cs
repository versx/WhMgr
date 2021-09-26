namespace WhMgr.Services.Geofence.Geocoding.Google.Geometry
{
    using System.Text.Json.Serialization;

    public class GoogleGeometry
    {
        [JsonPropertyName("bounds")]
        public GoogleGeometryBounds Bounds { get; set; }

        [JsonPropertyName("location")]
        public GoogleCoordinate Location { get; set; }

        [JsonPropertyName("location_type")]
        public string LocationType { get; set; }

        [JsonPropertyName("viewport")]
        public GoogleGeometryBounds ViewPort { get; set; }
    }
}