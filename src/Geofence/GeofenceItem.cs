namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class GeofenceItem
    {
        #region Properties

        public string Name { get; set; }

        public List<Location> Polygons { get; }

        #endregion

        #region Constructor(s)

        public GeofenceItem()
        {
            Name = "Unnamed";
            Polygons = new List<Location>();
        }

        public GeofenceItem(string name)
        {
            Name = name;
            Polygons = new List<Location>();
        }

        public GeofenceItem(string name, List<Location> polygons) : this()
        {
            Name = name;
            Polygons = polygons;
        }

        #endregion

        #region Static Methods

        public static GeofenceItem FromFile(string filePath)
        {
            var geofence = new GeofenceItem();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    geofence.Name = line.TrimStart('[').TrimEnd(']');
                    continue;
                }

                var coordinates = line.Replace(" ", null).Split(',');
                if (!double.TryParse(coordinates[0], out var lat))
                    continue;

                if (!double.TryParse(coordinates[1], out var lng))
                    continue;

                geofence.Polygons.Add(new Location(lat, lng));
            }

            return geofence;
        }

        #endregion
    }
}