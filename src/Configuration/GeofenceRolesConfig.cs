namespace WhMgr.Configuration
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Configuration file determining if geofences should be used as
    /// assignable roles
    /// </summary>
    public class GeofenceRolesConfig
    {
        /// <summary>
        /// Gets or sets a value determining whether to enable assigning
        /// geofence/area/city roles or not
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether geofence roles should be
        /// removed when a donor role is removed from a Discord member
        /// </summary>
        [JsonPropertyName("autoRemove")]
        public bool AutoRemove { get; set; } = true;

        /// <summary>
        /// Gets or sets whether geofence roles require a Donor role
        /// </summary>
        [JsonPropertyName("requiresDonorRole")]
        public bool RequiresDonorRole { get; set; }
    }
}