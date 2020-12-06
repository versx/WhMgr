using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Location = WhMgr.Geofence.Location;

namespace WhMgr.Utilities
{
    public class GeoUtils
    {
        /// <summary>
        /// Creates a Feature containing a Polygon created using the provided coordinates
        /// </summary>
        public static Feature CoordinateListToFeature(IEnumerable<Location> coordinates, IAttributesTable attributes = default)
        {
            var polygonRing = GeometryFactory.Default.CreateLinearRing(coordinates.Select(c => (Coordinate) c).ToArray());
            var polygon     = new Polygon(polygonRing);
            var feature     = new Feature(polygon, attributes ?? new AttributesTable());

            return feature;
        }
    }
}