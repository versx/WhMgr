namespace WhMgr.Geofence
{
    using System.Collections.Generic;
    using System.Linq;

    public static class GeofenceService
    {
        public static IEnumerable<GeofenceItem> GetGeofences(IEnumerable<GeofenceItem> geofences, Location point)
        {
            // Order descending by priority so that when we iterate forwards using FirstOrDefault, higher-priority
            // geofences are discovered first. Filters initially by bounding box (which is very fast) and then by
            // actual geometry after that.
            var orderedGeofences = geofences.OrderByDescending(g => g.Priority);
            var possibleContaining = orderedGeofences.Where(g => g.BBox.Contains(point));

            return possibleContaining.Where(g => g.Feature.Geometry.Contains(point));
        }

        public static GeofenceItem GetGeofence(IEnumerable<GeofenceItem> geofences, Location point)
            => GetGeofences(geofences, point).FirstOrDefault();
    }
}