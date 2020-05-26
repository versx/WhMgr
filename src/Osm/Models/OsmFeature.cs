namespace WhMgr.Osm.Models
{
    using Newtonsoft.Json;

    public class OsmFeature
    {
        [JsonProperty("geometry")]
        public OsmFeatureGeometry Geometry { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("properties")]
        public OsmFeatureProperties Properties { get; set; }
    }
}