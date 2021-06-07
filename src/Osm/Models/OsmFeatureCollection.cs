namespace WhMgr.Osm.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class OsmFeatureCollection
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("features")]
        public List<OsmFeature> Features { get; set; }
    }
}