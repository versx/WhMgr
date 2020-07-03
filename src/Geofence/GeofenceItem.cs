namespace WhMgr.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Geofence class
    /// </summary>
    public class GeofenceItem
    {
        private const string DefaultName = "Unnamed";

        #region Properties

        /// <summary>
        /// Gets or sets the name of the geofence
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the location polygons of the geofence
        /// </summary>
        public List<Location> Polygons { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class
        /// </summary>
        public GeofenceItem()
        {
            Name = DefaultName;
            Polygons = new List<Location>();
        }

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class by name
        /// </summary>
        /// <param name="name">Name of geofence</param>
        public GeofenceItem(string name) : this(name ?? DefaultName, new List<Location>())
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class with name and polygons
        /// </summary>
        /// <param name="name">Name of geofence</param>
        /// <param name="polygons">Location polygons of geofence</param>
        public GeofenceItem(string name, List<Location> polygons) : this()
        {
            Name = name ?? DefaultName;
            Polygons = polygons;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Load a geofence from the provided file path
        /// </summary>
        /// <param name="filePath">File path of geofence to load</param>
        /// <returns>Returns a geofence object of the file path</returns>
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