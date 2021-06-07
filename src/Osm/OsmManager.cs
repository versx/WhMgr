namespace WhMgr.Osm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using WhMgr.Extensions;
    using WhMgr.Osm.Models;

    public class OsmManager
    {
        public OsmFeatureCollection Nests { get; private set; }

        public OsmManager()
        {
            LoadNests();
        }

        private void LoadNests()
        {
            if (!File.Exists(Strings.OsmNestFilePath))
            {
                Console.WriteLine($"{Strings.OsmNestFilePath} does not exist, failed to load nests.");
                return;
            }

            var data = File.ReadAllText(Strings.OsmNestFilePath);
            var obj = data.FromJson<OsmFeatureCollection>();
            if (obj == null)
            {
                Console.WriteLine($"Failed to deserialize file data from {Strings.OsmNestFilePath} for nests collection.");
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

        public static string MultiPolygonToLatLng(List<MultiPolygon> coordinates, bool reverseCoordinates)
        {
            var sb = new System.Text.StringBuilder();
            //sb.Append("[");
            for (var i = 0; i < coordinates.Count; i++)
            {
                var multipolygon = coordinates[i];
                sb.Append('[');
                for (var j = 0; j < multipolygon.Count; j++)
                {
                    var polygon = multipolygon[j];
                    if (polygon.Count != 2)
                        continue;

                    var lat = polygon[0];
                    var lng = polygon[1];
                    if (reverseCoordinates)
                        sb.Append($"[{lng},{lat}]");
                    else
                        sb.Append($"[{lat},{lng}]");

                    if (j != multipolygon.Count - 1)
                        sb.Append(',');
                }
                sb.Append(']');
                if (i != coordinates.Count - 1)
                    sb.Append(',');
            }
            //sb.Append("]");
            return sb.ToString();
        }
    }
}