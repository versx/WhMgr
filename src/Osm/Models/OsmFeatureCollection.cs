namespace WhMgr.Osm.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class OsmFeatureCollection
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        public List<OsmFeature> Features { get; set; }
    }
}