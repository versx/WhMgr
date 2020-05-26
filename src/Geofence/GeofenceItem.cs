namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class GeofenceItem
    {
        private const string DefaultName = "Unnamed";

        #region Properties

        public string Name { get; set; }

        public List<Location> Polygons { get; }

        #endregion

        #region Constructor(s)

        public GeofenceItem()
        {
            Name = DefaultName;
            Polygons = new List<Location>();
        }

        public GeofenceItem(string name) : this(name ?? DefaultName, new List<Location>())
        {
        }

        public GeofenceItem(string name, List<Location> polygons) : this()
        {
            Name = name ?? DefaultName;
            Polygons = polygons;
        }

        #endregion

        #region Static Methods

        public static List<GeofenceItem> FromFile(string filePath)
        {
            var list = new List<GeofenceItem>();
            var lines = File.ReadAllLines(filePath);
            var geofence = new GeofenceItem();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    geofence = new GeofenceItem(line.TrimStart('[').TrimEnd(']'));
                    continue;
                }

                var coordinates = line.Replace(" ", null).Split(',');
                if (!double.TryParse(coordinates[0], out var lat))
                    continue;

                if (!double.TryParse(coordinates[1], out var lng))
                    continue;

                geofence.Polygons.Add(new Location(lat, lng));

                var isEnd = i == lines.Length - 1 || lines[i + 1].StartsWith("[", StringComparison.Ordinal);
                if (isEnd)
                {
                    list.Add(geofence);
                }
            }

            return list;
        }

        #endregion
    }
}