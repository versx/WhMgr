namespace WhMgr.Services.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    using NetTopologySuite.Features;
    using NetTopologySuite.Geometries;

    using WhMgr.Extensions;

    public class Geofence
    {
        private const string DefaultName = "Unnamed";

        #region Properties

        /// <summary>
        /// Gets or sets the name of the geofence
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The filename from which this geofence originated
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets the FeatureCollection containing the geometry which represents this geofence
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the geometry representing the smallest possible bounding box which contains all points of this geofence
        /// </summary>
        public Geometry BBox { get; }

        /// <summary>
        /// Gets or sets the priority of this geofence. Higher-priority geofences will take precedence
        /// when determining which geofence a particular location falls within if it falls within multiple.
        /// </summary>
        public int Priority { get; set; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class by name
        /// </summary>
        /// <param name="name">Name of geofence</param>
        public Geofence(string name = default)
        {
            Name = name ?? DefaultName;
            Priority = 0;
            Feature = new Feature();
            BBox = Geometry.DefaultFactory.CreateEmpty(Dimension.False);
        }

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class from a GeoJSON feature.
        /// If the feature has a "name" attribute, this geofence's name will be set from that.
        /// </summary>
        public Geofence(IFeature feature)
        {
            Name = feature.Attributes["name"]?.ToString() ?? DefaultName;
            Feature = feature;
            BBox = feature.Geometry.Envelope;

            try
            {
                Priority = Convert.ToInt32(feature.Attributes["priority"]);
            }
            catch
            {
                Priority = 0;
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="GeofenceItem"/> class with name and polygons
        /// </summary>
        /// <param name="name">Name of geofence</param>
        /// <param name="coordinates">Location polygons of geofence</param>
        public Geofence(string name, List<Coordinate> coordinates) : this(name)
        {
            Feature = GeofenceUtils.CoordinatesToFeature(coordinates);
            BBox = Feature.Geometry.Envelope;
        }

        #endregion

        #region Static Methods

        public static List<Geofence> FromFile(string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                return FromJsonFile(filePath);
            else
                return FromIniFile(filePath);
        }

        private static List<Geofence> FromJsonFile(string filePath)
        {
            FeatureCollection featureCollection;
            // Read the JSON from the file and deserialize it into a GeoJSON FeatureCollection
            var jsonText = File.ReadAllText(filePath);
            /*
            var serializer = GeoJsonSerializer.Create();
            using (var stringReader = new StringReader(jsonText))
            using (var jsonReader = new Utf8JsonReader(stringReader))
                featureCollection = serializer.Deserialize<FeatureCollection>(jsonReader);
            */
            featureCollection = jsonText.FromJson<FeatureCollection>();

            if (featureCollection == null)
                throw new JsonException($"Geofence file \"{filePath}\" contained invalid JSON or the JSON did not represent a FeatureCollection");

            // Turn each Feature in the FeatureCollection into a GeofenceItem
            return featureCollection.Select(feature => new Geofence(feature)
            {
                Filename = Path.GetFileName(filePath)
            }).ToList();
        }

        /// <summary>
        /// Load a geofence from the provided INI file path
        /// </summary>
        /// <param name="filePath">File path of geofence to load</param>
        /// <returns>Returns a geofence object of the file path</returns>
        private static List<Geofence> FromIniFile(string filePath)
        {
            var list = new List<Geofence>();
            // Read all lines of the file and remove all null, empty, or whitespace lines
            var lines = File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            var locations = new List<Coordinate>();
            string name = "";

            // Loop through each new line of the file
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // If the line starts with '[' then parse the Geofence name
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    name = line.TrimStart('[').TrimEnd(']');
                    continue;
                }

                // Trim off any extra whitespace and split the line by a comma ','
                var coordinates = line.Trim('\0').Split(',');
                if (!double.TryParse(coordinates[0], out var lat))
                    continue;

                if (!double.TryParse(coordinates[1], out var lng))
                    continue;

                locations.Add(new Coordinate(lat, lng));

                // If we have reached the end of the file or the start of another
                // geofence, add the current to the list of geofences
                var isEnd = i == lines.Length - 1 || lines[i + 1].StartsWith("[", StringComparison.Ordinal);

                if (isEnd)
                {
                    var geofence = new Geofence(name, locations)
                    {
                        Filename = Path.GetFileName(filePath)
                    };

                    list.Add(geofence);
                    name = "";
                    locations = new List<Coordinate>();
                }
            }

            return list;
        }

        #endregion
    }
}