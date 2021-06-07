namespace WhMgr.Services.Geofence
{
    using System.Collections.Generic;
    using System.Linq;

    public class GeofenceService : IGeofenceService
    {
        public static IEnumerable<Geofence> GetGeofences(IEnumerable<Geofence> geofences, Coordinate point)
        {
            // Order descending by priority so that when we iterate forwards using FirstOrDefault, higher-priority
            // geofences are discovered first. Filters initially by bounding box (which is very fast) and then by
            // actual geometry after that.
            var orderedGeofences = geofences.OrderByDescending(g => g.Priority);
            var possibleContaining = orderedGeofences.Where(g => g.BBox.Contains(point));

            return possibleContaining.Where(g => g.Feature.Geometry.Contains(point));
        }

        public static Geofence GetGeofence(IEnumerable<Geofence> geofences, Coordinate point)
            => GetGeofences(geofences, point).FirstOrDefault();
    }
}