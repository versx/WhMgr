namespace WhMgr.Services.Geofence.Geocoding
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence.Geocoding.Cache;
    using WhMgr.Services.Geofence.Geocoding.Google;
    using WhMgr.Services.Geofence.Geocoding.Nominatim;

    public class ReverseGeocodingLookup : IReverseGeocodingLookup
    {
        private const string CacheFolder = "cache";

        private static readonly AddressMemoryCache _cache = new();
        private static IReverseGeocodingLookup _instance;

        #region Properties

        public static IReverseGeocodingLookup Instance =>
            _instance ??= new ReverseGeocodingLookup(Startup.Config.ReverseGeocoding);

        public ReverseGeocodingConfig Config { get; set; }

        #endregion

        public ReverseGeocodingLookup(ReverseGeocodingConfig config)
        {
            Config = config;
        }

        #region Public Methods

        public string GetAddress(Coordinate coord)
        {
            return Config.Provider switch
            {
                ReverseGeocodingProvider.GMaps => GetGoogleAddress(coord),
                ReverseGeocodingProvider.Osm => GetNominatimAddress(coord),
                _ => null,
            };
        }

        #endregion

        /// <summary>
        /// Queries OpenStreetMaps Nominatim geolocation lookup endpoint
        /// </summary>
        /// <param name="coord">Latitude and longitude coordinates</param>
        /// <param name="format"></param>
        /// <returns></returns>
        private string GetNominatimAddress(Coordinate coord, string format = "jsonv2")
        {
            if (string.IsNullOrEmpty(Config.Nominatim?.Endpoint))
            {
                return null;
            }

            var key = (coord.Latitude, coord.Longitude);
            NominatimReverseLookup data = null;
            if (Config.CacheToDisk)
            {
                data = LoadFromDisk<NominatimReverseLookup>(key);
            }
            else
            {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }
            }

            if (data == null)
            {
                var baseUrl = Config.Nominatim.Endpoint;
                var url = $"{baseUrl}/reverse?format={format}&lat={coord.Latitude}&lon={coord.Longitude}";
                try
                {
                    var json = GetData(url);
                    var obj = json.FromJson<NominatimReverseLookup>();
                    if (obj == null)
                    {
                        return null;
                    }
                    data = obj;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
            }

            var parsedAddress = TemplateRenderer.Parse(Config.Nominatim?.Schema, data);
            if (Config.CacheToDisk)
            {
                if (data != null)
                {
                    SaveToDisk(key, data);
                }
            }
            else
            {
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, parsedAddress);
                }
            }
            return parsedAddress;
        }

        /// <summary>
        /// Queries Google Maps geolocation lookup endpoint
        /// </summary>
        /// <param name="city">Geofence specific city to associate with the returned address</param>
        /// <param name="lat">Latitude to lookup</param>
        /// <param name="lng">Longitude to lookup</param>
        /// <param name="gmapsKey">Google Maps key</param>
        /// <returns></returns>
        private string GetGoogleAddress(Coordinate coord)
        {
            if (string.IsNullOrEmpty(Config.GoogleMaps?.Key))
            {
                return null;
            }

            var key = (coord.Latitude, coord.Longitude);
            GoogleReverseLookup data = null;
            if (Config.CacheToDisk)
            {
                data = LoadFromDisk<GoogleReverseLookup>(key);
            }
            else
            {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }
            }

            if (data == null)
            {
                var baseUrl = "https://maps.googleapis.com/maps/api/geocode/json";
                var url = $"{baseUrl}?latlng={coord.Latitude},{coord.Longitude}&sensor=true&key={Config.GoogleMaps.Key}";
                try
                {
                    var json = GetData(url);
                    var obj = json.FromJson<GoogleReverseLookup>();
                    if (string.Compare(obj.Status, "OK", true) != 0)
                        return null;

                    data = obj;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
            }

            var parsedAddress = TemplateRenderer.Parse(Config.GoogleMaps?.Schema, data);
            if (Config.CacheToDisk)
            {
                if (data != null)
                {
                    SaveToDisk(key, data);
                }
            }
            else
            {
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, parsedAddress);
                }
            }
            return parsedAddress;
        }

        /// <summary>
        /// Get raw json data from HTTP GET request to provided url address
        /// </summary>
        /// <param name="url">Url address</param>
        /// <returns>Returns json string of HTTP request</returns>
        private static string GetData(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers[HttpRequestHeader.UserAgent] = $"{Strings.BotName} v{Strings.BotVersion}";
                var response = request.GetResponse();
                using var responseStream = response.GetResponseStream();
                using var sr = new StreamReader(responseStream, Encoding.UTF8);
                var json = sr.ReadToEnd();
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return null;
        }

        #region Caching Methods

        private static string GetProviderCacheFolderName(ReverseGeocodingProvider provider)
        {
            return provider switch
            {
                ReverseGeocodingProvider.GMaps => "gmaps",
                ReverseGeocodingProvider.Osm => "osm",
                _ => null,
            };
        }

        private void SaveToDisk<T>((double, double) key, T data)
        {
            var (lat, lon) = key;
            var fileName = $"{lat},{lon}.json";
            var providerFolder = Path.Combine(
                CacheFolder,
                GetProviderCacheFolderName(Config.Provider)
            );
            if (!Directory.Exists(providerFolder))
            {
                Directory.CreateDirectory(providerFolder);
            }
            var filePath = Path.Combine(providerFolder, fileName);
            if (!File.Exists(filePath))
            {
                // If cache file does not exist, write data to disk
                using var sw = new StreamWriter(filePath, false, Encoding.UTF8, ushort.MaxValue);
                var json = data.ToJson();
                sw.WriteLine(json);
            }
        }

        public T LoadFromDisk<T>((double, double) key)
        {
            var (lat, lon) = key;
            var fileName = $"{lat},{lon}.json";
            var providerFolder = Path.Combine(
                CacheFolder,
                GetProviderCacheFolderName(Config.Provider)
            );
            var filePath = Path.Combine(providerFolder, fileName);
            if (File.Exists(filePath))
            {
                // If cache file exists, read data from disk
                using var sr = new StreamReader(filePath, Encoding.UTF8);
                var json = sr.ReadToEnd();
                return json.FromJson<T>();
            }
            return default;
        }

        #endregion
    }
}
