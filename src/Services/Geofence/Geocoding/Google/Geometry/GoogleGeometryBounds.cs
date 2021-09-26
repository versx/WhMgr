namespace WhMgr.Services.Geofence.Geocoding.Google.Geometry
{
    using System.Text.Json.Serialization;

    public class GoogleGeometryBounds
    {
        [JsonPropertyName("northeast")]
        public GoogleCoordinate NorthEast { get; set; }

        [JsonPropertyName("southwest")]
        public GoogleCoordinate SouthWest { get; set; }
    }
}