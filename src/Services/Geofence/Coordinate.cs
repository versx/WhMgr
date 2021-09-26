namespace WhMgr.Services.Geofence
{
    using System.Text.Json.Serialization;

    using NetTopologySuite.Geometries;
    using NetTopCoordinate = NetTopologySuite.Geometries.Coordinate;

    public class Coordinate
    {
        #region Properties

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        #endregion

        #region Constructor(s)

        public Coordinate(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public Coordinate(string address, double lat, double lon)
            : this(lat, lon)
        {
            Address = address;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the string representation of <see cref="Location"/> class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }

        #region Operator Overrides

        public static implicit operator NetTopCoordinate(Coordinate location) =>
            new(location.Longitude, location.Latitude);

        public static implicit operator Point(Coordinate location) =>
            new(location.Longitude, location.Latitude);

        #endregion

        #endregion
    }
}