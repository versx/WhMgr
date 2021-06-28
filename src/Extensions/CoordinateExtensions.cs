namespace WhMgr.Extensions
{
    using System;

    using WhMgr.Services.Geofence;

    public static class CoordinatesDistanceExtensions
    {
        /// <summary>
        ///     Returns the distance between the latitude and longitude coordinates that are specified by this Coordinate and
        ///     another specified Coordinate.
        /// </summary>
        /// <returns>
        ///     The distance between the two coordinates, in meters.
        /// </returns>
        /// <param name="from">The Coordinate for the location to calculate the distance from.</param>
        /// <param name="to">The Coordinate for the location to calculate the distance to.</param>
        public static double DistanceTo(this Coordinate from, Coordinate to)
        {
            if (double.IsNaN(from.Latitude) || double.IsNaN(from.Longitude) ||
                double.IsNaN(to.Latitude) || double.IsNaN(to.Longitude))
            {
                throw new ArgumentException("Argument latitude or longitude is not a number");
            }

            var d1 = from.Latitude * (Math.PI / 180.0);
            var num1 = from.Longitude * (Math.PI / 180.0);
            var d2 = to.Latitude * (Math.PI / 180.0);
            var num2 = to.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}