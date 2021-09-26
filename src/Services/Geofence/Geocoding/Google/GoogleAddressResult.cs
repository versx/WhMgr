namespace WhMgr.Services.Geofence.Geocoding.Google
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Geofence.Geocoding.Google.Geometry;

    public class GoogleAddressResult
    {
        [JsonPropertyName("address_components")]
        public List<GoogleAddressComponent> AddressComponents { get; set; }

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("geometry")]
        public GoogleGeometry Geometry { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }

        // TODO: plus_code { compond_code, global_code }

        [JsonPropertyName("types")]
        public List<string> Types { get; set; }
    }
}