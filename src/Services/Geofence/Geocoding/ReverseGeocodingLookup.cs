namespace WhMgr.Services.Geofence.Geocoding
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence.Geocoding.Cache;
    using WhMgr.Services.Geofence.Geocoding.Google;
    using WhMgr.Services.Geofence.Geocoding.Nominatim;

    public class ReverseGeocodingLookup : IReverseGeocodingLookup
    {
        // TODO: Rename to .cache
        private const string CacheFolder = "cache";

        private static readonly AddressMemoryCache _cache = new();
        private static IReverseGeocodingLookup _instance;

        #region Properties

        /// <summary>
        /// Gets a singleton instance of <seealso cref="ReverseGeocodingLookup"/>
        /// </summary>
        public static IReverseGeocodingLookup Instance =>
            _instance ??= new ReverseGeocodingLookup(Startup.Config.ReverseGeocoding);

        /// <summary>
        /// Gets or sets a value used for configuring reverse geocoding lookups.
        /// </summary>
        public ReverseGeocodingConfig Config { get; set; }

        #endregion

        public ReverseGeocodingLookup(ReverseGeocodingConfig config)
        {
            Config = config;
        }

        #region Public Methods

        /// <summary>
        /// Returns the location address from reverse geocoding lookup from geocoordinates.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public async Task<string> GetAddressAsync(Coordinate coord)
        {
            return Config.Provider switch
            {
                ReverseGeocodingProvider.GMaps => await GetGoogleAddress(coord),
                ReverseGeocodingProvider.Osm => await GetNominatimAddress(coord),
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
        private async Task<string> GetNominatimAddress(Coordinate coord, string format = "jsonv2")
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
                var sb = new StringBuilder();
                sb.Append(Config.Nominatim.Endpoint);
                sb.Append("/reverse");
                sb.Append($"?format={format}");
                sb.Append($"&lat={coord.Latitude}");
                sb.Append($"&lon={coord.Longitude}");
                var url = sb.ToString();
                try
                {
                    var json = await GetData(url);
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
        private async Task<string> GetGoogleAddress(Coordinate coord)
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
                var sb = new StringBuilder();
                sb.Append(Strings.GoogleMapsReverseGeocodingApiUrl);
                sb.Append($"?latlng={coord.ToString()}");
                sb.Append("&sensor=true");
                sb.Append($"&key={Config.GoogleMaps.Key}");
                var url = sb.ToString();
                try
                {
                    var json = await GetData(url);
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
        private static async Task<string> GetData(string url)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("UserAgent", $"{Strings.BotName} v{Strings.BotVersion}");
                var responseData = await client.GetStringAsync(url);
                return responseData;
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

            // Create cache folder for provider if it does not exist
            if (!Directory.Exists(providerFolder))
            {
                Directory.CreateDirectory(providerFolder);
            }

            // Write and serialize address data to disk if cache file does not exist
            var filePath = Path.Combine(providerFolder, fileName);
            if (!File.Exists(filePath))
            {
                var json = data.ToJson();
                using var sw = new StreamWriter(filePath, false, Encoding.UTF8, ushort.MaxValue);
                sw.WriteLine(json);
            }
        }

        private T LoadFromDisk<T>((double, double) key)
        {
            var (lat, lon) = key;
            var fileName = $"{lat},{lon}.json";
            var providerFolder = Path.Combine(
                CacheFolder,
                GetProviderCacheFolderName(Config.Provider)
            );

            // Read and deserialize address data from disk if cache file exists
            var filePath = Path.Combine(providerFolder, fileName);
            if (!File.Exists(filePath))
                return default;

            using var sr = new StreamReader(filePath, Encoding.UTF8);
            var json = sr.ReadToEnd();
            return json.FromJson<T>();
        }

        #endregion
    }
}
