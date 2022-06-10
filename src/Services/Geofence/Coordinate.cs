namespace WhMgr.Services.Geofence
{
    using System.Text.Json.Serialization;

    using NetTopologySuite.Geometries;
    using NetTopCoordinate = NetTopologySuite.Geometries.Coordinate;

    using WhMgr.Services.Webhook.Models;

    public class Coordinate
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        public Coordinate(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public Coordinate(IWebhookPoint location)
        {
            Latitude = location.Latitude;
            Longitude = location.Longitude;
        }

        /// <summary>
        /// Returns the string representation of <see cref="Location"/> class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }

        #region Operator Overrides

        public static implicit operator NetTopCoordinate(Coordinate location) =>
            new(location.Longitude, location.Latitude);

        public static implicit operator Point(Coordinate location) =>
            new(location.Longitude, location.Latitude);

        #endregion
    }
}