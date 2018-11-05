namespace WhMgr.Alarms.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Alarms.Filters.Models;
    using WhMgr.Geofence;

    [JsonObject("alarm")]
    public class AlarmObject
    {
        [JsonIgnore]
        public List<GeofenceItem> Geofences { get; private set; }

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

        public List<GeofenceItem> LoadGeofence()
        {
            if (string.IsNullOrEmpty(GeofenceFile))
                return null;

            var path = Path.Combine(Strings.GeofenceFolder, GeofenceFile);
            if (!File.Exists(path))
                throw new FileNotFoundException("Geofence file not found.", path);

            return Geofences = GeofenceItem.FromFile(path);
        }
    }
}