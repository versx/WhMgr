namespace WhMgr.Osm.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class OsmFeatureGeometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public List<MultiPolygon> Coordinates { get; set; }
    }

    public class Polygon : List<double> { }

    public class MultiPolygon : List<Polygon> { }
}