namespace WhMgr.Osm.Models
{
    using System.Text.Json.Serialization;

    public class OsmFeatureProperties
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("fill")]
        public string Fill { get; set; }

        [JsonPropertyName("fill-opacity")]
        public double FillOpacity { get; set; }

        [JsonPropertyName("stroke")]
        public string Stroke { get; set; }

        [JsonPropertyName("stroke-opacity")]
        public double StrokeOpacity { get; set; }

        [JsonPropertyName("stroke-width")]
        public double StrokeWidth { get; set; }
    }
}