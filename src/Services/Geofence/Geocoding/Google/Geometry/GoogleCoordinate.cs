namespace WhMgr.Services.Geofence.Geocoding.Google.Geometry
{
    using System.Text.Json.Serialization;

    public class GoogleCoordinate
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }
}