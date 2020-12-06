using System;
using System.IO;
using WhMgr.Diagnostics;

namespace WhMgr.Geofence
{
    using System.Collections.Generic;
    using System.Linq;

    public static class GeofenceService
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("GEOFENCE", Program.LogLevel);
        
        public static List<GeofenceItem> LoadGeofences(string geofencesFolder)
        {
            var geofences = new List<GeofenceItem>();

            foreach (var file in Directory.EnumerateFiles(geofencesFolder))
            {
                try
                {
                    var fileGeofences = GeofenceItem.FromFile(file);

                    geofences.AddRange(fileGeofences);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Could not load Geofence file {file}:");
                    _logger.Error(ex);
                }
            }

            return geofences;
        }
        
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