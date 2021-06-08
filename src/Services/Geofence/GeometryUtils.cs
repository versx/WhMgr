namespace WhMgr.Services.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetTopologySuite.Features;
    using NetTopologySuite.Geometries;
    using NetTopCoordinate = NetTopologySuite.Geometries.Coordinate;

    public class GeometryUtils
    {
        /// <summary>
        /// Creates a Feature containing a Polygon created using the provided Coordinates
        /// </summary>
        public static Feature CoordinatesToFeature(IEnumerable<Coordinate> locations, IAttributesTable attributes = default)
        {
            var coordinateList = locations.Select(c => (NetTopCoordinate)c).ToList();

            if (coordinateList?.Count < 3)
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