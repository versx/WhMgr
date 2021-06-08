namespace WhMgr.Services.Geofence
{
    using System.Text.Json.Serialization;

    using NetTopologySuite.Geometries;
    using NetTopCoordinate = NetTopologySuite.Geometries.Coordinate;

    public class Coordinate
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        public Coordinate(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public Coordinate(string city, double lat, double lon)
            : this(lat, lon)
        {
            City = city;
        }

        /// <summary>
        /// Returns the string representation of <see cref="Location"/> class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }

        public static implicit operator NetTopCoordinate(Coordinate location) =>
            new(location.Longitude, location.Latitude);

        public static implicit operator Point(Coordinate location) =>
            new(location.Longitude, location.Latitude);
    }
}
