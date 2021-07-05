namespace WhMgr.Services.Geofence.Nominatim
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// NominatimReverseLookup class
    /// </summary>
    public partial class NominatimReverseLookup
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }

        [JsonPropertyName("licence")]
        public string Licence { get; set; }

        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }

        [JsonPropertyName("lat")]
        public string Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string Longitude { get; set; }

        [JsonPropertyName("place_rank")]
        public long PlaceRank { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("importance")]
        public double Importance { get; set; }

        [JsonPropertyName("addresstype")]
        public string Addresstype { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("address")]
        public NominatimAddress Address { get; set; }

        [JsonPropertyName("boundingbox")]
        public string[] Boundingbox { get; set; }
    }
}