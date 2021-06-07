namespace WhMgr.Osm.Models
{
    using System.Text.Json.Serialization;

    public class OsmFeature
    {
        [JsonPropertyName("geometry")]
        public OsmFeatureGeometry Geometry { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("properties")]
        public OsmFeatureProperties Properties { get; set; }
    }
}