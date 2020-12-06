namespace WhMgr.Geofence
{
    using System.Collections.Generic;
    using System.Linq;

    public static class GeofenceService
    {
        public static GeofenceItem GetGeofence(IEnumerable<GeofenceItem> geofences, Location point)
        {
            // Order descending by priority so that when we iterate forwards using FirstOrDefault, higher-priority
            // geofences are discovered first
            var orderedGeofences = geofences.OrderByDescending(g => g.Priority);

            return orderedGeofences.FirstOrDefault(g => g.Feature.Geometry.Contains(point));
        }
    }
}