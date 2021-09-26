namespace WhMgr.Services.Geofence.Geocoding.Google
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GoogleReverseLookup
    {
        [JsonPropertyName("plus_code")]
        public dynamic PlusCode { get; set; }

        [JsonPropertyName("results")]
        public List<GoogleAddressResult> Results { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}