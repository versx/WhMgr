namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class GeofenceService
    {
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

        public static List<GeofenceItem> FromFiles(List<string> filePaths)
        {
            var list = new List<GeofenceItem>();

            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Geofence file {filePath} does not exist.");
                    continue;
                }

                list.Add(GeofenceItem.FromFile(filePath));
            }

            return list;
        }

        public static List<GeofenceItem> FromFolder(string geofenceFolder)
        {
            return FromFiles(Directory.GetFiles(geofenceFolder, "*.txt").ToList());
        }

        public static List<GeofenceItem> FromFolder(string geofenceFolder, List<string> cities)
        {
            var list = new List<GeofenceItem>();
            foreach (var city in cities)
            {
                var filePath = Path.Combine(geofenceFolder, city + ".txt");
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Geofence file {filePath} does not exist.");
                    continue;
                }

                list.Add(GeofenceItem.FromFile(filePath));
            }
            return list;
        }
    }
}