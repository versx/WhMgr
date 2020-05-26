namespace WhMgr.Osm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Diagnostics;
    using WhMgr.Osm.Models;

    public class OsmManager
    {
        private const string NestFilePath = "";

        private static readonly IEventLogger _logger = EventLogger.GetLogger("OSM");

        public OsmFeatureCollection Nests { get; private set; }

        public OsmManager()
        {
            LoadNests();
        }

        private void LoadNests()
        {
            if (!File.Exists(Strings.OsmNestFilePath))
            {
                _logger.Warn($"{Strings.OsmNestFilePath} does not exist, failed to load nests.");
                return;
            }

            var data = File.ReadAllText(Strings.OsmNestFilePath);
            var obj = JsonConvert.DeserializeObject<OsmFeatureCollection>(data);
            if (obj == null)
            {
                _logger.Warn($"Failed to deserialize file data from {Strings.OsmNestFilePath} for nests collection.");
                return;
            }
            Nests = obj;
        }

        public List<OsmFeature> GetNest(string name)
        {
            if (Nests == null)
                return null;

            return Nests
                .Features?
                .Where(x => string.Compare(name, x?.Properties?.Name, true) == 0)?
                .ToList();
        }

        public static string MultiPolygonToLatLng(List<MultiPolygon> coordinates)
        {
            var sb = new System.Text.StringBuilder();
            //sb.Append("[");
            for (var i = 0; i < coordinates.Count; i++)
            {
                var multipolygon = coordinates[i];
                sb.Append("[");
                for (var j = 0; j < multipolygon.Count; j++)
                {
                    var polygon = multipolygon[j];
                    if (polygon.Count != 2)
                        continue;

                    var lng = polygon[0];
                    var lat = polygon[1];
                    sb.Append($"[{lat},{lng}]");
                    if (j != multipolygon.Count - 1)
                        sb.Append(",");
                }
                sb.Append("]");
                if (i != coordinates.Count - 1)
                    sb.Append(",");
            }
            //sb.Append("]");
            return sb.ToString();
        }

    }
}