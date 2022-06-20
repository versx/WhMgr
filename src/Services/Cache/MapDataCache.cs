namespace WhMgr.Services.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    //using Microsoft.Extensions.Logging;

    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Icons;
    using WhMgr.Services.Webhook.Models;

    public class MapDataCache : IMapDataCache
    {
        private readonly Microsoft.Extensions.Logging.ILogger<IMapDataCache> _logger;
        private readonly IDbContextFactory<MapDbContext> _dbFactory;

        private Dictionary<string, PokestopData> _pokestops;
        private Dictionary<string, GymDetailsData> _gyms;
        private Dictionary<long, WeatherData> _weather;

        public MapDataCache(
            Microsoft.Extensions.Logging.ILogger<IMapDataCache> logger,
            IDbContextFactory<MapDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        #region Pokestops

        /// <summary>
        /// Get Pokestop from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PokestopData> GetPokestop(string id)
        {
            if (string.IsNullOrEmpty(id) || string.Compare(id, "None", true) == 0)
            {
                return null;
            }
            if (_pokestops == null)
            {
                await LoadMapData().ConfigureAwait(false);
            }
            if (!(_pokestops?.ContainsKey(id) ?? false))
            {
                return null;
            }
            var pokestop = _pokestops[id];
            return pokestop;
        }

        public bool ContainsPokestop(string id) =>
            _pokestops?.ContainsKey(id) ?? false;

        public void UpdatePokestop(PokestopData pokestop)
        {
            if (ContainsPokestop(pokestop.FortId))
            {
                _pokestops[pokestop.FortId] = pokestop;
            }
            else
            {
                _pokestops.Add(pokestop.FortId, pokestop);
            }
        }

        #endregion

        #region Gyms

        /// <summary>
        /// Get Gym from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GymDetailsData> GetGym(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (_gyms == null)
            {
                await LoadMapData().ConfigureAwait(false);
            }
            if (!(_gyms?.ContainsKey(id) ?? false))
            {
                return null;
            }
            var gym = _gyms[id];
            return gym;
        }

        public bool ContainsGym(string id) =>
            _gyms?.ContainsKey(id) ?? false;

        public void UpdateGym(GymDetailsData gym)
        {
            if (ContainsGym(gym.FortId))
            {
                _gyms[gym.FortId] = gym;
            }
            else
            {
                _gyms.Add(gym.FortId, gym);
            }
        }

        #endregion

        #region Weather

        /// <summary>
        /// Get Weather from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WeatherData> GetWeather(long id)
        {
            if (id == 0)
            {
                return null;
            }
            if (_weather == null)
            {
                await LoadMapData().ConfigureAwait(false);
            }
            if (!(_weather?.ContainsKey(id) ?? false))
            {
                return null;
            }
            var weather = _weather[id];
            return weather;
        }

        public bool ContainsWeather(long id) =>
            _weather?.ContainsKey(id) ?? false;

        public void UpdateWeather(WeatherData weather)
        {
            if (ContainsWeather(weather.Id))
            {
                _weather[weather.Id] = weather;
            }
            else
            {
                _weather.Add(weather.Id, weather);
            }
        }

        #endregion

        /// <summary>
        /// Load pokestops gyms, and weather map data
        /// </summary>
        /// <returns></returns>
        private async Task LoadMapData()
        {
            if (_pokestops != null && _gyms != null && _weather != null)
            {
                // Already cached
                _logger.Debug($"Map data already cached");
                return;
            }

            using var ctx = _dbFactory.CreateDbContext();
            //_pokestops = await ctx.Pokestops.Include(pokestop => pokestop.Incidents).ToDictionaryAsync(key => key.PokestopId, value => value);
            _pokestops = await ctx.Pokestops.ToDictionaryAsync(key => key.FortId, value => value);
            _gyms = await ctx.Gyms.ToDictionaryAsync(key => key.FortId, value => value);
            _weather = await ctx.Weather.ToDictionaryAsync(key => key.Id, value => value);
        }

        public async Task<List<dynamic>> GetPokestopsNearby(double latitude, double longitude, double radiusM = 100)
        {
            if (_pokestops == null)
            {
                await LoadMapData().ConfigureAwait(false);
            }

            var nearby = _pokestops.Values.Where(stop => IsWithinRadius(stop.Latitude, stop.Longitude, latitude, longitude, radiusM))
                                          .Select(stop => new
            {
                id = stop.FortId,
                lat = stop.Latitude,
                lon = stop.Longitude,
                lure_id = Convert.ToInt32(stop.LureType),
                lure = stop.LureType,
                marker = //x.HasInvasion
                         //? UIconService.Instance.GetInvasionIcon("Default", stop.GruntType)
                    UIconService.Instance.GetPokestopIcon("Default", stop.LureType), // TODO: Get icon style
            }).ToList<dynamic>();
            return nearby;
        }

        public async Task<List<dynamic>> GetGymsNearby(double latitude, double longitude, double radiusM = 100)
        {
            if (_gyms == null)
            {
                await LoadMapData().ConfigureAwait(false);
            }

            var nearby = _gyms.Values.Where(gym => IsWithinRadius(gym.Latitude, gym.Longitude, latitude, longitude, radiusM))
                                     .Select(gym => new
            {
                id = gym.FortId,
                lat = gym.Latitude,
                lon = gym.Longitude,
                team_id = Convert.ToInt32(gym.Team),
                team = gym.Team,
                marker = UIconService.Instance.GetGymIcon("Default", gym.Team), // TODO: Get icon style
            }).ToList<dynamic>();
            return nearby;
        }

        private static bool IsWithinRadius(double lat, double lon, double mapLat, double mapLon, double radiusM = 100)
        {
            var coord = new Coordinate(lat, lon);
            var mapCoord = new Coordinate(mapLat, mapLon);
            var distance = coord.DistanceTo(mapCoord);
            var isWithinRadius = distance <= radiusM;
            return isWithinRadius;
        }
    }
}