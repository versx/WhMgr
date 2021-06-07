namespace WhMgr.Osm.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class OsmFeatureGeometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("coordinates")]
        public List<MultiPolygon> Coordinates { get; set; }
    }

    public class Polygon : List<double> { }

    public class MultiPolygon : List<Polygon> { }
}