namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class GeofenceService
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

        public static bool Contains(GeofenceItem geofence, Location point)
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

        /*
        public static bool Contains(GeofenceItem geofence, Location point)
        {
            var lats = geofence.Polygons.Select(x => x.Latitude).ToList();
            var lngs = geofence.Polygons.Select(x => x.Longitude).ToList();
            var length = lats.Count; // -1?
            var odd = false;
            var pX = lats;
            var pY = lngs;
            for (var i = 0; i < lats.Count; i++)
            {
                if (((pY[i] < point.Longitude
                    && pY[length] >= point.Longitude)
                    || (pY[length] < point.Longitude
                    && pY[i] >= point.Longitude))
                    && (pX[i] <= point.Latitude
                    || pX[length] <= point.Latitude))
                {
                    odd ^= (pX[i] + (point.Longitude - pY[i])
                        * (pX[length] - pX[i])
                        / (pY[length] - pY[i]))
                        < point.Latitude;
                }
                length = i;
            }
            return odd;
        }
        */

        /// <summary>
        /// Check if the provided location is within one of the provided geofences.
        /// </summary>
        /// <param name="geofences">List of geofences</param>
        /// <param name="location">Location to check</param>
        /// <returns></returns>
        public static GeofenceItem InGeofence(List<GeofenceItem> geofences, Location location)
        {
            for (var i = 0; i < geofences.Count; i++)
            {
                var geofence = geofences[i];
                if (!Contains(geofence, location))
                    continue;

                return geofence;
            }

            return null;
        }

        public static GeofenceItem GetGeofence(List<GeofenceItem> geofences, Location point)
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
    }
}