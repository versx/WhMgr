namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using WhMgr.Diagnostics;

    public class GeofenceService
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public bool Contains(GeofenceItem geofence, Location point)
        {
            //Credits: https://stackoverflow.com/a/7739297/2313836

            var contains = false;
            for (int i = 0, j = geofence.Polygons.Count - 1; i < geofence.Polygons.Count; j = i++)
            {
                if ((((geofence.Polygons[i].Latitude <= point.Latitude) && (point.Latitude < geofence.Polygons[j].Latitude))
                        || ((geofence.Polygons[j].Latitude <= point.Latitude) && (point.Latitude < geofence.Polygons[i].Latitude)))
                        && (point.Longitude < (geofence.Polygons[j].Longitude - geofence.Polygons[i].Longitude) * (point.Latitude - geofence.Polygons[i].Latitude)
                            / (geofence.Polygons[j].Latitude - geofence.Polygons[i].Latitude) + geofence.Polygons[i].Longitude))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        public bool InPolygon(GeofenceItem geofence, Location point)
        {
            /*
                self.portalInPolygon = function portalInPolygon(polygon, portal) {
                    var poly = polygon.getLatLngs();
                    var pt = portal.getLatLng();
                    var c = false;
                    for (var i = -1, l = poly.length, j = l - 1; ++i < l; j = i) {
                        ((poly[i].lat <= pt.lat && pt.lat < poly[j].lat) || (poly[j].lat <= pt.lat && pt.lat < poly[i].lat)) && (pt.lng < (poly[j].lng - poly[i].lng) * (pt.lat - poly[i].lat) / (poly[j].lat - poly[i].lat) + poly[i].lng) && (c = !c);
                    }
                    return c;
                };
             */

            var poly = geofence.Polygons;
            var c = false;
            for (int i = -1, l = poly.Count, j = l - 1; ++i < l; j = i)
            {
                c = ((poly[i].Latitude <= point.Latitude && point.Latitude < poly[j].Latitude) || (poly[j].Latitude <= point.Latitude && point.Latitude < poly[i].Latitude)) &&
                    (point.Longitude < (poly[j].Longitude - poly[i].Longitude) * (point.Latitude - poly[i].Latitude) / (poly[j].Latitude - poly[i].Latitude) + poly[i].Longitude) &&
                    (c = !c);
            }
            return c;
        }

        public GeofenceItem GetGeofence(List<GeofenceItem> geofences, Location point)
        {
            foreach (var geofence in geofences)
            {
                if (Contains(geofence, point))
                {
                    return geofence;
                }
            }

            return null;
        }

        //public static List<GeofenceItem> FromFiles(List<string> filePaths)
        //{
        //    var list = new List<GeofenceItem>();

        //    foreach (var filePath in filePaths)
        //    {
        //        if (!File.Exists(filePath))
        //        {
        //            _logger.Warn($"Geofence file {filePath} does not exist.");
        //            continue;
        //        }

        //        list.Add(GeofenceItem.FromFile(filePath));
        //    }

        //    return list;
        //}

        //public static List<GeofenceItem> FromFolder(string geofenceFolder)
        //{
        //    return FromFiles(Directory.GetFiles(geofenceFolder, "*.txt").ToList());
        //}

        //public static List<GeofenceItem> FromFolder(string geofenceFolder, List<string> cities)
        //{
        //    var list = new List<GeofenceItem>();
        //    foreach (var city in cities)
        //    {
        //        var filePath = Path.Combine(geofenceFolder, city + ".txt");
        //        if (!File.Exists(filePath))
        //        {
        //            _logger.Warn($"Geofence file {filePath} does not exist.");
        //            continue;
        //        }

        //        list.Add(GeofenceItem.FromFile(filePath));
        //    }
        //    return list;
        //}
    }
}