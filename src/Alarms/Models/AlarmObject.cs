namespace WhMgr.Alarms.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Filters.Models;
    using WhMgr.Geofence;

    [JsonObject("alarm")]
    public class AlarmObject
    {
        [JsonIgnore]
        public List<GeofenceItem> Geofences { get; private set; }

        [JsonIgnore]
        public AlertMessage Alerts { get; private set; }

        [JsonIgnore]
        public FilterObject Filters { get; private set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("filters")]
        public string FiltersFile { get; set; }

        [JsonProperty("alerts")]
        public string AlertsFile { get; set; }

        [JsonProperty("geofence")]
        public string GeofenceFile { get; set; }

        [JsonProperty("webhook")]
        public string Webhook { get; set; }

        public AlarmObject()
        {
            LoadGeofence();
            LoadAlerts();
            LoadFilters();
        }

        public List<GeofenceItem> LoadGeofence()
        {
            if (string.IsNullOrEmpty(GeofenceFile))
                return null;

            var path = Path.Combine(Strings.GeofenceFolder, GeofenceFile);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Geofence file {path} not found.", path);

            return Geofences = GeofenceItem.FromFile(path);
        }

        public AlertMessage LoadAlerts()
        {
            if (string.IsNullOrEmpty(AlertsFile))
                return null;

            var path = Path.Combine(Strings.AlertsFolder, AlertsFile);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Alert file {path} not found.", path);

            var data = File.ReadAllText(path);
            return Alerts = JsonConvert.DeserializeObject<AlertMessage>(data);
        }

        public FilterObject LoadFilters()
        {
            if (string.IsNullOrEmpty(FiltersFile))
                return null;

            var path = Path.Combine(Strings.FiltersFolder, FiltersFile);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Filter file {path} not found.", path);

            var data = File.ReadAllText(path);
            return Filters = JsonConvert.DeserializeObject<FilterObject>(data);
        }
    }
}