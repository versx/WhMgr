namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using WhMgr.Diagnostics;

    public class GeofenceService
    {
        //private static readonly IEventLogger _logger = EventLogger.GetLogger();

        //public bool Contains(GeofenceItem geofence, Location point)
        //{
        //    //Credits: https://stackoverflow.com/a/7739297/2313836

        //    var contains = false;
        //    for (int i = 0, j = geofence.Polygons.Count - 1; i < geofence.Polygons.Count; j = i++)
        //    {
        //        if ((((geofence.Polygons[i].Latitude <= point.Latitude) && (point.Latitude < geofence.Polygons[j].Latitude))
        //                || ((geofence.Polygons[j].Latitude <= point.Latitude) && (point.Latitude < geofence.Polygons[i].Latitude)))
        //                && (point.Longitude < (geofence.Polygons[j].Longitude - geofence.Polygons[i].Longitude) * (point.Latitude - geofence.Polygons[i].Latitude)
        //                    / (geofence.Polygons[j].Latitude - geofence.Polygons[i].Latitude) + geofence.Polygons[i].Longitude))
        //        {
        //            contains = !contains;
        //        }
        //    }
        //    return contains;
        //}

        //public bool InPolygon(GeofenceItem geofence, Location point)
        //{
        //    var poly = geofence.Polygons;
        //    var c = false;
        //    for (int i = -1, l = poly.Count, j = l - 1; ++i < l; j = i)
        //    {
        //        c = ((poly[i].Latitude <= point.Latitude && point.Latitude < poly[j].Latitude) || (poly[j].Latitude <= point.Latitude && point.Latitude < poly[i].Latitude)) &&
        //            (point.Longitude < (poly[j].Longitude - poly[i].Longitude) * (point.Latitude - poly[i].Latitude) / (poly[j].Latitude - poly[i].Latitude) + poly[i].Longitude) &&
        //            (c = !c);
        //    }
        //    return c;
        //}
        public bool Contains(GeofenceItem geofence, Location point)
        {
            var numOfPoints = geofence.Polygons.Count;
            var lats = geofence.Polygons.Select(x => x.Latitude).ToList();
            var lngs = geofence.Polygons.Select(x => x.Longitude).ToList();
            var polygonContainsPoint = false;
            for (int node = 0, altNode = (numOfPoints - 1); node < numOfPoints; altNode = node++)
            {
                if ((lngs[node] > point.Longitude != (lngs[altNode] > point.Longitude))
                    && (point.Latitude < (lats[altNode] - lats[node])
                                       * (point.Longitude - lngs[node])
                                       / (lngs[altNode] - lngs[node])
                                       + lats[node]
                )
            )
                {
                    polygonContainsPoint = !polygonContainsPoint;
                }
            }
            lats.Clear();
            lngs.Clear();
            return polygonContainsPoint;
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