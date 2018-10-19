namespace T.Alarms.Models
{
    using Newtonsoft.Json;

    using T.Alarms.Filters.Models;
    using T.Geofence;

    [JsonObject("alarm")]
    public class AlarmObject
    {
        [JsonIgnore]
        public GeofenceItem Geofence { get; private set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("filters")]
        public FilterObject Filters { get; set; }

        [JsonProperty("geofence")]
        public string GeofenceFile { get; set; }

        [JsonProperty("webhook")]
        public string Webhook { get; set; }

        public AlarmObject()
        {
            LoadGeofence();
        }

        public void LoadGeofence()
        {
            if (!string.IsNullOrEmpty(GeofenceFile))
            {
                Geofence = GeofenceItem.FromFile(GeofenceFile);
            }
        }
    }
}