namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class GeofenceService
    {
        public static GeofenceItem GetGeofence(List<GeofenceItem> geofences, Location point)
        {
            var containingGeofences = geofences.Where(geofence => geofence.Feature.Geometry.Contains(point)).ToList();

            if (!containingGeofences.Any())
                return null;

            int GetPriority(GeofenceItem geofence)
            {
                try
                {
                    return Convert.ToInt32(geofence.Feature.Attributes["priority"]);
                }
                catch
                {
                    return 0;
                }
            }

            return containingGeofences.OrderByDescending(GetPriority).FirstOrDefault();
        }
    }
}