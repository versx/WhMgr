namespace WhMgr.Services.Geofence.Nominatim
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// NominatimAddress class
    /// </summary>
    public partial class NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("road")]
        public string Road { get; set; }

        [JsonPropertyName("neighbourhood")]
        public string Neighbourhood { get; set; }

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }

        [JsonPropertyName("town")]
        public string Town { set { City = value; } }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }
}