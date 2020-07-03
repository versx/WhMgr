namespace WhMgr.Osm.Models
{
    using Newtonsoft.Json;

    public class OsmFeatureProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fill")]
        public string Fill { get; set; }

        [JsonProperty("fill-opacity")]
        public double FillOpacity { get; set; }

        [JsonProperty("stroke")]
        public string Stroke { get; set; }

        [JsonProperty("stroke-opacity")]
        public double StrokeOpacity { get; set; }

        [JsonProperty("stroke-width")]
        public double StrokeWidth { get; set; }
    }
}