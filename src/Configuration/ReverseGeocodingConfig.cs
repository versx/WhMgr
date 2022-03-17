namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    using WhMgr.Services.Geofence.Geocoding;

    public class ReverseGeocodingConfig
    {
        /// <summary>
        /// Gets or sets a value determining which reverse geocoding provider
        /// to use. (Google Maps or OpenStreetMaps Nominatim)
        /// </summary>
        [JsonPropertyName("provider")]
        public ReverseGeocodingProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether to cache reverse geocoding
        /// lookups to disk if set to true, otherwise cache in memory.
        /// </summary>
        [JsonPropertyName("cacheToDisk")]
        public bool CacheToDisk { get; set; }

        /// <summary>
        /// Gets or sets the Google Maps config for reverse geocoding.
        /// </summary>
        [JsonPropertyName("gmaps")]
        public GoogleMapsConfig GoogleMaps { get; set; }

        /// <summary>
        /// Gets or set the OpenStreetMaps Nominatim config for reverse geocoding.
        /// </summary>
        [JsonPropertyName("nominatim")]
        public NominatimConfig Nominatim { get; set; }
    }

    public class GoogleMapsConfig
    {
        /// <summary>
        /// Gets or sets the Google maps key for location lookup
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Google maps location string schema
        /// </summary>
        [JsonPropertyName("schema")]
        public string Schema { get; set; }
    }

    public class NominatimConfig
    {
        /// <summary>
        /// Gets or sets the OpenStreetMaps Nominatim endpoint to use for reverse location lookup
        /// </summary>
        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the OpenStreetMaps Nominatim location string schema
        /// </summary>
        [JsonPropertyName("schema")]
        public string Schema { get; set; }
    }
}