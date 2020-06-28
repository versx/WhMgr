namespace WhMgr.Geofence
{
    /// <summary>
    /// Geocoordinate location
    /// </summary>
    public class Location
    {
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
        /// Returns the string representation of <see cref="Location"/> class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}