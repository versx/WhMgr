using NetTopologySuite.Geometries;

namespace WhMgr.Geofence
{
    /// <summary>
    /// Geocoordinate location
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets the address for the location
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets the city of the address
        /// </summary>
        public string City { get; }

        /// <summary>
        /// Gets the geocoordinate latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the geocoordinate longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Instantiates a new <see cref="Location"/> class
        /// </summary>
        /// <param name="lat">Geocoordinate latitude</param>
        /// <param name="lng">Geocoordinate longitude</param>
        public Location(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }

        /// <summary>
        /// Instantiates a new <see cref="Location"/> class
        /// </summary>
        /// <param name="address">Address of geocoordinates</param>
        /// <param name="city">City of address</param>
        /// <param name="lat">Geocoordinate latitude</param>
        /// <param name="lng">Geocoordinate longitude</param>
        public Location(string address, string city, double lat, double lng)
        {
            Address = address;
            City = city;
            Latitude = lat;
            Longitude = lng;
        }

        /// <summary>
        /// Returns the string representation of <see cref="Location"/> class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }

        public static implicit operator Coordinate(Location location) => new Coordinate(location.Longitude, location.Latitude);

        public static implicit operator Point(Location location) => new Point(location.Longitude, location.Latitude);
    }
}