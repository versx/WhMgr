namespace WhMgr.Services.Geofence.Geocoding.Google
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    // TODO: Add more Google entity models?
    // https://github.com/Necrobot-Private/NecroBot/tree/master/PoGo.NecroBot.Logic/Model/Google/GoogleObjects
    public class GoogleReverseLookup
    {
        [JsonPropertyName("results")]
        public List<GoogleAddressResult> Results { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}