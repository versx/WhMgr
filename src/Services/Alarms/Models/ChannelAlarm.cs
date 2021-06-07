namespace WhMgr.Services.Alarms.Models
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Serialization;

    using WhMgr.Extensions;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Alarms.Filters.Models;
    using WhMgr.Services.Geofence;

    public class ChannelAlarm : IChannelAlarm
    {
        /// <summary>
        /// Gets the Area geofences for the alarm
        /// </summary>
        [JsonIgnore]
        public List<Geofence> GeofenceItems { get; private set; }

        /// <summary>
        /// Gets the Discord alert messages for the alarm
        /// </summary>
        [JsonIgnore]
        public EmbedMessage Embeds { get; private set; }

        /// <summary>
        /// Gets the Alarm filters for the alarm
        /// </summary>
        [JsonIgnore]
        public WebhookFilter Filters { get; private set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("filters")]
        public string FiltersFile { get; set; }

        [JsonPropertyName("embeds")]
        public string EmbedsFile { get; set; }

        [JsonPropertyName("geofences")]
        public List<string> Geofences { get; set; }

        [JsonPropertyName("webhook")]
        public string Webhook { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="AlarmObject"/> class
        /// </summary>
        public ChannelAlarm()
        {
            GeofenceItems = new List<Geofence>();
            Embeds = LoadEmbeds();
            Filters = LoadFilters();
        }

        /// <summary>
        /// Load alerts from the `/Alerts` folder
        /// </summary>
        /// <returns>Returns parsed alert message</returns>
        public EmbedMessage LoadEmbeds()
        {
            if (string.IsNullOrEmpty(EmbedsFile))
                return null;

            var path = Path.Combine(Strings.EmbedsFolder, EmbedsFile);
            return Embeds = Data.MasterFile.LoadInit<EmbedMessage>(path);
        }

        /// <summary>
        /// Load alarm filters from the `/Filters` folder
        /// </summary>
        /// <returns>Returns parsed filters object</returns>
        public WebhookFilter LoadFilters()
        {
            if (string.IsNullOrEmpty(FiltersFile))
                return null;

            var path = Path.Combine(Strings.FiltersFolder, FiltersFile);
            return Filters = Data.MasterFile.LoadInit<WebhookFilter>(path);
        }
    }
}