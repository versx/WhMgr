using System;
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
        /// Creates a Feature containing a Polygon created using the provided Locations
        /// </summary>
        public static Feature LocationsToFeature(IEnumerable<Location> locations, IAttributesTable attributes = default)
        {
            var coordinateList = locations.Select(c => (Coordinate) c).ToList();

            if (coordinateList.Count < 3)
                throw new ArgumentException("At least three locations are required", nameof(locations));

            if (!coordinateList.First().Equals2D(coordinateList.Last(), double.Epsilon))
                // A closed linear ring requires the same point at the start and end of the list
                coordinateList.Add(coordinateList.First());

            var polygonRing = GeometryFactory.Default.CreateLinearRing(coordinateList.ToArray());
            var polygon = new Polygon(polygonRing);
            var feature = new Feature(polygon, attributes ?? new AttributesTable());

            return feature;
        }
    }
}