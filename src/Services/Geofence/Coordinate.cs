namespace WhMgr.Services.Geofence
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Text.Json.Serialization;

    using NetTopologySuite.Geometries;
    using NetTopCoordinate = NetTopologySuite.Geometries.Coordinate;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence.Nominatim;

    public class LocationCache : Dictionary<(double, double), Coordinate>
    {
    }

    public class Coordinate
    {
        private static readonly LocationCache _cache = new();
        private readonly object _lock = new();

        #region Properties

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        #endregion

        #region Constructor(s)

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

        public Coordinate(string address, string city, double lat, double lon)
            : this(city, lat, lon)
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

        #region Reverse Geocoding Methods

        /// <summary>
        /// Gets the geolocation lookup of the current <seealso cref="Location"/> object.
        /// </summary>
        /// <param name="config">Config that includes the Google Maps key and OSM Nominatim endpoint</param>
        /// <returns>Returns a <seealso cref="Location"/> object containing the address</returns>
        public Coordinate GetAddress(Config config)
        {
            lock (_lock)
            {
                var key = (Latitude, Longitude);
                // Check if cache already contains lat/lon tuple key, if so return it.
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }

                // Check if we want any reverse geocoding address
                Coordinate location = null;
                if (!string.IsNullOrEmpty(config.GoogleMapsKey))
                {
                    location = GetGoogleAddress(City, Latitude, Longitude, config.GoogleMapsKey);
                }

                if (!string.IsNullOrEmpty(config.NominatimEndpoint))
                {
                    location = GetNominatimAddress(City, Latitude, Longitude, config.NominatimEndpoint, config.NominatimSchema);
                }

                // Check if lat/lon tuple key has not been cached already, if not add it.
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, location);
                }
                return location;
            }
        }

        /// <summary>
        /// Queries Google Maps geolocation lookup endpoint
        /// </summary>
        /// <param name="city">Geofence specific city to associate with the returned address</param>
        /// <param name="lat">Latitude to lookup</param>
        /// <param name="lng">Longitude to lookup</param>
        /// <param name="gmapsKey">Google Maps key</param>
        /// <returns></returns>
        public static Coordinate GetGoogleAddress(string city, double lat, double lon, string gmapsKey)
        {
            var apiKey = string.IsNullOrEmpty(gmapsKey) ? string.Empty : $"&key={gmapsKey}";
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lon}&sensor=true{apiKey}";
            var unknown = "Unknown";
            // TODO: Google reverse geocoding parse
            /*
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
                    return new Coordinate(address, city ?? unknown, lat, lng);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            */
            return null;
        }

        /// <summary>
        /// Queries OpenStreetMaps Nominatim geolocation lookup endpoint
        /// </summary>
        /// <param name="city">Geofence specific city to associate with the returned address</param>
        /// <param name="lat">Latitude to lookup</param>
        /// <param name="lng">Longitude to lookup</param>
        /// <param name="endpoint">Nominatim endpoint</param>
        /// <param name="nominatimSchema">Nominatim schema</param>
        /// <returns></returns>
        public static Coordinate GetNominatimAddress(string city, double lat, double lon, string endpoint, string nominatimSchema)
        {
            var unknown = "Unknown";
            var url = $"{endpoint}/reverse?format=jsonv2&lat={lat}&lon={lon}";
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    wc.Headers.Add("User-Agent", Strings.BotName);
                    var json = wc.DownloadString(url);
                    var obj = json.FromJson<NominatimReverseLookup>();
                    var parsedLocation = TemplateRenderer.Parse(nominatimSchema, obj);
                    return new Coordinate(parsedLocation, city ?? unknown, Convert.ToDouble(obj.Latitude), Convert.ToDouble(obj.Longitude));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return null;
        }

        #endregion
    }
}