namespace WhMgr.Geofence
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using NetTopologySuite.Geometries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using WhMgr.Configuration;
    using WhMgr.Diagnostics;

    /// <summary>
    /// Geocoordinate location
    /// </summary>
    public class Location
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("LOCATION", Program.LogLevel);

        /// <summary>
        /// Gets or sets the address for the location
        /// </summary>
        public string Address { get; private set; }

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
        /// Gets the geolocation lookup of the current <seealso cref="Location"/> object.
        /// </summary>
        /// <param name="config">Config that includes the Google Maps key and OSM Nominatim endpoint</param>
        /// <returns>Returns a <seealso cref="Location"/> object containing the address</returns>
        public Location GetAddress(WhConfig config)
        {
            if (!string.IsNullOrEmpty(config.GoogleMapsKey))
                return GetGoogleAddress(City, Latitude, Longitude, config.GoogleMapsKey);

            if (!string.IsNullOrEmpty(config.NominatimEndpoint))
                return GetNominatimAddress(City, Latitude, Longitude, config.NominatimEndpoint);

            return null;
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

        /// <summary>
        /// Queries Google Maps geolocation lookup endpoint
        /// </summary>
        /// <param name="city">Geofence specific city to associate with the returned address</param>
        /// <param name="lat">Latitude to lookup</param>
        /// <param name="lng">Longitude to lookup</param>
        /// <param name="gmapsKey">Google Maps key</param>
        /// <returns></returns>
        private Location GetGoogleAddress(string city, double lat, double lng, string gmapsKey)
        {
            var apiKey = string.IsNullOrEmpty(gmapsKey) ? string.Empty : $"&key={gmapsKey}";
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&sensor=true{apiKey}";
            var unknown = "Unknown";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var data = reader.ReadToEnd();
                    var parseJson = JObject.Parse(data);
                    var status = Convert.ToString(parseJson["status"]);
                    if (string.Compare(status, "OK", true) != 0)
                        return null;

                    var result = parseJson["results"].FirstOrDefault();
                    var address = Convert.ToString(result["formatted_address"]);
                    //var area = Convert.ToString(result["address_components"][2]["long_name"]);
                    return new Location(address, city ?? unknown, lat, lng);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }

        /// <summary>
        /// Queries OpenStreetMaps Nominatim geolocation lookup endpoint
        /// </summary>
        /// <param name="city">Geofence specific city to associate with the returned address</param>
        /// <param name="lat">Latitude to lookup</param>
        /// <param name="lng">Longitude to lookup</param>
        /// <param name="endpoint">Nominatim endpoint</param>
        /// <returns></returns>
        private Location GetNominatimAddress(string city, double lat, double lng, string endpoint)
        {
            var unknown = "Unknown";
            var url = $"{endpoint}/reverse?format=jsonv2&lat={lat}&lon={lng}";
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    wc.Headers.Add("User-Agent", Strings.BotName);
                    var json = wc.DownloadString(url);
                    dynamic obj = JsonConvert.DeserializeObject(json);
                    return new Location(Convert.ToString(obj.display_name), city ?? unknown, Convert.ToDouble(obj.lat), Convert.ToDouble(obj.lon));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }
    }
}