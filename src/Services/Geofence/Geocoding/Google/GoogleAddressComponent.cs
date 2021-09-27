namespace WhMgr.Services.Geofence.Geocoding.Google
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GoogleAddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("types")]
        public List<string> Types { get; set; }
    }
}